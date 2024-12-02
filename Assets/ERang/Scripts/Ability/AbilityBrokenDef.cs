using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityBrokenDef : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.BrokenDef;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            foreach (BSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    Changes.Add((StatType.Def, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                    continue;
                }

                int before = targetSlot.Card.Def;
                int change = abilityData.value;

                targetSlot.DecreaseDefense(change);

                Changes.Add((StatType.Def, true, targetSlot.SlotNum, targetSlot.Card.Id, targetSlot.SlotCardType, before, targetSlot.Card.Def, change));
            }

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            AbilityData abilityData = AbilityData.GetAbilityData(ability.abilityId);
            BaseCard card = targetSlot.Card;

            if (card == null || abilityData == null)
                yield break;

            int before = card.Def;
            int amount = abilityData.value;

            card.IncreaseDefense(amount);

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, amount));

            yield return new WaitForSeconds(0.1f);
        }
    }
}