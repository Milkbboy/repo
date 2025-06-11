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
        public List<(StatType statType, bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)> Changes { get; set; }

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
        public List<(StatType statType, bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)> Changes { get; set; } = new();

        public abstract IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);
        public abstract IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);

        /// <summary>
        /// 대상 슬롯 검증 - 공통 검증 로직
        /// </summary>
        protected bool ValidateTargetSlot(BSlot targetSlot, string abilityName)
        {
            if (targetSlot?.Card == null)
            {
                Debug.LogWarning($"<color=orange>[{abilityName}]</color> {targetSlot?.LogText ?? "null"} 카드가 없습니다.");
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
                Debug.LogWarning($"<color=orange>[{abilityName}]</color> {card.LogText}는 {typeof(T).Name} 타입이 아닙니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 변경사항 기록 - 공통 기록 로직
        /// </summary>
        protected void RecordChange(StatType statType, BSlot targetSlot, int before, int after, int changeValue)
        {
            if (targetSlot?.Card == null) return;

            Changes.Add((statType, true, targetSlot.SlotNum, targetSlot.Card.Id, targetSlot.SlotCardType, before, after, changeValue));
        }

        /// <summary>
        /// 핸드 카드용 변경사항 기록 (슬롯이 없는 경우)
        /// </summary>
        protected void RecordHandCardChange(StatType statType, BaseCard card, int before, int after, int changeValue)
        {
            if (card == null) return;

            // 핸드 카드는 슬롯이 없으므로 -1로 설정
            Changes.Add((statType, true, -1, card.Id, card.CardType, before, after, changeValue));
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
            if (!ValidateHandCard(card)) return false;

            int before = card.Mana;
            bool success = false;

            switch (card)
            {
                case CreatureCard creatureCard:
                    if (isDecrease)
                        creatureCard.DecreaseMana(value);
                    else
                        creatureCard.IncreaseMana(value);
                    success = true;
                    break;

                case MagicCard magicCard:
                    if (isDecrease)
                        magicCard.DecreaseMana(value);
                    else
                        magicCard.IncreaseMana(value);
                    success = true;
                    break;

                default:
                    LogAbility($"마나 변경을 지원하지 않는 카드 타입: {card.GetType().Name}", LogType.Warning);
                    return false;
            }

            if (success)
            {
                int changeValue = isDecrease ? -value : value;
                RecordHandCardChange(StatType.Mana, card, before, card.Mana, changeValue);
                LogAbility($"마나 변경: {before} -> {card.Mana} (변경량: {changeValue})");
            }

            return success;
        }
    }
}