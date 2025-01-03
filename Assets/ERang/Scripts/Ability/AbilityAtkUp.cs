using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityAtkUp : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AtkUp;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            foreach (BSlot targetSlot in targetSlots)
            {
                BaseCard card = targetSlot.Card;

                if (card == null || card is not CreatureCard)
                {
                    Changes.Add((StatType.Atk, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                    continue;
                }

                CreatureCard creatureCard = card as CreatureCard;

                int before = creatureCard.Atk;
                int change = abilityData.value;

                creatureCard.IncreaseAttack(change);

                Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, creatureCard.Atk, change));
            }

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator ApplySingle(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null || card is not CreatureCard)
            {
                Changes.Add((StatType.Atk, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                yield break;
            }

            CreatureCard creatureCard = card as CreatureCard;

            int before = creatureCard.Atk;
            int change = abilityData.value;

            creatureCard.IncreaseAttack(change);

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, creatureCard.Atk, change));

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;
            AbilityData abilityData = AbilityData.GetAbilityData(ability.abilityId);

            if (card == null || card is not CreatureCard || abilityData == null)
                yield break;

            CreatureCard creatureCard = card as CreatureCard;

            int before = creatureCard.Atk;
            int amount = abilityData.value * -1;

            creatureCard.IncreaseAttack(amount);

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, creatureCard.Atk, amount));

            yield return new WaitForSeconds(0.1f);
        }
    }
}