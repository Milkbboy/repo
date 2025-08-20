using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;

namespace ERang
{
    public class AbilitySummonDrawDeckSelect : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.SummonDrawDeckSelect;

        [Header("카드 선택 UI")]
        public CardSelect cardSelectObject;

        private List<SelectCard> selectedCards;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (cardSelectObject == null)
            {
                LogAbility("CardSelect 오브젝트가 할당되지 않았습니다.", LogType.Error);
                yield break;
            }

            AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);
            if (abilityData == null)
            {
                LogAbility($"AbilityData({cardAbility.abilityId})를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            int summonGroupId = abilityData.summonGroupId;
            if (summonGroupId <= 0)
            {
                LogAbility($"잘못된 소환 그룹 ID: {summonGroupId}", LogType.Warning);
                yield break;
            }

            List<int> cardIds = SummonData.PickUpCard(summonGroupId, abilityData.value2);
            if (cardIds.Count == 0)
            {
                LogAbility($"드로우덱 카드 소환 그룹 {summonGroupId}에서 카드 뽑기 실패. 확률을 확인해주세요.", LogType.Error);
                yield break;
            }
            else
            {
                LogAbility($"드로우덱 카드 소환 그룹 {summonGroupId}에서 {string.Join(",", cardIds)}");
            }

            List<BaseCard> cards = new();
            CardFactory cardFactory = new(AiLogic.Instance);

            foreach (int cardId in cardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);
                if (cardData == null)
                {
                    LogAbility($"CardData({cardId})를 찾을 수 없습니다.", LogType.Error);
                    continue;
                }

                BaseCard card = cardFactory.CreateCard(cardData);

                if (card is MagicCard magicCard)
                    magicCard.SetSelectAttackType(AiLogic.Instance.IsSelectAttackType(card));

                cards.Add(card);
            }

            cardSelectObject.gameObject.SetActive(true);
            cardSelectObject.SetMaxSelectCardCount(abilityData.value);
            cardSelectObject.DrawCards(cards);

            yield return StartCoroutine(WaitForCardSelection());

            if (selectedCards == null || selectedCards.Count == 0)
            {
                LogAbility("카드 선택이 취소되었습니다.", LogType.Warning);
                yield break;
            }

            // 카드 선택 UI 종료
            cardSelectObject.gameObject.SetActive(false);

            // 선택한 카드 드로우덱으로 이동
            foreach (SelectCard selectCard in selectedCards)
            {
                yield return StartCoroutine(HandDeck.Instance.SummonCardToDeck(selectCard.Card.Id, DeckKind.Deck));
                LogAbility($"드로우덱으로 소환 완료: {selectCard.Card.Name}");
            }

            cardSelectObject.OnConfirmClickCoroutine();

            LogAbility("선택 카드 DrawDeck 이동 완료");
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

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 소환은 해제되지 않는 즉시 효과
            LogAbility("소환은 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}
