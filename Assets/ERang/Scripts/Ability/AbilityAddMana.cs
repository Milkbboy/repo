using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityAddMana : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AddMana;
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            int beforeMana = Master.Instance.Mana;

            // 마나 추가 획득
            BoardSystem.Instance.AddMana(Master.Instance, abilityData.value);
            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} <color=#257dca>마나 {abilityData.value} 추가 획득</color>({beforeMana} => {Master.Instance.Mana})");

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            int beforeMana = Master.Instance.Mana;

            BoardSystem.Instance.AddMana(Master.Instance, ability.abilityValue * -1);

            Debug.Log($"<color=#257dca>마나 {ability.abilityValue} 감소</color>({beforeMana} => {Master.Instance.Mana})");

            yield return new WaitForSeconds(0.1f);
        }
    }
}