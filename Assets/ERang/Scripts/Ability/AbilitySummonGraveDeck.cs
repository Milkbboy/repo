using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 랜덤으로 뽑힌 카드 무덤 덱으로 이동
    /// </summary>
    public class AbilitySummonGraveDeck : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.SummonGraveDeck;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AbilityData abilityData = Utils.CheckData(AbilityData.GetAbilityData, "AbilityData", cardAbility.abilityId);

            if (abilityData == null)
                yield break;

            int summonGroupId = abilityData.summonGroupId;

            int cardId = SummonData.PickUpCard(summonGroupId);

            if (cardId == 0)
            {
                Debug.LogError($"{AbilityType.SummonGraveDeck} SummonGroupId: {summonGroupId} 에 해당하는 카드 뽑기 실패. 확률을 확인해 주세요.");
                yield break;
            }

            // 뽑은 카드 만들어서 그레이브에 추가
            yield return StartCoroutine(HandDeck.Instance.SummonCardToDeck(cardId, DeckKind.Grave));
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}