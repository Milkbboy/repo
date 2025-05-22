using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityDefUp : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.DefUp;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isDefUp)
        {
            GameCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int before = card.State.Def;
            int value = cardAbility.abilityValue;

            if (isDefUp)
                card.IncreaseDefense(value);
            else
                card.DecreaseDefense(value);

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.State.Def, isDefUp ? value : value * -1));

            yield return new WaitForSeconds(0.1f);
        }
    }
}