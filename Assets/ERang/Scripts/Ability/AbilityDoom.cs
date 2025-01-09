using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityDoom : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Doom;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }

        /// <summary>
        /// Hp 0 설정
        /// </summary>
        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            if (card is not CreatureCard && card is not MasterCard)
            {
                Debug.LogWarning($"{targetSlot.LogText}: 타겟 슬롯 카드가 CreatureCard 또는 MasterCard 가 아닙니다.");
                yield break;
            }

            int value = card.Hp;
            int beforeHp = card.Hp;

            yield return StartCoroutine(targetSlot.TakeDamage(value));

            if (card != null)
            {
                Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, targetSlot.Card.Hp, value));
                yield break;
            }
        }
    }
}