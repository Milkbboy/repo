using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityChargeDamage : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.ChargeDamage;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "ChargeDamage"))
                yield break;

            LogAbility($"차지 데미지 충전 시작 - {cardAbility.duration}턴 후 발동");

            // 차징 애니메이션이나 시각적 효과 추가 가능
            // TODO: 차징 애니메이션 구현 시 여기에 추가
            yield break;
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "ChargeDamage") || !ValidateTargetSlot(selfSlot, "ChargeDamage"))
                yield break;

            AiData aiData = Utils.CheckData(AiData.GetAiData, "AiData", cardAbility.aiDataId);
            if (aiData == null)
            {
                LogAbility("AiData를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            int damage = 0;

            // 크리처 카드의 경우 공격력 사용
            if (selfSlot.Card is CreatureCard creatureCard)
                damage = creatureCard.Atk;

            // 카드 선택 공격 타입이면 어빌리티 데미지 값 사용
            if (Constants.SelectAttackTypes.Contains(aiData.attackType))
                damage = cardAbility.abilityValue;

            LogAbility($"차지 데미지 계산 완료: {damage} (공격 타입: {aiData.attackType})");

            int atkCount = aiData.atk_Cnt;

            LogAbility($"차지 데미지 발동! 데미지: {damage} x {atkCount}회");

            selfSlot.ApplyDamageAnimation();

            // 원거리 미사일 발사 (기존 코드와 호환성 유지)
            if (aiData.type == AiDataType.Ranged)
            {
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, new List<BSlot> { targetSlot }, atkCount, damage));
            }

            // 공격 횟수만큼 데미지 적용
            for (int i = 0; i < atkCount; i++)
            {
                LogAbility($"차지 데미지 적용 {i + 1}/{atkCount} - 데미지: {damage}");
                yield return StartCoroutine(targetSlot.TakeDamage(damage));

                // 대상이 파괴되면 중단
                if (targetSlot.Card == null)
                {
                    LogAbility($"대상이 파괴되어 차지 데미지 중단 ({i + 1}/{atkCount})");
                    break;
                }

                yield return new WaitForSeconds(0.3f);
            }

            // 변경사항 기록
            LogAbility($"차지 데미지 완료 - 총 데미지: {damage * atkCount}");
        }
    }
}