using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityAddSatiety : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.AddSatiety;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
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

            LogAbility($"포만감 증가: +{value}");
            BattleLogic.Instance.UpdateSatietyGauge(value);

            // 포만감은 전역 자원이므로 슬롯 정보는 -1로 기록
            RecordChange(StatType.None, selfSlot, 0, value, value); // TODO: 포만감용 StatType 추가 고려

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 포만감은 해제되지 않는 즉시 효과
            LogAbility("포만감은 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}