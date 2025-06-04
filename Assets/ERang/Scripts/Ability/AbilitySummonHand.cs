using System.Collections;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilitySummonHand : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.SummonHand;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(SummonCardToLocation(cardAbility, DeckKind.Hand));
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 소환은 해제되지 않는 즉시 효과
            LogAbility("소환은 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }

        private IEnumerator SummonCardToLocation(CardAbility cardAbility, DeckKind targetDeck)
        {
            AbilityData abilityData = Utils.CheckData(AbilityData.GetAbilityData, "AbilityData", cardAbility.abilityId);
            if (abilityData == null)
            {
                LogAbility("AbilityData를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            int summonGroupId = abilityData.summonGroupId;
            if (summonGroupId <= 0)
            {
                LogAbility($"잘못된 소환 그룹 ID: {summonGroupId}", LogType.Warning);
                yield break;
            }

            // 소환할 카드 선택
            int cardId = SummonData.PickUpCard(summonGroupId);
            if (cardId == 0)
            {
                LogAbility($"소환 그룹 {summonGroupId}에서 카드 뽑기 실패. 확률을 확인해주세요.", LogType.Error);
                yield break;
            }

            // 카드 이름 가져오기 (로그용)
            var cardData = Utils.CheckData(CardData.GetCardData, "CardData", cardId);
            string cardName = cardData?.nameDesc ?? $"카드 ID {cardId}";

            LogAbility($"카드 소환: {cardName} -> {targetDeck}");

            // 핸드덱에 카드 소환
            if (HandDeck.Instance != null)
            {
                yield return StartCoroutine(HandDeck.Instance.SummonCardToDeck(cardId, targetDeck));
                LogAbility($"소환 완료: {cardName}");
            }
            else
            {
                LogAbility("HandDeck 인스턴스가 없습니다.", LogType.Error);
            }
        }
    }
}