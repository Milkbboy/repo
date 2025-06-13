using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityPoison : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Poison;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Poison"))
                yield break;

            // 중독은 CreatureCard와 MasterCard만 가능
            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Poison") &&
                !ValidateCardType<MasterCard>(targetSlot.Card, "Poison"))
            {
                LogAbility($"중독을 받을 수 없는 카드 타입: {targetSlot.Card.CardType}", LogType.Warning);
                yield break;
            }

            int poisonDamage = cardAbility.abilityValue;
            if (poisonDamage <= 0)
            {
                LogAbility($"잘못된 중독 데미지 값: {poisonDamage}", LogType.Warning);
                yield break;
            }

            LogAbility($"중독 데미지 적용: {poisonDamage} (행동 후 발동)");

            yield return StartCoroutine(targetSlot.TakeDamage(poisonDamage));
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 중독은 지속 시간이 끝나면 자동으로 해제
            LogAbility("중독 상태 해제");
            yield break;
        }
    }
}