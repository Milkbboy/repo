using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilitySummonHand : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.SummonHand;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AbilityData abilityData = Utils.CheckData(AbilityData.GetAbilityData, "AbilityData", cardAbility.abilityId);

            if (abilityData == null)
                yield break;

            // 핸드로 카드 소환
            int summonGroupId = abilityData.summonGroupId;

            int cardId = SummonData.PickUpCard(summonGroupId);

            if (cardId == 0)
            {
                Debug.LogError($"{AbilityType.SummonHand} SummonGroupId: {summonGroupId} 에 해당하는 카드 뽑기 실패. 확률을 확인해 주세요.");
                yield break;
            }

            // 뽑은 카드 만들어서 핸드에 추가
            yield return StartCoroutine(HandDeck.Instance.SummonCardToDeck(cardId, DeckKind.Hand));
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}