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

        public IEnumerator ApplySingle(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            // 차징하는 애니메이션 있으면 좋을 듯
            yield break;
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AiData aiData = Utils.CheckData(AiData.GetAiData, "AiData", cardAbility.aiDataId);

            if (aiData == null)
                yield break;

            int atkCount = aiData.atk_Cnt;

            selfSlot.ApplyDamageAnimation();

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            int damage = 0;

            if (selfSlot.Card.CardType == CardType.Creature)
                damage = selfSlot.Card.State.Atk;

            if (Constants.SelectAttackTypes.Contains(aiData.attackType))
                damage = cardAbility.abilityValue;

            // 원거리 미사일 발사
            if (aiData.type == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, new List<BSlot> { targetSlot }, atkCount, damage));

            GameCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int beforeHp = card.State.Hp;
            int beforeDef = card.State.Def;

            for (int i = 0; i < atkCount; i++)
            {
                yield return StartCoroutine(targetSlot.TakeDamage(damage));
            }

            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, card.State.Hp, damage * atkCount));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, card.State.Def, damage * atkCount));
        }
    }
}