using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityMaxSatietyGauge : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.MaxSatietyGauge;

        private int originalMaxSatiety;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            int value = cardAbility.abilityValue;

            if (value < 0)
            {
                LogAbility($"잘못된 최대 만복도 값: {value}", LogType.Warning);
                yield break;
            }

            // 원래 최대 만복도 저장
            originalMaxSatiety = Player.Instance.MaxSatiety;

            LogAbility($"최대 만복도 변경: {originalMaxSatiety} -> {value}");
            BattleController.Instance.UpdateMaxSatietyGauge(value);

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            LogAbility($"최대 만복도 복구: {Player.Instance.MaxSatiety} -> {originalMaxSatiety}");
            BattleController.Instance.UpdateMaxSatietyGauge(originalMaxSatiety);

            yield return new WaitForSeconds(0.1f);
        }
    }
}