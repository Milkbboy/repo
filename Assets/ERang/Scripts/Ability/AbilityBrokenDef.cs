using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityBrokenDef : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.BrokenDef;
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

                int before = targetSlot.Card.def;
                int change = abilityData.value * -1;

                targetSlot.AddCardDef(change);

                Changes.Add((true, targetSlot.Slot, targetSlot.Card.Id, targetSlot.CardType, before, targetSlot.Card.def, change));
            }

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            Card card = targetSlot.Card;

            if (card == null)
                yield break;

            int before = card.def;
            int change = ability.abilityValue;

            targetSlot.AddCardDef(change);

            Changes.Add((true, targetSlot.Slot, card.Id, targetSlot.CardType, before, card.def, change));

            yield return new WaitForSeconds(0.1f);
        }
    }
}