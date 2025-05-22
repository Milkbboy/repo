using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityReducedMana : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.ReducedMana;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(GameCard card)
        {
            Debug.Log("<color=red>---- ReducedMana ----</color> 마나 감소!!!");

            Apply(card, true);

            yield break;
        }

        public IEnumerator Release(GameCard card)
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

        private void Apply(GameCard card, bool isReduced)
        {
            foreach (CardAbility cardAbility in card.AbilitySystem.HandAbilities)
            {
                if (card.CardType == CardType.Creature)
                {
                    Debug.Log("크리처 카드!!!!");

                    if (isReduced)
                        card.DecreaseMana(cardAbility.abilityValue);
                    else
                        card.IncreaseMana(cardAbility.abilityValue);
                }

                if (card.CardType == CardType.Magic)
                {
                    Debug.Log("마법 카드!!!!");

                    if (isReduced)
                        card.DecreaseMana(cardAbility.abilityValue);
                    else
                        card.IncreaseMana(cardAbility.abilityValue);
                }
            }
        }
    }
}