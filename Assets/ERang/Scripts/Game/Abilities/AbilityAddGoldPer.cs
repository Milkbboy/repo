using System.Collections;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityAddGoldPer : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.AddGoldPer;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AiData aiData = AiData.GetAiData(cardAbility.aiDataId);
            if (aiData == null)
            {
                LogAbility("AiData를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);
            if (abilityData == null)
            {
                LogAbility("AbilityData를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            if (Player.Instance == null)
            {
                LogAbility("Player 인스턴스가 없습니다.", LogType.Error);
                yield break;
            }

            // 퍼센트 기반 골드 계산
            float baseGold = aiData.value;
            float bonusGold = baseGold * abilityData.ratio;
            int totalGold = (int)(baseGold + bonusGold);

            int beforeGold = Player.Instance.Gold;
            BoardSystem.Instance.AddGold(Player.Instance, totalGold);

            LogAbility($"골드 획득: {totalGold} (기본: {baseGold}, 보너스: {bonusGold:F1}, 비율: {abilityData.ratio:P0})");
            LogAbility($"골드 변화: {beforeGold} -> {Player.Instance.Gold}");

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 골드는 해제되지 않는 즉시 효과
            LogAbility("골드는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}