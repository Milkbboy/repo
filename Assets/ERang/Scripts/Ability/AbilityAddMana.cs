using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityAddMana : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AddMana;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, selfSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isAdd)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            if (card is not MasterCard masterCard)
            {
                Debug.LogWarning($"{card.LogText} 마스터 카드 아님.");
                yield break;
            }

            int beforeMana = masterCard.Mana;
            int value = cardAbility.abilityValue;

            if (isAdd)
                masterCard.IncreaseMana(value);
            else
                masterCard.DecreaseMana(value);

            Debug.Log($"<color=#257dca>마나 변화량 {(isAdd ? value : value * -1)}</color>({beforeMana} => {masterCard.Mana})");

            yield return new WaitForSeconds(0.1f);
        }
    }
}