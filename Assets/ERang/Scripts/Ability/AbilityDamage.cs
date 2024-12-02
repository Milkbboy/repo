using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityDamage : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Damage;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            AiDataType aiDataType = aiData.type;
            AiDataAttackType aiAttackType = aiData.attackType;
            int atkCount = aiData.atk_Cnt;
            int abilityValue = abilityData.value;

            selfSlot.ApplyDamageAnimation();

            int damage = 0;

            if (selfSlot.Card is CreatureCard)
                damage = (selfSlot.Card as CreatureCard).Atk;

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            if (Constants.SelectAttackTypes.Contains(aiAttackType))
                damage = abilityValue;

            // 원거리 미사일 발사
            if (aiDataType == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlots, atkCount, damage));

            // Debug.Log($"targetSlots: {string.Join(", ", targetSlots.Select(x => x.SlotNum))}");

            foreach (BSlot targetSlot in targetSlots)
            {
                BaseCard card = targetSlot.Card;

                if (card == null)
                {
                    Debug.LogWarning($"{targetSlot.LogText}");
                    continue;
                }

                if (card is not CreatureCard && card is not MasterCard)
                {
                    Debug.LogWarning($"{targetSlot.LogText}: 타겟 슬롯 카드가 CreatureCard 또는 MasterCard 가 아닙니다.");

                    Changes.Add((StatType.Hp, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, card.Hp, card.Hp, damage));
                    Changes.Add((StatType.Def, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, card.Def, card.Def, damage));
                    continue;
                }

                int beforeHp = card.Hp;
                int beforeDef = card.Def;

                for (int i = 0; i < atkCount; i++)
                {
                    Debug.Log($"{targetSlot.LogText}, hp: {card.Hp}, def: {card.Def}, damage : {damage}");

                    yield return StartCoroutine(targetSlot.TakeDamage(damage));
                    targetSlot.TakeDamageAnimation();

                    yield return new WaitForSeconds(0.5f);
                }

                // targetSlot.TakeDamage 으로 hp 가 0 이 되면 카드 제거로 Card 가 null 이 됨
                if (card == null)
                {
                    Changes.Add((StatType.Hp, false, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, 0, 0, damage));
                    continue;
                }

                // 카드가 hp 0 으로 제거되는 경우도 있음
                Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, targetSlot.Card.Hp, damage * atkCount));
                Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, targetSlot.Card.Def, damage * atkCount));
            }
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}