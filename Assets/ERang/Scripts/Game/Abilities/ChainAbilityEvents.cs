using System;
using UnityEngine;

namespace ERang
{
    public enum ChainAbilityTrigger
    {
        None,           // 체인 없음
        DamageDealt,    // 데미지를 가했을 때
        DamageReceived, // 데미지를 받았을 때
        AbilityCompleted, // 어빌리티 완료 시
        TurnStart,      // 턴 시작 시
        TurnEnd,        // 턴 종료 시
        CardPlayed,     // 카드 사용 시
        CardDestroyed,  // 카드 파괴 시
        HPBelow50,      // HP 50% 이하일 때
        HPBelow25,      // HP 25% 이하일 때
        ManaFull,       // 마나 가득 찰 때
        KillEnemy,      // 적을 처치했을 때
        SummonCard,     // 카드 소환 시
        BuffApplied,    // 버프 적용 시
        DebuffApplied   // 디버프 적용 시
    }

    [System.Serializable]
    public class ChainAbilityEventData
    {
        public ChainAbilityTrigger trigger;
        public BoardSlot sourceSlot;
        public int sourceAbilityId; // 트리거를 발생시킨 어빌리티 ID

        public ChainAbilityEventData(ChainAbilityTrigger trigger, BoardSlot sourceSlot, int sourceAbilityId)
        {
            this.trigger = trigger;
            this.sourceSlot = sourceSlot;
            this.sourceAbilityId = sourceAbilityId;
        }
    }

    public static class ChainAbilityEvents
    {
        // 이벤트 정의
        public static event Action<ChainAbilityEventData> OnDamageDealt;
        public static event Action<ChainAbilityEventData> OnDamageReceived;
        public static event Action<ChainAbilityEventData> OnAbilityCompleted;
        public static event Action<ChainAbilityEventData> OnTurnStart;
        public static event Action<ChainAbilityEventData> OnTurnEnd;
        public static event Action<ChainAbilityEventData> OnCardPlayed;
        public static event Action<ChainAbilityEventData> OnCardDestroyed;

        // 트리거 메서드들 - 단순화
        public static void TriggerDamageDealt(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.DamageDealt, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerDamageReceived(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.DamageReceived, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerAbilityCompleted(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.AbilityCompleted, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerTurnStart(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.TurnStart, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerTurnEnd(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.TurnEnd, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerCardPlayed(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.CardPlayed, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerCardDestroyed(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.CardDestroyed, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerKillEnemy(BoardSlot sourceSlot, int sourceAbilityId)
        {
            var eventData = new ChainAbilityEventData(ChainAbilityTrigger.KillEnemy, sourceSlot, sourceAbilityId);
            TriggerEvent(eventData);
        }

        public static void TriggerHPBelow(BoardSlot sourceSlot, int percentage, int sourceAbilityId)
        {
            var trigger = percentage switch
            {
                50 => ChainAbilityTrigger.HPBelow50,
                25 => ChainAbilityTrigger.HPBelow25,
                _ => ChainAbilityTrigger.None
            };

            if (trigger != ChainAbilityTrigger.None)
            {
                var eventData = new ChainAbilityEventData(trigger, sourceSlot, sourceAbilityId);
                TriggerEvent(eventData);
            }
        }

        private static void TriggerEvent(ChainAbilityEventData eventData)
        {
            switch (eventData.trigger)
            {
                case ChainAbilityTrigger.DamageDealt:
                    OnDamageDealt?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.DamageReceived:
                    OnDamageReceived?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.AbilityCompleted:
                    OnAbilityCompleted?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.TurnStart:
                    OnTurnStart?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.TurnEnd:
                    OnTurnEnd?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.CardPlayed:
                    OnCardPlayed?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.CardDestroyed:
                    OnCardDestroyed?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.HPBelow50:
                case ChainAbilityTrigger.HPBelow25:
                    // HP 조건 트리거들은 DamageReceived 이벤트로 처리
                    OnDamageReceived?.Invoke(eventData);
                    break;
                case ChainAbilityTrigger.KillEnemy:
                    // KillEnemy는 별도 처리 또는 DamageDealt에서 처리
                    OnDamageDealt?.Invoke(eventData);
                    break;
                default:
                    Debug.LogWarning($"<color=orange>[ChainAbility]</color> 처리되지 않은 트리거: {eventData.trigger}");
                    break;
            }
        }
    }
}