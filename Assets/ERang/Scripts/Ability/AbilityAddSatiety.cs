using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityAddSatiety : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AddSatiety;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            BattleLogic.Instance.UpdateSatietyGauge(cardAbility.abilityValue);

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}