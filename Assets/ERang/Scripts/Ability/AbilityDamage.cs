using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityDamage : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Damage;
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

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
                if (targetSlot.Card == null || (targetSlot.Card is not CreatureCard && targetSlot.Card is not MasterCard))
                {
                    if (targetSlot.Card == null)
                        Debug.LogWarning($"targetSlot.Card is null. slotNum: {targetSlot.SlotNum}");

                    if (targetSlot.Card is not CreatureCard && targetSlot.Card is not MasterCard)
                        Debug.LogWarning($"targetSlot.Card is not CreatureCard && targetSlot.Card is not MasterCard. slotNum: {targetSlot.SlotNum}");

                    Changes.Add((false, targetSlot.SlotNum, 0, targetSlot.SlotCardType, 0, 0, 0));
                    continue;
                }

                CreatureCard creatureCard = targetSlot.Card as CreatureCard;

                int cardId = creatureCard.Id;
                int before = creatureCard.Hp;

                for (int i = 0; i < atkCount; i++)
                {
                    // Debug.Log($"slotNum : {targetSlot.SlotNum}, cardId : {cardId}, hp : {creatureCard.Hp}, damage : {damage}");

                    yield return StartCoroutine(targetSlot.TakeDamage(damage));
                    targetSlot.TakeDamageAnimation();

                    yield return new WaitForSeconds(0.5f);
                }

                // targetSlot.SetDamage 으로 hp 가 0 이 되면 카드 제거로 Card 가 null 이 됨
                if (creatureCard == null)
                {
                    Changes.Add((false, targetSlot.SlotNum, cardId, targetSlot.SlotCardType, 0, 0, 0));
                    continue;
                }

                // 카드가 hp 0 으로 제거되는 경우도 있음
                Changes.Add((true, targetSlot.SlotNum, cardId, targetSlot.SlotCardType, before, creatureCard.Hp, damage * atkCount));
            }
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}