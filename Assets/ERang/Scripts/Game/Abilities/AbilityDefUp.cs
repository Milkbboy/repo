using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityDefUp : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.DefUp;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BoardSlot targetSlot, bool isDefUp)
        {
            if (!ValidateTargetSlot(targetSlot, "DefUp"))
                yield break;

            BaseCard targetCard = targetSlot.Card;
            int beforeDef = targetCard.Def;
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 방어력 변경 값: {value}", LogType.Warning);
                yield break;
            }

            // 방어력 증가
            if (isDefUp)
            {
                targetSlot.IncreaseDefense(value);
                LogAbility($"방어력 증가: {beforeDef} -> {targetCard.Def} (+{value})");
            }
            else
            {
                targetSlot.DecreaseDefense(value);
                LogAbility($"방어력 감소: {beforeDef} -> {targetCard.Def} (-{value})");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}