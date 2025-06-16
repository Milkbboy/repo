using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;

namespace ERang
{
    public class AbilitySwallow : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Swallow;

        [Header("카드 선택 UI")]
        public CardSelect cardSelectObject;

        private List<SelectCard> selectedCards; // 결과 저장 변수 추가

        public override IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            int selectCount = cardAbility.abilityValue;
            if (selectCount <= 0)
            {
                LogAbility($"잘못된 선택 카드 수: {selectCount}", LogType.Warning);
                yield break;
            }

            if (cardSelectObject == null)
            {
                LogAbility("CardSelect 오브젝트가 할당되지 않았습니다.", LogType.Error);
                yield break;
            }

            if (DeckManager.Instance?.Data.HandCards == null || DeckManager.Instance.Data.HandCards.Count == 0)
            {
                LogAbility("선택할 핸드 카드가 없습니다.", LogType.Warning);
                yield break;
            }

            LogAbility($"카드 삼키기 시작 - 선택할 카드 수: {selectCount}");

            // 카드 선택 UI 활성화
            yield return StartCoroutine(ShowCardSelection(selectCount));

            // 코루틴 실행 대기
            yield return StartCoroutine(WaitForCardSelection());

            if (selectedCards == null || selectedCards.Count == 0)
            {
                LogAbility("카드 선택이 취소되었습니다.", LogType.Warning);
                yield break;
            }

            LogAbility($"선택된 카드: {string.Join(", ", selectedCards.Select(card => card.Card.Id))}");

            // 연쇄 어빌리티 적용
            yield return StartCoroutine(ApplyChainAbilities(cardAbility, selectedCards));

            // 카드 선택 UI 종료
            yield return StartCoroutine(cardSelectObject.CloseCardSelect());
            LogAbility("카드 삼키기 완료");
        }

        public override IEnumerator Release(CardAbility ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 삼키기는 해제되지 않는 즉시 효과
            LogAbility("삼키기는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }

        private IEnumerator ShowCardSelection(int selectCount)
        {
            cardSelectObject.gameObject.SetActive(true);
            cardSelectObject.SetMaxSelectCardCount(selectCount);
            cardSelectObject.DrawCards(DeckManager.Instance.Data.HandCards);

            LogAbility($"카드 선택 UI 활성화 - 최대 선택 수: {selectCount}");
            yield return new WaitForSeconds(0.1f); // UI 초기화 대기
        }

        private IEnumerator WaitForCardSelection()
        {
            bool isCardSelectComplete = false;
            selectedCards = null; // 초기화

            // 콜백 함수 정의
            UnityAction<List<SelectCard>> onCardSelectComplete = (cards) =>
            {
                selectedCards = cards;
                isCardSelectComplete = true;
                LogAbility($"카드 선택 완료: {cards.Count}장");
            };

            try
            {
                // 이벤트 구독
                cardSelectObject.OnCardSelectComplete += onCardSelectComplete;
                // 카드 선택 완료까지 대기
                yield return new WaitUntil(() => isCardSelectComplete);
            }
            finally
            {
                // 이벤트 구독 해제 (안전성 보장)
                cardSelectObject.OnCardSelectComplete -= onCardSelectComplete;
            }
        }

        private IEnumerator ApplyChainAbilities(CardAbility originalAbility, List<SelectCard> selectedCards)
        {
            // 연쇄 어빌리티 데이터 가져오기
            var abilityData = AbilityData.GetAbilityData(originalAbility.abilityId);
            if (abilityData == null)
            {
                LogAbility("원본 어빌리티 데이터를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            var chainAbilityData = AbilityData.GetAbilityData(abilityData.chainAbilityId);
            if (chainAbilityData == null)
            {
                LogAbility($"연쇄 어빌리티 데이터를 찾을 수 없습니다. ID: {abilityData.chainAbilityId}", LogType.Error);
                yield break;
            }

            LogAbility($"연쇄 어빌리티 적용: {chainAbilityData.ToAbilityLogInfo()}, selectedCards: {string.Join(", ", selectedCards.Select(card => card.Card.ToCardLogInfo()))}");

            foreach (SelectCard selectCard in selectedCards)
            {
                BaseCard card = selectCard?.Card;

                if (card == null) continue;

                // 연쇄 어빌리티 생성
                var handCardAbility = new CardAbility
                {
                    abilityId = chainAbilityData.abilityId,
                    aiDataId = originalAbility.aiDataId,
                    selfSlotNum = originalAbility.selfSlotNum,
                    targetSlotNum = originalAbility.targetSlotNum,

                    aiType = originalAbility.aiType,
                    abilityType = chainAbilityData.abilityType,
                    abilityValue = chainAbilityData.value,
                    workType = chainAbilityData.workType,
                    duration = chainAbilityData.duration,
                    abilityName = chainAbilityData.nameDesc,
                    cardId = card.Id
                };

                LogAbility($"다음 턴 선택 설정: {card.ToCardLogInfo()}");

                // 선택된 카드에 효과 적용
                // 1. 다음 턴에 핸드로 선택되도록 설정
                card.SetCardTraits(CardTraits.NextTurnSelect);

                // 2. 핸드 어빌리티 추가 (주로 마나 감소 효과)
                card.AbilitySystem.AddHandCardAbility(handCardAbility);

                LogAbility($"카드 처리 완료: {card.ToCardLogInfo()}");

                // 카드를 덱으로 이동
                DeckManager.Instance.HandCardToDeck(card);
            }
        }
    }
}