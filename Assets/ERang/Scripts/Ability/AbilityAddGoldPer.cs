using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class AbilityAddGoldPer : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AddGoldPer;
        public List<(bool, int, int, CardType, int, int, int)> Changes { get; set; } = new List<(bool, int, int, CardType, int, int, int)>();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            float gainGold = aiData.value * abilityData.ratio;
            int gold = aiData.value + (int)gainGold;
            int beforeGold = Master.Instance.Gold;

            BoardSystem.Instance.AddGold(Master.Instance, gold);

            // 골드 획득량 표시
            selfSlot.SetFloatingGold(beforeGold, Master.Instance.Gold);

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield break;
        }
    }
}