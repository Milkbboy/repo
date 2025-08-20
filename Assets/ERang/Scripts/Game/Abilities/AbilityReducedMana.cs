using System.Collections;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 마나 감소 어빌리티 - 핸드 카드 전용
    /// 해당 턴에서 카드 소비 마나가 줄어드는 효과
    /// </summary>
    public class AbilityReducedMana : BaseHandAbility
    {
        public override AbilityType AbilityType => AbilityType.ReducedMana;

        /// <summary>
        /// 마나 감소 적용 (핸드에 카드가 들어올 때)
        /// </summary>
        public override IEnumerator ApplySingle(BaseCard card)
        {
            if (!ValidateHandCard(card)) yield break;

            LogAbility($"마나 감소 적용 시작: {card.ToCardLogInfo()}");
            Apply(card, true);

            yield break;
        }

        /// <summary>
        /// 마나 감소 해제 (핸드에서 카드가 나갈 때)
        /// </summary>
        public override IEnumerator Release(BaseCard card)
        {
            if (!ValidateHandCard(card)) yield break;

            LogAbility($"마나 감소 해제 시작: {card.ToCardLogInfo()}");
            Apply(card, false);

            yield break;
        }

        /// <summary>
        /// 마나 증감 적용 실행
        /// </summary>
        /// <param name="card">대상 카드</param>
        /// <param name="isReduced">true: 감소 적용, false: 감소 해제</param>
        private void Apply(BaseCard card, bool isReduced)
        {
            if (card?.AbilitySystem?.HandAbilities == null)
            {
                LogAbility("카드의 어빌리티 시스템이 없습니다.", LogType.Warning);
                return;
            }

            // ReducedMana 타입의 핸드 어빌리티만 처리
            var reducedManaAbilities = card.AbilitySystem.HandAbilities.FindAll(
                ability => ability.abilityType == AbilityType.ReducedMana);

            if (reducedManaAbilities.Count == 0)
            {
                LogAbility("ReducedMana 어빌리티가 없습니다.", LogType.Warning);
                return;
            }

            foreach (CardAbility cardAbility in reducedManaAbilities)
            {
                int value = cardAbility.abilityValue;

                if (value <= 0)
                {
                    LogAbility($"잘못된 어빌리티 값: {value}", LogType.Warning);
                    continue;
                }

                // 마나 변경 처리 (BaseHandAbility의 헬퍼 메서드 사용)
                bool success = TryChangeMana(card, value, isReduced);

                if (success)
                {
                    string action = isReduced ? "감소" : "복구";
                    LogAbility($"{action} 완료 - 값: {value}, 최종 마나: {card.Mana}");
                }
            }
        }
    }
}