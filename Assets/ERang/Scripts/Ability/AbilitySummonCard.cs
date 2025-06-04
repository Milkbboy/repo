using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public abstract class BaseSummonAbility : BaseAbility
    {
        protected abstract DeckKind TargetDeck { get; }

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
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

            LogAbility($"카드 소환: {cardName} -> {TargetDeck}");

            // 핸드덱에 카드 소환
            if (HandDeck.Instance != null)
            {
                yield return StartCoroutine(HandDeck.Instance.SummonCardToDeck(cardId, TargetDeck));
                LogAbility($"소환 완료: {cardName}");
            }
            else
            {
                LogAbility("HandDeck 인스턴스가 없습니다.", LogType.Error);
            }
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 소환은 해제되지 않는 즉시 효과
            LogAbility("소환은 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }

    public class AbilitySummonHand : BaseSummonAbility
    {
        public override AbilityType AbilityType => AbilityType.SummonHand;
        protected override DeckKind TargetDeck => DeckKind.Hand;
    }

    public class AbilitySummonDrawDeck : BaseSummonAbility
    {
        public override AbilityType AbilityType => AbilityType.SummonDrawDeck;
        protected override DeckKind TargetDeck => DeckKind.Deck;
    }

    public class AbilitySummonGraveDeck : BaseSummonAbility
    {
        public override AbilityType AbilityType => AbilityType.SummonGraveDeck;
        protected override DeckKind TargetDeck => DeckKind.Grave;
    }
}
