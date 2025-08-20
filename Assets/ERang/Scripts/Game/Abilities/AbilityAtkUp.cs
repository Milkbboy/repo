using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityAtkUp : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.AtkUp;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BoardSlot targetSlot, bool isAtkUp)
        {
            if (!ValidateTargetSlot(targetSlot, "AtkUp"))
                yield break;

            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "AtkUp"))
                yield break;

            CreatureCard creatureCard = targetSlot.Card as CreatureCard;
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 공격력 변경 값: {value}", LogType.Warning);
                yield break;
            }

            int beforeAtk = creatureCard.Atk;

            if (isAtkUp)
            {
                targetSlot.IncreaseAttack(value);
                LogAbility($"공격력 증가: {beforeAtk} -> {creatureCard.Atk} (+{value})");
            }
            else
            {
                targetSlot.DecreaseAttack(value);
                LogAbility($"공격력 감소: {beforeAtk} -> {creatureCard.Atk} (-{value})");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}