using System.Collections;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 어빌리티 구현을 위한 기본 추상 클래스
    /// </summary>
    public abstract class BaseAbility : MonoBehaviour, IAbility
    {
        public abstract AbilityType AbilityType { get; }

        protected virtual IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 각 어빌리티의 실제 효과 구현
            yield break;
        }

        public virtual IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            // 1. 효과 적용
            yield return StartCoroutine(ApplyEffect(cardAbility, selfSlot, targetSlot));

            // 2. duration 감소
            cardAbility.DecreaseDuration(TurnManager.Instance.TurnCount);

            // 3. duration 0 이면 release 처리는 외부에서
            // if (cardAbility.duration <= 0)
            // {
            //     // ability 해제
            //     yield return StartCoroutine(Release(cardAbility, selfSlot, targetSlot));
            //     // ability 제거
            //     if (targetSlot.Card != null)
            //     {
            //         targetSlot.Card.AbilitySystem.RemoveCardAbility(cardAbility);
            //     }
            // }

            // 4. 아이콘 갱신
            targetSlot.DrawAbilityIcons();
        }

        public abstract IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot);

        /// <summary>
        /// 대상 슬롯 검증 - 공통 검증 로직
        /// </summary>
        protected bool ValidateTargetSlot(BoardSlot targetSlot, string abilityName)
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
                AbilityType.Damage or AbilityType.Burn or AbilityType.Poison or AbilityType.MaxSatietyGauge => $"<color=red>[{AbilityType}]</color> {message}",
                AbilityType.Heal => $"<color=green>[{AbilityType}]</color> {message}",
                AbilityType.AtkUp or AbilityType.DefUp => $"<color=blue>[{AbilityType}]</color> {message}",
                AbilityType.ReducedMana => $"<color=cyan>[{AbilityType}]</color> {message}",
                AbilityType.SubSatiety => $"<color=yellow>[{AbilityType}]</color> {message}",
                AbilityType.Stun => $"<color=purple>[{AbilityType}]</color> {message}",
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
}