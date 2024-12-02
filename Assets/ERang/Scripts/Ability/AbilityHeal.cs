using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityHeal : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Heal;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            foreach (BSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    Changes.Add((StatType.Hp, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                    continue;
                }

                int before = targetSlot.Card.Hp;

                targetSlot.RestoreHealth(abilityData.value);

                Changes.Add((StatType.Hp, true, targetSlot.SlotNum, targetSlot.Card.Id, targetSlot.SlotCardType, before, targetSlot.Card.Hp, abilityData.value));
            }

            yield return new WaitForSeconds(0.1f);
        }

        // 즉시 효과는 해제 불필요
        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}