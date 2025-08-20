using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityDoom : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.Doom;

        protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Doom"))
                yield break;

            // 파멸은 CreatureCard와 MasterCard만 가능
            if (!ValidateCardType<CreatureCard>(targetSlot.Card, "Doom") &&
                !ValidateCardType<MasterCard>(targetSlot.Card, "Doom"))
            {
                LogAbility($"파멸을 받을 수 없는 카드 타입: {targetSlot.Card.CardType}", LogType.Warning);
                yield break;
            }

            LogAbility($"파멸 상태 적용 - {cardAbility.duration}턴 후 즉사");

            // 파멸은 적용 시점에는 별다른 효과 없음 (시각적 표시만)
            // 실제 효과는 Release에서 발동
            yield break;
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            if (!ValidateTargetSlot(targetSlot, "Doom"))
                yield break;

            BaseCard targetCard = targetSlot.Card;

            // 파멸 발동: 현재 HP + DEF를 모두 뚫는 데미지로 즉사
            int lethalDamage = targetCard.Hp + targetCard.Def;

            LogAbility($"파멸 발동! 즉사 데미지: {lethalDamage}");
            yield return StartCoroutine(targetSlot.TakeDamage(lethalDamage));

            // 카드가 파괴되었을 가능성이 높으므로 null 체크
            if (targetSlot.Card != null)
            {
                LogAbility("파멸 완료 - 대상이 생존함 (예상외)");
            }
            else
            {
                LogAbility("파멸 완료 - 대상 제거됨");
            }
        }
    }
}