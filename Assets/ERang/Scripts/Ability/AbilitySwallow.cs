using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;

namespace ERang
{
    public class AbilitySwallow : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Swallow;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public CardSelect cardSelectObject;

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // cardAbility.abilityValue 만큼 유저가 핸드 카드를 뽑을때까지 대기
            // BattleLogic.Instance.ShowCardSelect(cardAbility.abilityValue);
            cardSelectObject.gameObject.SetActive(true);

            cardSelectObject.SetMaxSelectCardCount(cardAbility.abilityValue);
            cardSelectObject.DrawCards(Deck.Instance.HandCards);

            bool isCardSelectComplete = false;
            List<SelectCard> selectCards = null;

            UnityAction<List<SelectCard>> onCardSelectComplete = (cards) =>
            {
                selectCards = cards;
                isCardSelectComplete = true;
            };

            // 선택한 카드를 받을 UnityAction 구독
            cardSelectObject.OnCardSelectComplete += onCardSelectComplete;

            // 카드 선택까지 대기
            yield return new WaitUntil(() => isCardSelectComplete);

            Debug.Log($"Select Cards: {string.Join(", ", selectCards.Select(selectCard => selectCard.Card.Id))}");

            AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);
            // 해당 어빌리티에 연결되어 있는 어빌리티
            AbilityData chainAbilityData = AbilityData.GetAbilityData(abilityData.chainAbilityId);

            if (chainAbilityData == null)
                Debug.LogError($"{chainAbilityData.LogText} {Utils.RedText("테이블 데이터 없음")} - AbilitySwallow: ApplySingle");

            foreach (SelectCard selectCard in selectCards)
            {
                CardAbility handCardAbility = new()
                {
                    abilityId = chainAbilityData.abilityId,
                    aiDataId = cardAbility.aiDataId,
                    aiType = cardAbility.aiType,
                    abilityType = chainAbilityData.abilityType,
                    abilityValue = chainAbilityData.value,
                    workType = chainAbilityData.workType,
                };

                // 선택한 카드 핸드 어빌리티 설정
                // 1. 다음 턴에 핸드덱으로 선택되게
                selectCard.Card.Traits = CardTraits.NextTurnSelect;
                // 2. 사용 마나 감소 효과 적용
                // 3. 사용되거나 그레이브덱으로 이동하면 마나 감소 효과 제거
                selectCard.Card.AddHandCardAbility(handCardAbility);

                // 핸드 카드 덱으로 이동
                Deck.Instance.HandCardToDeck(selectCard);
            }

            // 구독 해제
            cardSelectObject.OnCardSelectComplete -= onCardSelectComplete;
            // 선택한 카드 Deck 으로 이동
            yield return StartCoroutine(cardSelectObject.CloseCardSelect());
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}