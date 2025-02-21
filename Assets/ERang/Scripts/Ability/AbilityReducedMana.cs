using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 사용 마나 감소 어빌리티
    /// </summary>
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
            foreach (CardAbility cardAbility in card.HandAbilities)
            {
                if (isReduced)
                    card.DecreaseMana(cardAbility.abilityValue);
                else
                    card.IncreaseMana(cardAbility.abilityValue);
            }
        }
    }
}