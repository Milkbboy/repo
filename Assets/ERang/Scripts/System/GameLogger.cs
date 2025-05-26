using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ERang
{
    public enum LogCategory
    {
        // 최우선 - 게임 진행 흐름
        GAME_FLOW,   // 턴, 배틀, 페이즈 전환

        // 높은 우선순위 - 카드 시스템
        CARD,        // 카드 생성, 사용, 상태변화
        CARD_STATE,  // 카드 스탯 변화 상세

        // 중간 우선순위 - AI 시스템  
        AI,          // AI 행동, 결정

        // 중간 우선순위 - 어빌리티 시스템
        ABILITY,     // 어빌리티 발동, 효과
        ABILITY_DETAIL, // 어빌리티 상세 로그 (토글 가능)

        // 낮은 우선순위
        DATA,        // 데이터 로드, 테이블 검증
        UI,          // UI 업데이트
        AUDIO,       // 오디오 재생
        ERROR,       // 에러, 예외상황
        DEBUG        // 개발용 디버그
    }

    public static class GameLogger
    {
        private static readonly Dictionary<LogCategory, string> CategoryColors = new()
        {
            { LogCategory.GAME_FLOW, "#2196F3" },     // 파란색 - 최우선
            { LogCategory.CARD, "#FF9800" },          // 주황색 - 높은 우선순위
            { LogCategory.CARD_STATE, "#FF5722" },    // 진한 주황색 - 카드 상태변화
            { LogCategory.AI, "#9C27B0" },            // 보라색 - 중간 우선순위
            { LogCategory.ABILITY, "#F44336" },       // 빨간색 - 중간 우선순위
            { LogCategory.ABILITY_DETAIL, "#E91E63" }, // 핑크 - 어빌리티 상세
            { LogCategory.DATA, "#4CAF50" },          // 녹색
            { LogCategory.UI, "#607D8B" },            // 회색
            { LogCategory.AUDIO, "#795548" },         // 갈색
            { LogCategory.ERROR, "#D32F2F" },         // 진한 빨간색
            { LogCategory.DEBUG, "#FFC107" }          // 노란색
        };

        private static readonly Dictionary<LogCategory, bool> CategoryEnabled = new()
        {
            { LogCategory.GAME_FLOW, true },
            { LogCategory.CARD, true },
            { LogCategory.CARD_STATE, true },
            { LogCategory.AI, true },
            { LogCategory.ABILITY, true },
            { LogCategory.ABILITY_DETAIL, false }, // 기본적으로 비활성화
            { LogCategory.DATA, true },
            { LogCategory.UI, false },
            { LogCategory.AUDIO, false },
            { LogCategory.ERROR, true },
            { LogCategory.DEBUG, true }
        };

        // 성능 모드 (릴리즈 빌드에서 로그 비활성화)
        public static bool IsLoggingEnabled
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return PlayerPrefsUtility.GetValue<bool>("EnableLogging", false);
#endif
            }
        }

        public static void Log(LogCategory category, string message, Object context = null)
        {
            if (!IsLoggingEnabled) return;
            if (!CategoryEnabled.GetValueOrDefault(category, true)) return;

            // 통계 카운트 증가
            IncrementLogCount(category);

            string color = CategoryColors[category];
            string formattedMessage = $"<color={color}>[{category}]</color> {message}";

            switch (category)
            {
                case LogCategory.ERROR:
                    Debug.LogError(formattedMessage, context);
                    break;
                default:
                    Debug.Log(formattedMessage, context);
                    break;
            }
        }

        // 게임 플로우 로그 (기획자도 쉽게 이해)
        public static void LogGameFlow(string phase, string details = "")
        {
            if (!IsLoggingEnabled) return;

            Log(LogCategory.GAME_FLOW, $"🎯 {phase.ToUpper()}{(string.IsNullOrEmpty(details) ? "" : $" - {details}")}");
        }

        // 카드 사용 체인 추적
        public static void LogCardChain(string cardName, string action, string target = "", string result = "")
        {
            if (!IsLoggingEnabled) return;

            string chainLog = $"🃏 {cardName} ➤ {action}";
            if (!string.IsNullOrEmpty(target)) chainLog += $" → {target}";
            if (!string.IsNullOrEmpty(result)) chainLog += $" = {result}";

            Log(LogCategory.CARD, chainLog);
        }

        // 카드 상태 변화 (기획자 확인용)
        public static void LogCardState(string cardName, string property, object oldValue, object newValue, string reason = "")
        {
            if (!IsLoggingEnabled) return;

            string reasonText = string.IsNullOrEmpty(reason) ? "" : $" ({reason})";
            Log(LogCategory.CARD_STATE, $"📊 {cardName} {property}: {oldValue} → {newValue}{reasonText}");
        }

        // AI 행동 로그
        public static void LogAI(string actorName, string action, string target = "", string result = "")
        {
            if (!IsLoggingEnabled) return;

            string aiLog = $"🤖 {actorName} {action}";
            if (!string.IsNullOrEmpty(target)) aiLog += $" → {target}";
            if (!string.IsNullOrEmpty(result)) aiLog += $" = {result}";

            Log(LogCategory.AI, aiLog);
        }

        // 어빌리티 발동 로그
        public static void LogAbility(string abilityName, string source, string target, string phase = "발동")
        {
            if (!IsLoggingEnabled) return;

            Log(LogCategory.ABILITY, $"🔥 {abilityName} {phase}: {source} → {target}");
        }

        // 어빌리티 시작/완료 로그 (Phase 2 추가)
        public static void LogAbilityStart(string abilityName, string source, string target, string additionalInfo = "")
        {
            if (!IsLoggingEnabled) return;

            string info = string.IsNullOrEmpty(additionalInfo) ? "" : $" - {additionalInfo}";
            Log(LogCategory.ABILITY, $"🔥 {abilityName} 시작: {source} → {target}{info}");
        }

        public static void LogAbilityComplete(string abilityName, string source, string target, string result = "")
        {
            if (!IsLoggingEnabled) return;

            string resultText = string.IsNullOrEmpty(result) ? "" : $" - {result}";
            Log(LogCategory.ABILITY, $"🔥 {abilityName} 완료: {source} → {target}{resultText}");
        }

        // 어빌리티 상세 로그
        public static void LogAbilityDetail(string message)
        {
            if (!IsLoggingEnabled) return;
            if (!CategoryEnabled[LogCategory.ABILITY_DETAIL]) return;

            Log(LogCategory.ABILITY_DETAIL, $"   🔍 {message}");
        }

        // 데이터 검증 로그 (기획자용)
        public static void LogDataValidation(string tableName, int recordCount, string validation = "")
        {
            if (!IsLoggingEnabled) return;

            string validationText = string.IsNullOrEmpty(validation) ? "✅" : validation;
            Log(LogCategory.DATA, $"📋 {tableName}: {recordCount}개 레코드 로드 {validationText}");
        }

        // 배틀 요약 로그 (기획자용)
        public static void LogBattleSummary(int turnCount, int masterHp, int maxHp, int usedCards, int totalDamage)
        {
            if (!IsLoggingEnabled) return;

            var summary = new StringBuilder();
            summary.AppendLine("📊 배틀 요약");
            summary.AppendLine($"   턴 수: {turnCount}");
            summary.AppendLine($"   마스터 HP: {masterHp}/{maxHp}");
            summary.AppendLine($"   사용한 카드: {usedCards}장");
            summary.AppendLine($"   입힌 총 데미지: {totalDamage}");

            Log(LogCategory.GAME_FLOW, summary.ToString());
        }

        // 카테고리 활성화/비활성화
        public static void SetCategoryEnabled(LogCategory category, bool enabled)
        {
            CategoryEnabled[category] = enabled;
            Log(LogCategory.DEBUG, $"{category} 로그: {(enabled ? "활성화" : "비활성화")}");
        }

        // 어빌리티 상세 로그 토글
        public static void EnableAbilityDetail(bool enable)
        {
            SetCategoryEnabled(LogCategory.ABILITY_DETAIL, enable);
        }

        // 모든 카테고리 상태 출력
        public static void PrintCategoryStatus()
        {
            if (!IsLoggingEnabled) return;

            var status = new StringBuilder();
            status.AppendLine("📊 로그 카테고리 상태");

            foreach (var kvp in CategoryEnabled)
            {
                string emoji = kvp.Value ? "✅" : "❌";
                status.AppendLine($"   {emoji} {kvp.Key}");
            }

            Log(LogCategory.DEBUG, status.ToString());
        }

        // 런타임 컨트롤을 위한 업데이트 (MonoBehaviour에서 호출)
        public static void HandleRuntimeInput()
        {
            if (!IsLoggingEnabled) return;

            // F1: 어빌리티 상세 로그 토글
            if (Input.GetKeyDown(KeyCode.F1))
            {
                EnableAbilityDetail(!CategoryEnabled[LogCategory.ABILITY_DETAIL]);
            }

            // F2: 카테고리 상태 출력
            if (Input.GetKeyDown(KeyCode.F2))
            {
                PrintCategoryStatus();
            }

            // F3: AI 로그 토글
            if (Input.GetKeyDown(KeyCode.F3))
            {
                SetCategoryEnabled(LogCategory.AI, !CategoryEnabled[LogCategory.AI]);
            }

            // F4: 카드 상태 로그 토글 (Phase 2 추가)
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SetCategoryEnabled(LogCategory.CARD_STATE, !CategoryEnabled[LogCategory.CARD_STATE]);
            }

            // F5: 에러만 보기 모드 토글
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ToggleErrorOnlyMode();
            }
        }

        // 에러만 보기 모드 토글
        private static bool _errorOnlyMode = false;
        public static void ToggleErrorOnlyMode()
        {
            _errorOnlyMode = !_errorOnlyMode;
            
            if (_errorOnlyMode)
            {
                // 에러만 보기 모드: 에러와 게임 플로우만 활성화
                foreach (var category in System.Enum.GetValues(typeof(LogCategory)))
                {
                    var cat = (LogCategory)category;
                    CategoryEnabled[cat] = cat == LogCategory.ERROR || cat == LogCategory.GAME_FLOW;
                }
                Log(LogCategory.DEBUG, "⚠️ 에러만 보기 모드 활성화 (에러 + 게임플로우만)");
            }
            else
            {
                // 모든 로그 복원 (기본값으로)
                CategoryEnabled[LogCategory.GAME_FLOW] = true;
                CategoryEnabled[LogCategory.CARD] = true;
                CategoryEnabled[LogCategory.CARD_STATE] = true;
                CategoryEnabled[LogCategory.AI] = true;
                CategoryEnabled[LogCategory.ABILITY] = true;
                CategoryEnabled[LogCategory.ABILITY_DETAIL] = false;
                CategoryEnabled[LogCategory.DATA] = true;
                CategoryEnabled[LogCategory.UI] = false;
                CategoryEnabled[LogCategory.AUDIO] = false;
                CategoryEnabled[LogCategory.ERROR] = true;
                CategoryEnabled[LogCategory.DEBUG] = true;
                Log(LogCategory.DEBUG, "✅ 모든 로그 모드 복원");
            }
        }
        
        // 성능 측정 및 통계 (Phase 2 추가)
        private static readonly System.Collections.Generic.Dictionary<LogCategory, int> _logCounts = new();
        
        public static void IncrementLogCount(LogCategory category)
        {
            if (!_logCounts.ContainsKey(category))
                _logCounts[category] = 0;
            _logCounts[category]++;
        }
        
        public static void PrintLogStatistics()
        {
            if (!IsLoggingEnabled) return;
            
            var stats = new StringBuilder();
            stats.AppendLine("📈 로그 통계 (세션 시작 이후)");
            
            foreach (var kvp in _logCounts)
            {
                stats.AppendLine($"   {kvp.Key}: {kvp.Value}회");
            }
            
            Log(LogCategory.DEBUG, stats.ToString());
        }
        
        // F6 키 추가: 로그 통계 출력
        public static void HandleAdditionalInput()
        {
            if (!IsLoggingEnabled) return;
            
            if (Input.GetKeyDown(KeyCode.F6))
            {
                PrintLogStatistics();
            }
        }
    }
}