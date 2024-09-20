using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityDamage : MonoBehaviour, IAbility
    {
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            AiDataType aiDataType = aiData.type;
            AiDataAttackType aiAttackType = aiData.attackType;
            int atkCount = aiData.atk_Cnt;
            int abilityValue = abilityData.value;

            selfSlot.AniAttack();

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            int damage = Constants.SelectAttackTypes.Contains(aiAttackType) ? abilityValue : selfSlot.Card.atk;

            // 원거리 미사일 발사
            if (aiDataType == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlots, atkCount, damage));

            // Debug.Log($"targetSlots: {string.Join(", ", targetSlots.Select(x => x.Slot))}");

            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    Changes.Add((false, targetSlot.Slot, 0, targetSlot.CardType, 0, 0, 0));
                    continue;
                }

                int cardId = targetSlot.Card.Id;
                int before = targetSlot.Card.hp;

                for (int i = 0; i < atkCount; i++)
                {
                    targetSlot.SetDamage(damage);
                    targetSlot.AniDamaged();

                    yield return new WaitForSeconds(0.5f);
                }

                // targetSlot.SetDamage 으로 hp 가 0 이 되면 카드 제거로 Card 가 null 이 됨
                if (targetSlot.Card == null)
                {
                    Changes.Add((false, targetSlot.Slot, cardId, targetSlot.CardType, 0, 0, 0));
                    continue;
                }

                // 카드가 hp 0 으로 제거되는 경우도 있음
                Changes.Add((true, targetSlot.Slot, cardId, targetSlot.CardType, before, targetSlot.Card.hp, damage * atkCount));
            }
        }

        public IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield break;
        }
    }
}