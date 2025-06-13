using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 계단 현상 없는 평면적 로그 시스템
    /// Static 메서드로 간단하게 호출
    /// </summary>
    public static class FlatLogger
    {
        // 로그 활성화 설정
        public static bool EnableLogging = true;
        public static bool EnableColorLogging = true;

        public static void LogCard(string cardInfo)
        {
            if (!EnableLogging) return;

            if (EnableColorLogging)
                Debug.Log($"<color=cyan>[카드 사용]</color> {cardInfo}");
            else
                Debug.Log($"[카드 사용] {cardInfo}");
        }

        public static void LogAiGroup(int aiGroupId)
        {
            if (!EnableLogging) return;

            if (EnableColorLogging)
                Debug.Log($" └▶ <color=yellow>[AiGroup]</color> AiGroupId: {aiGroupId} 선택");
            else
                Debug.Log($" └▶ [AiGroup] AiGroupId: {aiGroupId} 선택");
        }

        public static void LogAiData(AiData aiData)
        {
            if (!EnableLogging) return;

            if (EnableColorLogging)
                Debug.Log($"     └▶ <color=orange>[AiData]</color> AiDataId: {aiData.ai_Id} ({aiData.name}) 선택");
            else
                Debug.Log($"     └▶ [AiData] AiDataId: {aiData.ai_Id} ({aiData.name}) 선택");
        }

        public static void LogAbility(int abilityId, string abilityName)
        {
            if (!EnableLogging) return;

            if (EnableColorLogging)
                Debug.Log($"         └▶ <color=lime>[Ability]</color> AbilityId: {abilityId} ({abilityName}) 실행");
            else
                Debug.Log($"         └▶ [Ability] AbilityId: {abilityId} ({abilityName}) 실행");
        }

        public static void LogEffect(string target, string effect)
        {
            if (!EnableLogging) return;

            if (EnableColorLogging)
                Debug.Log($"             └▶ <color=lightblue>[Effect]</color> 대상: {target}, 효과: {effect}");
            else
                Debug.Log($"             └▶ [Effect] 대상: {target}, 효과: {effect}");
        }

        // 🆕 추가 유틸리티 메서드들
        public static void LogError(string message)
        {
            Debug.LogError($"<color=red>[ERROR]</color> {message}");
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning($"<color=orange>[WARNING]</color> {message}");
        }

        public static void LogSeparator()
        {
            if (!EnableLogging) return;
            Debug.Log("─────────────────────────────────────────");
        }
    }

    /// <summary>
    /// 확장 메서드 (기존 코드와 호환성)
    /// </summary>
    public static class ERangLogExtensions
    {
        public static string ToCardLogInfo(this BaseCard card)
        {
            return $"<b><color=orange>{card.Id}</color></b> <color=lightblue>{card.Name}</color> 카드";
        }

        public static string ToSlotLogInfo(this BSlot slot)
        {
            if (slot?.Card == null)
                return $"{slot?.SlotNum ?? -1}번 슬롯 (빈 슬롯)";

            return $"<color=cyan>[{slot.SlotNum}번 슬롯]</color> {GetCardType(slot.Card.CardType)} 카드({slot.Card.Id})";
        }

        public static string ToAbilityLogInfo(this AbilityData abilityData)
        {
            return $"{abilityData.nameDesc} 어빌리티({abilityData.abilityId})";
        }

        public static string ToCardAbilityLogInfo(this CardAbility cardAbility)
        {
            CardData cardData = CardData.GetCardData(cardAbility.cardId);
            return $"{cardData.nameDesc} 카드 {cardAbility.abilityType} {cardAbility.abilityName} 어빌리티({cardAbility.abilityId})";
        }

        public static string GetEffectDescription(this AbilityData abilityData)
        {
            return abilityData.abilityType switch
            {
                AbilityType.Damage => $"데미지 {abilityData.value}",
                AbilityType.Heal => $"회복 {abilityData.value}",
                AbilityType.AtkUp => $"공격력 {abilityData.value} 증가",
                AbilityType.DefUp => $"방어력 {abilityData.value} 증가",
                AbilityType.BrokenDef => $"방어력 {abilityData.value} 감소",
                AbilityType.Burn => $"화상 상태 부여 (턴당 {abilityData.value} 데미지)",
                AbilityType.Poison => $"중독 상태 부여 (턴당 {abilityData.value} 데미지)",
                AbilityType.ArmorBreak => "방어구 파괴 상태 부여",
                AbilityType.AddMana => $"마나 {abilityData.value} 증가",
                AbilityType.SubMana => $"마나 {abilityData.value} 감소",
                AbilityType.AddSatiety => $"포만감 {abilityData.value} 증가",
                AbilityType.SubSatiety => $"포만감 {abilityData.value} 감소",
                _ => $"{abilityData.nameDesc} 효과 적용"
            };
        }
        
        private static string GetCardType(CardType cardType)
        {
            return cardType switch
            {
                CardType.Master => "마스터",
                CardType.Magic => "마법",
                CardType.Individuality => "전용 마법",
                CardType.Creature => "크리쳐",
                CardType.Building => "건물",
                CardType.Charm => "축복",
                CardType.Curse => "저주",
                CardType.Monster => "몬스터",
                CardType.EnemyMaster => "적 마스터",
                _ => "없음",
            };
        }
    }
}