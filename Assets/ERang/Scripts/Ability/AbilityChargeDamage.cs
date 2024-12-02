using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityChargeDamage : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.ChargeDamage;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            // 차징 애니메이션
            selfSlot.ApplyDamageAnimation();

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            AbilityData abilityData = AbilityData.GetAbilityData(ability.abilityId);
            AiData aiData = AiData.GetAiData(ability.aiDataId);

            int atkCount = aiData.atk_Cnt;

            selfSlot.ApplyDamageAnimation();

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            int damage = 0;

            if (selfSlot.Card is CreatureCard)
                damage = (selfSlot.Card as CreatureCard).Atk;

            if (Constants.SelectAttackTypes.Contains(aiData.attackType))
                damage = abilityData.value;

            // 원거리 미사일 발사
            if (aiData.type == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, new List<BSlot> { targetSlot }, atkCount, damage));

            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Changes.Add((StatType.Hp, false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                yield break;
            }

            int beforeHp = card.Hp;
            int beforeDef = card.Def;

            for (int i = 0; i < atkCount; i++)
            {
                yield return StartCoroutine(targetSlot.TakeDamage(damage));
                targetSlot.TakeDamageAnimation();

                yield return new WaitForSeconds(0.5f);
            }

            // targetSlot.SetDamage 으로 hp 가 0 이 되면 카드 제거로 Card 가 null 이 됨
            if (card == null)
            {
                Changes.Add((StatType.Hp, false, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, 0, 0, 0));
                yield break;
            }

            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, card.Hp, damage * atkCount));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, card.Def, damage * atkCount));
        }
    }
}