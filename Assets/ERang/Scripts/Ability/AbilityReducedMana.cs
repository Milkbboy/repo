using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityReducedMana : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.ReducedMana;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(BaseCard card)
        {
            Debug.Log("<color=red>---- ReducedMana ----</color> 마나 감소!!!");

            Apply(card, true);

            yield break;
        }

        public IEnumerator Release(BaseCard card)
        {
            Debug.Log("<color=red>---- ReducedMana ----</color> 감소된 마나 복구!!!");

            Apply(card, false);

            yield break;
        }

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }

        private void Apply(BaseCard card, bool isReduced)
        {
            foreach (CardAbility cardAbility in card.AbilitySystem.HandAbilities)
            {
                if (card is CreatureCard creatureCard)
                {
                    Debug.Log("크리처 카드!!!!");

                    if (isReduced)
                        creatureCard.DecreaseMana(cardAbility.abilityValue);
                    else
                        creatureCard.IncreaseMana(cardAbility.abilityValue);
                }

                if (card is MagicCard magicCard)
                {
                    Debug.Log("마법 카드!!!!");

                    if (isReduced)
                        magicCard.DecreaseMana(cardAbility.abilityValue);
                    else
                        magicCard.IncreaseMana(cardAbility.abilityValue);
                }
            }
        }
    }
}