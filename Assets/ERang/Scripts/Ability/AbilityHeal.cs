using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityHeal : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Heal;
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            foreach (BSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null || targetSlot.Card is not CreatureCard)
                {
                    Changes.Add((false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                    continue;
                }

                CreatureCard creatureCard = targetSlot.Card as CreatureCard;

                int before = creatureCard.Hp;

                creatureCard.RestoreHealth(abilityData.value);

                Changes.Add((true, targetSlot.SlotNum, targetSlot.Card.Id, targetSlot.SlotCardType, before, creatureCard.Hp, abilityData.value));
            }

            yield return new WaitForSeconds(0.1f);
        }

        // 즉시 효과는 해제 불필요
        public IEnumerator Release(Ability ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}