using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilitySubSatiety : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.SubSatiety;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 포만감 값: {value}", LogType.Warning);
                yield break;
            }

            if (BattleLogic.Instance == null)
            {
                LogAbility("BattleLogic 인스턴스가 없습니다.", LogType.Error);
                yield break;
            }

            // 음수로 변환하여 포만감 감소
            int reductionValue = -value;

            LogAbility($"포만감 감소: {reductionValue}");

            BattleLogic.Instance.UpdateSatietyGauge(reductionValue);

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 포만감은 해제되지 않는 즉시 효과
            LogAbility("포만감은 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}