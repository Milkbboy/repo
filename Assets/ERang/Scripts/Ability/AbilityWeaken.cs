using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 공격력 감소 어빌리티
    /// </summary>
    public class AbilityWeaken : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Weaken;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        /// <summary>
        /// 적용 - Atk 감소
        /// </summary>
        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        /// <summary>
        /// 해제 - Atk 증가
        /// </summary>
        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
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
                targetSlot.IncreaseAttack(value);
            else
                targetSlot.DecreaseAttack(value);

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Atk, isAtkUp ? value : value * -1));

            yield return new WaitForSeconds(0.1f);
        }
    }
}