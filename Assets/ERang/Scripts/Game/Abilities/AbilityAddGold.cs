using System.Collections;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityAddGold : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.AddGold;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            AiData aiData = AiData.GetAiData(cardAbility.aiDataId);
            if (aiData == null)
            {
                LogAbility("AiData를 찾을 수 없습니다.", LogType.Error);
                yield break;
            }

            if (Player.Instance == null)
            {
                LogAbility("Player 인스턴스가 없습니다.", LogType.Error);
                yield break;
            }

            // 고정 골드 추가
            int goldToAdd = cardAbility.abilityValue;

            int beforeGold = Player.Instance.Gold;
            BoardSystem.Instance.AddGold(Player.Instance, goldToAdd);

            LogAbility($"골드 획득: {goldToAdd}");
            LogAbility($"골드 변화: {beforeGold} -> {Player.Instance.Gold}");

            yield return new WaitForSeconds(0.1f);
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 골드는 해제되지 않는 즉시 효과
            LogAbility("골드는 해제할 수 없는 즉시 효과입니다.");
            yield break;
        }
    }
}
