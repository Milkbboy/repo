using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityWeaken : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Weaken;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false)); // 공격력 감소
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        private IEnumerator Apply(CardAbility cardAbility, BoardSlot targetSlot, bool isAtkUp)
        {
            if (!ValidateTargetSlot(targetSlot, "Weaken"))
                yield break;

            BaseCard targetCard = targetSlot.Card;
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 약화 값: {value}", LogType.Warning);
                yield break;
            }

            int beforeAtk = targetCard.Atk;

            if (isAtkUp)
            {
                targetSlot.IncreaseAttack(value);
                LogAbility($"약화 해제: {beforeAtk} -> {targetCard.Atk} (+{value})");
            }
            else
            {
                targetSlot.DecreaseAttack(value);
                LogAbility($"약화 적용: {beforeAtk} -> {targetCard.Atk} (-{value})");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}