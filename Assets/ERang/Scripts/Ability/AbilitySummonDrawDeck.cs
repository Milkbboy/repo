using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilitySummonDrawDeck : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.SummonDrawDeck;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            // 핸드로 카드 소환
            int summonGroupId = abilityData.summonGroupId;

            int cardId = SummonData.PickUpCard(summonGroupId);

            if (cardId == 0)
            {
                Debug.LogError($"SummonGroupId: {summonGroupId} 에 해당하는 카드 뽑기 실패. 확률을 확인해 주세요.");
                yield break;
            }

            // 카드가 만들어져서 해당 덱으로 이동하는걸 만들어야 겠네
            // 뽑은 카드 만들어서 핸드에 추가
            HandDeck.Instance.SummonCardToDeck(cardId, DeckKind.Deck);
            yield break;
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}