using System.Linq;
using System.Collections;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityDamage : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Damage;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Damage"))
                yield break;

            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Damage") &&
                !ValidateCardType<MasterCard>(targetSlot.Card, "Damage"))
            {
                LogAbility($"데미지를 받을 수 없는 카드 타입: {targetSlot.Card.CardType}", LogType.Warning);
                yield break;
            }

            AiData aiData = AiData.GetAiData(cardAbility.aiDataId);

            if (aiData == null)
            {
                LogAbility($"AiData({cardAbility.aiDataId})를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            int damage = 0;

            // 크리처 카드의 경우 공격력 사용
            if (selfSlot.Card is CreatureCard)
                damage = (selfSlot.Card as CreatureCard).Atk;
            else
                damage = cardAbility.abilityValue;

            LogAbility($"계산된 데미지: {damage} (공격 타입: {aiData.attackType})");

            int atkCount = aiData.atk_Cnt;

            selfSlot.ApplyDamageAnimation();

            // 원거리 미사일 발사
            if (aiData.type == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlot, atkCount, damage));

            BaseCard targetCard = targetSlot.Card;

            // 공격 횟수만큼 데미지 적용
            for (int i = 0; i < atkCount; i++)
            {
                LogAbility($"데미지 적용 총 {atkCount}회 중 {i + 1}회 - 대상: {targetSlot.ToSlotLogInfo()}, HP: {targetCard.Hp}, DEF: {targetCard.Def}, 데미지: {damage}");

                yield return StartCoroutine(targetSlot.TakeDamage(damage));

                // 데미지 적용 후 타겟 카드 확인
                LogAbility($"데미지 적용 후 타겟 카드 확인 - 대상: {targetSlot.ToSlotLogInfo()}, HP: {targetCard.Hp}, DEF: {targetCard.Def}");

                // ⭐ 체인 어빌리티 트리거 - 데미지를 가할 때마다 호출!
                ChainAbilityEvents.TriggerDamageDealt(selfSlot, cardAbility.aiDataId);

                yield return new WaitForSeconds(0.5f);
            }

            // LogAbility($"데미지 완료 - 대상: {targetSlot.ToSlotLogInfo()}, 총 데미지: {damage * atkCount}");
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 데미지는 해제되지 않는 즉시 효과
            LogAbility("데미지는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}