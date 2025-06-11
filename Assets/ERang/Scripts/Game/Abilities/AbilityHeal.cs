using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityHeal : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Heal;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Heal"))
                yield break;

            int healAmount = cardAbility.abilityValue;
            if (healAmount <= 0)
            {
                LogAbility($"잘못된 힐 값: {healAmount}", LogType.Warning);
                yield break;
            }

            BaseCard targetCard = targetSlot.Card;
            int beforeHp = targetCard.Hp;
            int maxHp = targetCard.MaxHp;

            // 이미 최대 체력인 경우
            if (beforeHp >= maxHp)
            {
                LogAbility($"이미 최대 체력입니다. 현재: {beforeHp}/{maxHp}");
                yield break;
            }

            targetSlot.RestoreHealth(healAmount);

            int actualHeal = targetCard.Hp - beforeHp;
            RecordChange(StatType.Hp, targetSlot, beforeHp, targetCard.Hp, -actualHeal); // 음수로 기록 (회복)

            LogAbility($"체력 회복 완료: {beforeHp} -> {targetCard.Hp} (+{actualHeal})");
            yield return new WaitForSeconds(0.1f);
        }

        // 즉시 효과는 해제 불필요
        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 힐은 해제되지 않는 즉시 효과
            LogAbility("힐은 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}