using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityBurn : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Burn;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            yield return StartCoroutine(ApplySingle(aiData, abilityData, selfSlot, null));
        }

        public IEnumerator ApplySingle(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText}");
                yield break;
            }

            int damage = abilityData.value;

            if (card is not CreatureCard && card is not MasterCard)
            {
                Debug.LogWarning($"{targetSlot.LogText}: 타겟 슬롯 카드가 CreatureCard 또는 MasterCard 가 아닙니다.");

                Changes.Add((StatType.Hp, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, card.Hp, card.Hp, damage));
                Changes.Add((StatType.Def, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, card.Def, card.Def, damage));
                yield break;
            }

            int beforeHp = card.Hp;
            int beforeDef = card.Def;

            yield return StartCoroutine(targetSlot.TakeDamage(damage));
            targetSlot.TakeDamageAnimation();

            // targetSlot.TakeDamage 으로 hp 가 0 이 되면 카드 제거로 Card 가 null 이 됨
            if (card == null)
            {
                Changes.Add((StatType.Hp, false, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, 0, 0, damage));
                yield break;
            }

            if (card == null || targetSlot.Card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText}: 카드가 null 입니다.");
                yield break;
            }

            // 카드가 hp 0 으로 제거되는 경우도 있음
            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, targetSlot.Card.Hp, damage));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, targetSlot.Card.Def, damage));
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}