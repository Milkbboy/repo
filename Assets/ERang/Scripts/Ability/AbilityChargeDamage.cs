using System.Collections;
using System.Linq;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityChargeDamage : MonoBehaviour, IAbility
    {
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            // 차징 애니메이션
            selfSlot.AniAttack();

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            AiDataAttackType aiAttackType = ability.aiAttackType;
            int abilityValue = ability.abilityValue;
            int atkCount = ability.atkCount;
            AiDataType aiDataType = ability.aiType;

            selfSlot.AniAttack();

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            int damage = Constants.SelectAttackTypes.Contains(aiAttackType) ? abilityValue : selfSlot.Card.atk;

            // 원거리 미사일 발사
            if (aiDataType == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, new List<BoardSlot> { targetSlot }, atkCount, damage));

            if (targetSlot.Card == null)
            {
                Changes.Add((false, targetSlot.Slot, 0, targetSlot.CardType, 0, 0, 0));
                yield break;
            }

            int before = targetSlot.Card.hp;

            for (int i = 0; i < atkCount; i++)
            {
                targetSlot.SetDamage(damage);
                targetSlot.AniDamaged();

                yield return new WaitForSeconds(0.5f);
            }

            Changes.Add((true, targetSlot.Slot, targetSlot.Card.Id, targetSlot.CardType, before, targetSlot.Card.hp, damage * atkCount));
        }
    }
}