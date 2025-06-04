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

            BaseCard targetCard = targetSlot.Card;
            int beforeHp = targetCard.Hp;
            int beforeDef = targetCard.Def;

            LogAbility($"중독 데미지 적용: {poisonDamage} (행동 후 발동)");
            yield return StartCoroutine(targetSlot.TakeDamage(poisonDamage));

            // 변경사항 기록 (카드가 파괴될 수 있음)
            RecordChange(StatType.Hp, targetSlot, beforeHp, targetCard?.Hp ?? 0, poisonDamage);
            RecordChange(StatType.Def, targetSlot, beforeDef, targetCard?.Def ?? 0, poisonDamage);
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            // 중독은 지속 시간이 끝나면 자동으로 해제
            LogAbility("중독 상태 해제");
            yield break;
        }
    }
}