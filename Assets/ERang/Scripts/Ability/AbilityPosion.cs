using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 독 어빌리티
    /// </summary>
    public class AbilityPosion : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Poison;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
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

            int value = cardAbility.abilityValue;
            int beforeHp = card.Hp;
            int beforeDef = card.Def;

            yield return StartCoroutine(targetSlot.TakeDamage(value));

            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, targetSlot.Card.Hp, value));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, targetSlot.Card.Def, value));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}