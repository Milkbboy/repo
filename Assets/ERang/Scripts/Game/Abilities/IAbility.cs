using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 기본 어빌리티 인터페이스 (보드 슬롯용)
    /// </summary>
    public interface IAbility
    {
        public AbilityType AbilityType => AbilityType.None;

        IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);
        IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);
    }

    /// <summary>
    /// 핸드 카드 전용 어빌리티 인터페이스
    /// </summary>
    public interface IHandAbility : IAbility
    {
        IEnumerator ApplySingle(BaseCard card);
        IEnumerator Release(BaseCard card);
    }

    /// <summary>
    /// 어빌리티 구현을 위한 기본 추상 클래스
    /// </summary>
    public abstract class BaseAbility : MonoBehaviour, IAbility
    {
        public abstract AbilityType AbilityType { get; }

        public abstract IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);
        public abstract IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);

        /// <summary>
        /// 대상 슬롯 검증 - 공통 검증 로직
        /// </summary>
        protected bool ValidateTargetSlot(BSlot targetSlot, string abilityName)
        {
            if (targetSlot?.Card == null)
            {
                Debug.LogWarning($"<color=orange>[{abilityName}]</color> {targetSlot?.ToSlotLogInfo() ?? "null"} 카드가 없습니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 카드 타입 검증 - 공통 검증 로직
        /// </summary>
        protected bool ValidateCardType<T>(BaseCard card, string abilityName) where T : BaseCard
        {
            if (card is not T)
            {
                Debug.LogWarning($"<color=orange>[{abilityName}]</color> {card.ToCardLogInfo()}는 {typeof(T).Name} 타입이 아닙니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 디버그 로그 출력 (어빌리티별 색상 구분)
        /// </summary>
        protected void LogAbility(string message, LogType logType = LogType.Log)
        {
            string coloredMessage = AbilityType switch
            {
                AbilityType.Damage or AbilityType.Burn or AbilityType.Poison => $"<color=red>[{AbilityType}]</color> {message}",
                AbilityType.Heal => $"<color=green>[{AbilityType}]</color> {message}",
                AbilityType.AtkUp or AbilityType.DefUp => $"<color=blue>[{AbilityType}]</color> {message}",
                AbilityType.ReducedMana => $"<color=cyan>[{AbilityType}]</color> {message}",
                AbilityType.SubSatiety => $"<color=yellow>[{AbilityType}]</color> {message}",
                _ => $"<color=white>[{AbilityType}]</color> {message}"
            };

            switch (logType)
            {
                case LogType.Warning:
                    Debug.LogWarning(coloredMessage);
                    break;
                case LogType.Error:
                    Debug.LogError(coloredMessage);
                    break;
                default:
                    Debug.Log(coloredMessage);
                    break;
            }
        }
    }

    /// <summary>
    /// 핸드 카드 어빌리티를 위한 기본 추상 클래스
    /// </summary>
    public abstract class BaseHandAbility : BaseAbility, IHandAbility
    {
        // IHandAbility 구현
        public abstract IEnumerator ApplySingle(BaseCard card);
        public abstract IEnumerator Release(BaseCard card);

        // 보드 슬롯용 메서드는 기본적으로 경고 출력
        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            LogAbility("이 어빌리티는 핸드 카드 전용입니다.", LogType.Warning);
            yield break;
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
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