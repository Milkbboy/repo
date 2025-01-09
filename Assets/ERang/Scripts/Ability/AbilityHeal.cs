using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityHeal : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Heal;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int value = cardAbility.abilityValue;
            int before = targetSlot.Card.Hp;

            targetSlot.RestoreHealth(value);

            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, targetSlot.Card.Id, targetSlot.SlotCardType, before, targetSlot.Card.Hp, value));

            yield return new WaitForSeconds(0.1f);
        }

        // 즉시 효과는 해제 불필요
        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}