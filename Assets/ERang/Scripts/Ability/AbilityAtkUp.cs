using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 공격력 증가 어빌리티
    /// </summary>
    public class AbilityAtkUp : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AtkUp;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isAtkUp)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int before = card.Atk;
            int value = cardAbility.abilityValue;

            if (isAtkUp)
                card.IncreaseAttack(value);
            else
                card.DecreaseAttack(value);

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Atk, isAtkUp ? value : value * -1));

            yield return new WaitForSeconds(0.1f);
        }
    }
}