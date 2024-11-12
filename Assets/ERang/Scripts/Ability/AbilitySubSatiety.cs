using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilitySubSatiety : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.SubSatiety;
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            int amount = abilityData.value * -1;

            BattleLogic.Instance.UpdateSatietyGauge(amount);

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}