using System.Collections;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 핸드 카드 어빌리티를 위한 기본 추상 클래스
    /// </summary>
    public abstract class BaseHandAbility : BaseAbility, IHandAbility
    {
        // IHandAbility 구현
        public abstract IEnumerator ApplySingle(BaseCard card);
        public abstract IEnumerator Release(BaseCard card);

        // 보드 슬롯용 메서드는 기본적으로 경고 출력
        public override IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            LogAbility("이 어빌리티는 핸드 카드 전용입니다.", LogType.Warning);
            yield break;
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            LogAbility("이 어빌리티는 핸드 카드 전용입니다.", LogType.Warning);
            yield break;
        }

        /// <summary>
        /// 핸드 카드 검증
        /// </summary>
        protected bool ValidateHandCard(BaseCard card)
        {
            if (card == null)
            {
                LogAbility("대상 카드가 null입니다.", LogType.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 마나 변경 처리 (카드 타입별 분기)
        /// </summary>
        protected bool TryChangeMana(BaseCard card, int value, bool isDecrease)
        {
            if (!ValidateHandCard(card))
            {
                LogAbility("대상 카드가 null입니다.", LogType.Warning);
                return false;
            }

            int before = card.Mana;

            switch (card)
            {
                case CreatureCard creatureCard:
                    if (isDecrease)
                        creatureCard.DecreaseMana(value);
                    else
                        creatureCard.IncreaseMana(value);
                    break;

                case MagicCard magicCard:
                    if (isDecrease)
                        magicCard.DecreaseMana(value);
                    else
                        magicCard.IncreaseMana(value);
                    break;

                default:
                    LogAbility($"마나 변경을 지원하지 않는 카드 타입: {card.GetType().Name}", LogType.Warning);
                    return false;
            }

            int changeValue = isDecrease ? -value : value;
            LogAbility($"마나 변경: {before} -> {card.Mana} (변경량: {changeValue})");

            return true;
        }
    }
}