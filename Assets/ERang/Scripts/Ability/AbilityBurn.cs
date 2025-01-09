using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityBurn : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Burn;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText}: 카드가 null 입니다.");
                yield break;
            }

            int value = cardAbility.abilityValue;

            int beforeHp = card.Hp;
            int beforeDef = card.Def;

            yield return StartCoroutine(targetSlot.TakeDamage(value));

            // 카드가 hp 0 으로 제거되는 경우도 있음
            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, targetSlot.Card.Hp, value));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, targetSlot.Card.Def, value));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}