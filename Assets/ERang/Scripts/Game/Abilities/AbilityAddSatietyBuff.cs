using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityAddSatietyBuff : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.AddSatietyBuff;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 포만감 값: {value}", LogType.Warning);
                yield break;
            }

            LogAbility($"포만감 증가 버프: +{value}");
            BattleController.Instance.UpdateSatietyGauge(value);

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 포만감은 해제되지 않는 즉시 효과
            LogAbility("포만감 버프는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}
