using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityBurn : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Burn;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Burn"))
                yield break;

            int burnDamage = cardAbility.abilityValue;

            if (burnDamage <= 0)
            {
                LogAbility($"잘못된 화상 데미지 값: {burnDamage}", LogType.Warning);
                yield break;
            }

            BaseCard targetCard = targetSlot.Card;
            int beforeHp = targetCard.Hp;
            int beforeDef = targetCard.Def;

            LogAbility($"화상 데미지 적용: {burnDamage} (행동 전 발동)");
            yield return StartCoroutine(targetSlot.TakeDamage(burnDamage));
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 화상은 지속 시간이 끝나면 자동으로 해제
            LogAbility("화상 상태 해제");
            yield break;
        }
    }
}