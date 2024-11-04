using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityAtkUp : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AtkUp;
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

                int before = creatureCard.Atk;
                int change = abilityData.value;

                creatureCard.IncreaseAttack(change);

                Changes.Add((true, targetSlot.SlotNum, targetSlot.Card.Id, targetSlot.SlotCardType, before, creatureCard.Atk, change));
            }

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

            Changes.Add((true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, creatureCard.Atk, amount));

            yield return new WaitForSeconds(0.1f);
        }
    }
}