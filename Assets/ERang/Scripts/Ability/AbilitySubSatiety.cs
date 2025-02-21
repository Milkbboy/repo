using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 포만도 감소 어빌리티
    /// </summary>
    public class AbilitySubSatiety : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.SubSatiety;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            int value = cardAbility.abilityValue * -1;

            BattleLogic.Instance.UpdateSatietyGauge(value);

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}