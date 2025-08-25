using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityBrokenDef : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.BrokenDef;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "BrokenDef"))
                yield break;

            BaseCard targetCard = targetSlot.Card;
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 방어력 변경 값: {value}", LogType.Warning);
                yield break;
            }

            if (targetCard.AbilitySystem.ArmorBreakAbility != null)
            {
                LogAbility($"ArmorBreak 상태로 인해 방어력 감소 무시");
                yield break;
            }

            // 이 어빌리티를 제외한 현재 방어력 계산
            int beforeDef = targetCard.Stat.CalculateStatWithoutAbility(StatType.Def, cardAbility);
            // 어빌리티가 적용된 후의 방어력 (다음 Def 접근 시 자동 계산됨)
            int afterDef = targetCard.Def;

            LogAbility($"방어력 감소: {beforeDef} -> {afterDef} (-{value})");

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "BrokenDef"))
                yield break;

            BaseCard targetCard = targetSlot.Card;

            // 해제되기 전의 방어력
            int beforeDef = targetCard.Def;
            // 해제된 후의 방어력 (이 어빌리티 제외하고 계산)
            int afterDef = targetCard.Stat.CalculateStatWithoutAbility(StatType.Def, cardAbility);

            LogAbility($"방어력 감소 해제: {beforeDef} -> {afterDef}");
            yield break;
        }
    }
}