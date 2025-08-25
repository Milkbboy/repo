using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityWeaken : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Weaken;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Weaken"))
                yield break;

            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Weaken"))
                yield break;

            CreatureCard creatureCard = targetSlot.Card as CreatureCard;
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 약화 수치: {value}", LogType.Warning);
                yield break;
            }

            // 이 어빌리티를 제외한 현재 공격력 계산
            int beforeAtk = creatureCard.Stat.CalculateStatWithoutAbility(StatType.Atk, cardAbility);
            // 어빌리티가 적용된 후의 공격력 (다음 Atk 접근 시 자동 계산됨)
            int afterAtk = creatureCard.Atk;

            LogAbility($"약화 효과 적용: {beforeAtk} -> {afterAtk} (-{value})");

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Weaken"))
                yield break;

            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Weaken"))
                yield break;

            CreatureCard creatureCard = targetSlot.Card as CreatureCard;
            
            // 해제되기 전의 공격력
            int beforeAtk = creatureCard.Atk;
            // 해제된 후의 공격력 (이 어빌리티 제외하고 계산)
            int afterAtk = creatureCard.Stat.CalculateStatWithoutAbility(StatType.Atk, cardAbility);

            LogAbility($"약화 효과 해제: {beforeAtk} -> {afterAtk}");
            yield break;
        }
    }
}