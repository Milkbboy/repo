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

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AiData aiData = Utils.CheckData(AiData.GetAiData, "AiData", cardAbility.aiDataId);

            if (aiData == null)
                yield break;

            AiDataType aiDataType = aiData.type;
            AiDataAttackType aiAttackType = aiData.attackType;
            int atkCount = aiData.atk_Cnt;

            selfSlot.ApplyDamageAnimation();

            int value = 0;

            if (selfSlot.Card is CreatureCard)
                value = (selfSlot.Card as CreatureCard).Atk;

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            if (Constants.SelectAttackTypes.Contains(aiAttackType))
                value = cardAbility.abilityValue;

            // 원거리 미사일 발사
            if (aiDataType == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlot, atkCount, value));

            // Debug.Log($"targetSlots: {string.Join(", ", targetSlots.Select(x => x.SlotNum))}");

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

            int beforeHp = card.Hp;
            int beforeDef = card.Def;

            for (int i = 0; i < atkCount; i++)
            {
                Debug.Log($"{targetSlot.LogText}, hp: {card.Hp}, def: {card.Def}, damage : {value}");

                yield return StartCoroutine(targetSlot.TakeDamage(value));

                yield return new WaitForSeconds(0.5f);
            }

            // TakeDamage 에서 카드가 Destroy 되면 null 이 되는 경우도 있음
            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card?.Id ?? 0, targetSlot.SlotCardType, beforeHp, card?.Hp ?? 0, value * atkCount));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card?.Id ?? 0, targetSlot.SlotCardType, beforeDef, card?.Def ?? 0, value * atkCount));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}