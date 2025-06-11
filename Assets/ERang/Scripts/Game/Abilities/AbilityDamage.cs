using System.Linq;
using System.Collections;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityDamage : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Damage;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Damage"))
                yield break;

            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Damage") &&
                !ValidateCardType<MasterCard>(targetSlot.Card, "Damage"))
            {
                LogAbility($"데미지를 받을 수 없는 카드 타입: {targetSlot.Card.CardType}", LogType.Warning);
                yield break;
            }

            AiData aiData = Utils.CheckData(AiData.GetAiData, "AiData", cardAbility.aiDataId);

            if (aiData == null)
            {
                LogAbility("AiData를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            int damage = 0;

            // 크리처 카드의 경우 공격력 사용
            if (selfSlot.Card is CreatureCard)
                damage = (selfSlot.Card as CreatureCard).Atk;

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            if (Constants.SelectAttackTypes.Contains(aiData.attackType))
                damage = cardAbility.abilityValue;

            LogAbility($"계산된 데미지: {damage} (공격 타입: {aiData.attackType})");

            int atkCount = aiData.atk_Cnt;

            selfSlot.ApplyDamageAnimation();

            // 원거리 미사일 발사
            if (aiData.type == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlot, atkCount, damage));

            BaseCard targetCard = targetSlot.Card;
            int beforeHp = targetCard.Hp;
            int beforeDef = targetCard.Def;

            // 공격 횟수만큼 데미지 적용
            for (int i = 0; i < atkCount; i++)
            {
                LogAbility($"데미지 적용 {i + 1}/{atkCount} - 대상: {targetSlot.LogText}, HP: {targetCard.Hp}, DEF: {targetCard.Def}, 데미지: {damage}");

                yield return StartCoroutine(targetSlot.TakeDamage(damage));
                yield return new WaitForSeconds(0.5f);
            }

            // 변경사항 기록 (카드가 파괴되면 null이 될 수 있음)
            int totalDamage = damage * atkCount;
            RecordChange(StatType.Def, targetSlot, beforeDef, targetCard?.Def ?? 0, totalDamage);
            RecordChange(StatType.Hp, targetSlot, beforeHp, targetCard?.Hp ?? 0, totalDamage);
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 데미지는 해제되지 않는 즉시 효과
            LogAbility("데미지는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}