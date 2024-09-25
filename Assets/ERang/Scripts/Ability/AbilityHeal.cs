using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityHeal : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType { get { return AbilityType.Heal; } }
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    Changes.Add((false, targetSlot.Slot, 0, targetSlot.CardType, 0, 0, 0));
                    continue;
                }

                int before = targetSlot.Card.hp;

                targetSlot.AddCardHp(abilityData.value);

                Changes.Add((true, targetSlot.Slot, targetSlot.Card.Id, targetSlot.CardType, before, targetSlot.Card.hp, abilityData.value));
            }

            yield return new WaitForSeconds(0.1f);
        }

        // 즉시 효과는 해제 불필요
        public IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield break;
        }
    }
}