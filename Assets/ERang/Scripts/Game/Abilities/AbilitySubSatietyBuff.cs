using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilitySubSatietyBuff : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.SubSatietyBuff;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 포만감 값: {value}", LogType.Warning);
                yield break;
            }

            // 음수로 변환하여 포만감 감소
            int reductionValue = -value;

            LogAbility($"포만감 감소 버프: {reductionValue}");

            BattleController.Instance.UpdateSatietyGauge(reductionValue);

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            LogAbility("포만감 감소 버프는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}
