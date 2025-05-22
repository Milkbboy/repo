using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityAddGoldPer : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AddGoldPer;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AiData aiData = Utils.CheckData(AiData.GetAiData, "AiData", cardAbility.aiDataId);

            if (aiData == null)
                yield break;

            AbilityData abilityData = Utils.CheckData(AbilityData.GetAbilityData, "AbilityData", cardAbility.abilityId);

            if (abilityData == null)
                yield break;

            float gainGold = aiData.value * abilityData.ratio;
            int gold = aiData.value + (int)gainGold;
            int beforeGold = Player.Instance.masterCard.Gold;

            BoardSystem.Instance.AddGold(Player.Instance.masterCard, gold);

            // 골드 획득량 표시. 슬롯이 아닌 보드 골드에 표시하는 걸로
            // selfSlot.SetFloatingGold(beforeGold, Master.Instance.Gold);

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield break;
        }
    }
}