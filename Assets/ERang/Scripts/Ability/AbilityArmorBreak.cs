using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityArmorBreak : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Doom;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            yield return StartCoroutine(ApplySingle(aiData, abilityData, selfSlot, null));
        }

        public IEnumerator ApplySingle(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}