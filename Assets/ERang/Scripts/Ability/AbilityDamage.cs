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
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ AiData({cardAbility.aiDataId}) 테이블 데이터 없음");
                yield break;
            }

            AiDataType aiDataType = aiData.type;
            AiDataAttackType aiAttackType = aiData.attackType;
            int atkCount = aiData.atk_Cnt;

            string sourceInfo = selfSlot.Card?.Name ?? $"슬롯{selfSlot.SlotNum}";
            string targetInfo = targetSlot.Card?.Name ?? $"슬롯{targetSlot.SlotNum}";

            GameLogger.LogAbilityDetail($"데미지 어빌리티: {aiDataType}, 공격타입: {aiAttackType}, 공격횟수: {atkCount}");

            selfSlot.ApplyDamageAnimation();

            int value = 0;

            if (selfSlot.Card.CardType == CardType.Creature)
                value = selfSlot.Card.State.Atk;

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            if (Constants.SelectAttackTypes.Contains(aiAttackType))
            {
                value = cardAbility.abilityValue;
                GameLogger.LogAbilityDetail($"선택 공격 타입 - 어빌리티 값 사용: {value}");
            }
            else if (selfSlot.Card.CardType == CardType.Creature)
            {
                GameLogger.LogAbilityDetail($"크리쳐 공격력 사용: {value}");
            }

            // 원거리 미사일 발사
            if (aiDataType == AiDataType.Ranged)
            {
                GameLogger.LogAbilityDetail($"원거리 공격 - 미사일 발사: {sourceInfo} → {targetInfo}");
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlot, atkCount, value));
            }

            // Debug.Log($"targetSlots: {string.Join(", ", targetSlots.Select(x => x.SlotNum))}");

            GameCard card = targetSlot.Card;

            if (card == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {targetSlot.LogText} 카드 없음");
                yield break;
            }

            if (card.CardType != CardType.Creature && card.CardType != CardType.Master)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {targetSlot.LogText}: 타겟 슬롯 카드가 CreatureCard 또는 MasterCard가 아닙니다. 현재: {card.CardType}");
                yield break;
            }

            int beforeHp = card.State.Hp;
            int beforeDef = card.State.Def;

            GameLogger.LogAbilityDetail($"데미지 적용 전 상태 - HP: {beforeHp}, DEF: {beforeDef}");

            for (int i = 0; i < atkCount; i++)
            {
                GameLogger.LogAbilityDetail($"공격 {i + 1}/{atkCount} - {targetInfo} HP: {card.State.Hp}, DEF: {card.State.Def}, 데미지: {value}");

                yield return StartCoroutine(targetSlot.TakeDamage(value));
                yield return new WaitForSeconds(0.5f);
            }

            // TakeDamage 에서 카드가 Destroy 되면 null 이 되는 경우도 있음
            int finalHp = card?.State.Hp ?? 0;
            int finalDef = card?.State.Def ?? 0;
            int totalDamage = value * atkCount;

            GameLogger.LogAbilityDetail($"데미지 적용 완료 - HP: {beforeHp} → {finalHp}, DEF: {beforeDef} → {finalDef}, 총 데미지: {totalDamage}");

            // TakeDamage 에서 카드가 Destroy 되면 null 이 되는 경우도 있음
            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card?.Id ?? 0, targetSlot.SlotCardType, beforeHp, card?.State.Hp ?? 0, value * atkCount));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card?.Id ?? 0, targetSlot.SlotCardType, beforeDef, card?.State.Def ?? 0, value * atkCount));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 데미지는 즉시 효과라서 해제할 것 없음
            GameLogger.LogAbilityDetail("데미지 어빌리티는 즉시 효과 - 해제 불필요");
            yield break;
        }
    }
}