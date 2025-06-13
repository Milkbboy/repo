using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityBrokenDef : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.BrokenDef;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false)); // 방어력 감소
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true)); // 방어력 복구
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isDefUp)
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
                LogAbility($"ArmorBreak 상태로 인해 방어력 {(isDefUp ? "증가" : "감소")} 무시");
                yield break;
            }

            int beforeDef = targetCard.Def;

            if (isDefUp)
            {
                targetCard.IncreaseDefense(value);
                LogAbility($"방어력 복구: {beforeDef} -> {targetCard.Def} (+{value})");
            }
            else
            {
                targetCard.DecreaseDefense(value);
                LogAbility($"방어력 감소: {beforeDef} -> {targetCard.Def} (-{value})");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}