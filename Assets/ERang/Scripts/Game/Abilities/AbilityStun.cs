using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityStun : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Stun;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Stun"))
                yield break;

            // 스턴은 CreatureCard와 MasterCard만 가능
            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Stun") &&
                !ValidateCardType<MasterCard>(targetSlot.Card, "Stun"))
            {
                LogAbility($"스턴을 받을 수 없는 카드 타입: {targetSlot.Card.CardType}", LogType.Warning);
                yield break;
            }

            LogAbility($"스턴 상태 적용 - {cardAbility.duration}턴 동안 행동 불가");

            // 스턴은 적용 시점에는 별다른 효과 없음 (시각적 표시만)
            // 실제 효과는 TurnManager의 CardAiAction에서 체크
            yield break;
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Stun"))
                yield break;

            LogAbility($"스턴 해제 - 행동 가능");
            yield break;
        }
    }
}