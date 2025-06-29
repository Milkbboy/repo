using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class ChainAbilityManager : MonoBehaviour
    {
        public static ChainAbilityManager Instance { get; private set; }

        [Header("디버그")]
        public bool enableDebugLog = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SubscribeToEvents();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            ChainAbilityEvents.OnDamageDealt += HandleDamageDealt;
            ChainAbilityEvents.OnDamageReceived += HandleDamageReceived;
            ChainAbilityEvents.OnAbilityCompleted += HandleAbilityCompleted;
            ChainAbilityEvents.OnTurnStart += HandleTurnStart;
            ChainAbilityEvents.OnTurnEnd += HandleTurnEnd;
            ChainAbilityEvents.OnCardPlayed += HandleCardPlayed;
        }

        private void UnsubscribeFromEvents()
        {
            ChainAbilityEvents.OnDamageDealt -= HandleDamageDealt;
            ChainAbilityEvents.OnDamageReceived -= HandleDamageReceived;
            ChainAbilityEvents.OnAbilityCompleted -= HandleAbilityCompleted;
            ChainAbilityEvents.OnTurnStart -= HandleTurnStart;
            ChainAbilityEvents.OnTurnEnd -= HandleTurnEnd;
            ChainAbilityEvents.OnCardPlayed -= HandleCardPlayed;
        }

        private void HandleDamageDealt(ChainAbilityEventData eventData)
        {
            StartCoroutine(ProcessChainAbility(eventData));
        }

        private void HandleDamageReceived(ChainAbilityEventData eventData)
        {
            StartCoroutine(ProcessChainAbility(eventData));
        }

        private void HandleAbilityCompleted(ChainAbilityEventData eventData)
        {
            StartCoroutine(ProcessChainAbility(eventData));
        }

        private void HandleTurnStart(ChainAbilityEventData eventData)
        {
            StartCoroutine(ProcessChainAbility(eventData));
        }

        private void HandleTurnEnd(ChainAbilityEventData eventData)
        {
            StartCoroutine(ProcessChainAbility(eventData));
        }

        private void HandleCardPlayed(ChainAbilityEventData eventData)
        {
            StartCoroutine(ProcessChainAbility(eventData));
        }

        private IEnumerator ProcessChainAbility(ChainAbilityEventData eventData)
        {
            if (enableDebugLog)
                Debug.Log($"<color=cyan>[ChainAbility]</color> 이벤트 처리: {eventData.trigger}, 소스 어빌리티 ID: {eventData.sourceAbilityId}");

            // 해당 트리거에 맞는 체인 어빌리티들 찾기
            var chainAiDataList = FindChainAbilitiesForTrigger(eventData);

            foreach (var chainAiData in chainAiDataList)
            {
                if (enableDebugLog)
                    Debug.Log($"<color=yellow>[ChainAbility]</color> 체인 어빌리티 실행: {chainAiData.name}");

                // 체인 AiData 실행 (기존 시스템 활용)
                yield return StartCoroutine(ExecuteChainAiData(chainAiData, eventData));
            }
        }

        private List<AiData> FindChainAbilitiesForTrigger(ChainAbilityEventData eventData)
        {
            var result = new List<AiData>();

            // ⭐ 모든 AiData에서 해당 트리거 조건을 만족하는 체인 찾기
            foreach (var aiData in AiData.ai_list)
            {
                // 체인 설정이 있고, 트리거가 일치하는지 확인
                if (aiData.chainAiDataId != 0 && aiData.chainTrigger == eventData.trigger)
                {
                    // 추가 조건 확인 (있는 경우)
                    // if (CheckChainCondition(aiData, eventData))
                    // {
                    var chainAiData = AiData.GetAiData(aiData.chainAiDataId);
                    if (chainAiData != null)
                    {
                        result.Add(chainAiData);

                        if (enableDebugLog)
                            Debug.Log($"<color=green>[ChainAbility]</color> 체인 조건 만족: {aiData.name} -> {chainAiData.name} (트리거: {eventData.trigger})");
                    }
                    // }
                }
            }

            return result;
        }

        /// <summary>
        /// 체인 어빌리티 추가 조건 확인 (간소화)
        /// </summary>
        // private bool CheckChainCondition(AiData sourceAiData, ChainAbilityEventData eventData)
        // {
        //     // 조건이 없으면 항상 발동
        //     if (string.IsNullOrEmpty(sourceAiData.chainCondition))
        //         return true;

        //     // 간단한 조건들 (실제 구현에서는 더 정교한 로직 필요)
        //     string condition = sourceAiData.chainCondition.Replace(" ", "");

        //     switch (condition)
        //     {
        //         case "HP<50%":
        //         case "HP<25%":
        //         case "Mana>=10":
        //         case "Damage>=20":
        //         case "FirstAttack":
        //             // 실제 조건 확인은 게임 상태에 따라 구현
        //             return true; // 임시로 항상 true

        //         default:
        //             if (enableDebugLog)
        //                 Debug.LogWarning($"<color=orange>[ChainAbility]</color> 알 수 없는 조건: {condition}");
        //             return true; // 알 수 없는 조건은 무시
        //     }
        // }

        private IEnumerator ExecuteChainAiData(AiData chainAiData, ChainAbilityEventData eventData)
        {
            if (enableDebugLog)
                Debug.Log($"<color=yellow>[ChainAbility]</color> 체인 AiData 실행: {chainAiData.name}, 타겟: {chainAiData.target}");

            // ⭐ 기존 어빌리티 시스템 활용 - AbilityDamage, AbilityHeal 등의 클래스들 사용
            foreach (var abilityId in chainAiData.ability_Ids)
            {
                var abilityData = AbilityData.GetAbilityData(abilityId);
                if (abilityData == null) continue;

                List<BoardSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(chainAiData, eventData.sourceSlot);

                yield return StartCoroutine(ExecuteChainAbility(abilityData, eventData.sourceSlot, targetSlots));

                if (enableDebugLog)
                    Debug.Log($"<color=green>[ChainAbility]</color> 어빌리티 실행 완료: {abilityData.nameDesc}");

                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator ExecuteChainAbility(AbilityData abilityData, BoardSlot sourceSlot, List<BoardSlot> targetSlots)
        {
            // 어빌리티 타입에 따라 실행
            switch (abilityData.abilityType)
            {
                case AbilityType.Damage:
                    yield return ExecuteDamage(abilityData, sourceSlot, targetSlots);
                    break;

                case AbilityType.AddSatiety:
                    yield return ExecuteAddSatiety(abilityData, targetSlots);
                    break;

                case AbilityType.AddMana:
                    yield return ExecuteAddMana(abilityData, targetSlots);
                    break;

                case AbilityType.Heal:
                    yield return ExecuteHeal(abilityData, targetSlots);
                    break;

                case AbilityType.AtkUp:
                    yield return ExecuteAtkUp(abilityData, targetSlots);
                    break;

                case AbilityType.DefUp:
                    yield return ExecuteDefUp(abilityData, targetSlots);
                    break;

                case AbilityType.Poison:
                    yield return ExecutePoison(abilityData, targetSlots);
                    break;

                case AbilityType.Burn:
                    yield return ExecuteBurn(abilityData, targetSlots);
                    break;

                case AbilityType.Weaken:
                    yield return ExecuteWeaken(abilityData, targetSlots);
                    break;

                case AbilityType.ArmorBreak:
                    yield return ExecuteArmorBreak(abilityData, targetSlots);
                    break;

                default:
                    if (enableDebugLog)
                        Debug.LogWarning($"<color=orange>[ChainAbility]</color> 지원하지 않는 체인 어빌리티 타입: {abilityData.abilityType}");
                    break;
            }
        }

        /// <summary>
        /// 체인 어빌리티 데미지 처리 (간소화된 버전)
        /// </summary>
        private IEnumerator ExecuteDamage(AbilityData abilityData, BoardSlot sourceSlot, List<BoardSlot> targetSlots)
        {
            if (targetSlots.Count == 0)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"<color=orange>[ChainAbility]</color> 데미지 대상이 없습니다.");
                yield break;
            }

            foreach (var targetSlot in targetSlots)
            {
                // 데미지를 받을 수 있는 카드인지 확인
                if (!(targetSlot.Card is CreatureCard) && !(targetSlot.Card is MasterCard))
                {
                    if (enableDebugLog)
                        Debug.LogWarning($"<color=orange>[ChainAbility]</color> 데미지를 받을 수 없는 카드 타입: {targetSlot.Card.CardType}");
                    yield break;
                }

                int damage = abilityData.value;

                // 비율 기반 데미지 계산 (선택적)
                if (abilityData.ratio > 0f && sourceSlot?.Card is CreatureCard sourceCreature)
                {
                    damage = Mathf.RoundToInt(sourceCreature.Atk * abilityData.ratio);
                }

                if (enableDebugLog)
                    Debug.Log($"<color=red>[ChainAbility]</color> 체인 데미지: {damage} ({sourceSlot?.ToSlotLogInfo() ?? "Unknown"} → {targetSlot.ToSlotLogInfo()})");

                // 간소화된 데미지 적용 (애니메이션 없이)
                yield return StartCoroutine(targetSlot.TakeDamage(damage));

                if (enableDebugLog)
                    Debug.Log($"<color=red>[ChainAbility]</color> 체력 감소: -{damage}, 남은 HP: {targetSlot.Card?.Hp ?? 0}");

                yield return new WaitForSeconds(0.1f);
            }

            // 체인 데미지로 인한 추가 체인 어빌리티 방지 (무한 루프 방지)
            // ChainAbilityEvents.TriggerDamageDealt는 호출하지 않음
        }

        /// <summary>
        /// 기존 AbilityDamage 시스템을 재사용하는 방법 (대안)
        /// </summary>
        private IEnumerator ExecuteDamageWithFullSystem(AbilityData abilityData, BoardSlot sourceSlot, BoardSlot targetSlot)
        {
            // 기존 AbilityDamage 클래스를 직접 사용하는 방법
            var damageAbility = FindObjectOfType<AbilityDamage>();
            if (damageAbility != null)
            {
                // CardAbility 객체 생성
                var chainCardAbility = new CardAbility
                {
                    abilityId = abilityData.abilityId,
                    abilityType = abilityData.abilityType,
                    abilityValue = abilityData.value,
                    aiDataId = 0 // 체인 어빌리티는 별도 AiData 없음
                };

                // 기존 시스템으로 데미지 적용 (단, 체인 이벤트 발생 방지 필요)
                yield return StartCoroutine(damageAbility.ApplySingle(chainCardAbility, sourceSlot, targetSlot));
            }
            else
            {
                if (enableDebugLog)
                    Debug.LogError($"<color=red>[ChainAbility]</color> AbilityDamage 컴포넌트를 찾을 수 없습니다.");
            }
        }

        private IEnumerator ExecuteAddSatiety(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card is MasterCard masterCard)
                {
                    int satietyIncrease = abilityData.value;
                    BattleController.Instance.UpdateSatietyGauge(satietyIncrease);

                    if (enableDebugLog)
                        Debug.Log($"<color=green>[ChainAbility]</color> 만복도 증가: +{satietyIncrease} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteAddMana(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card is MasterCard masterCard)
                {
                    int manaIncrease = abilityData.value;
                    targetSlot.IncreaseMana(manaIncrease);

                    if (enableDebugLog)
                        Debug.Log($"<color=blue>[ChainAbility]</color> 마나 증가: +{manaIncrease} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteHeal(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card != null)
                {
                    int healAmount = abilityData.value;
                    targetSlot.Card.RestoreHealth(healAmount);

                    if (enableDebugLog)
                        Debug.Log($"<color=green>[ChainAbility]</color> 체력 회복: +{healAmount} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteAtkUp(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card is CreatureCard creatureCard)
                {
                    int atkIncrease = abilityData.value;
                    creatureCard.IncreaseAttack(atkIncrease);

                    if (enableDebugLog)
                        Debug.Log($"<color=red>[ChainAbility]</color> 공격력 증가: +{atkIncrease} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteDefUp(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card is CreatureCard creatureCard)
                {
                    int defIncrease = abilityData.value;
                    creatureCard.IncreaseDefense(defIncrease);

                    if (enableDebugLog)
                        Debug.Log($"<color=blue>[ChainAbility]</color> 방어력 증가: +{defIncrease} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecutePoison(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card != null)
                {
                    // 독 상태 적용 (기존 상태이상 시스템 활용)
                    int poisonDamage = abilityData.value;
                    int duration = abilityData.duration > 0 ? abilityData.duration : 3; // 기본 3턴

                    // 기존 독 어빌리티 시스템을 활용하거나 직접 구현
                    // targetSlot.Card.ApplyPoison(poisonDamage, duration);

                    if (enableDebugLog)
                        Debug.Log($"<color=green>[ChainAbility]</color> 독 적용: {poisonDamage} 데미지 {duration}턴 ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteBurn(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card != null)
                {
                    // 화상 상태 적용
                    int burnDamage = abilityData.value;
                    int duration = abilityData.duration > 0 ? abilityData.duration : 2; // 기본 2턴

                    if (enableDebugLog)
                        Debug.Log($"<color=orange>[ChainAbility]</color> 화상 적용: {burnDamage} 데미지 {duration}턴 ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteWeaken(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card is CreatureCard creatureCard)
                {
                    int weakenAmount = abilityData.value;
                    creatureCard.DecreaseAttack(weakenAmount);

                    if (enableDebugLog)
                        Debug.Log($"<color=purple>[ChainAbility]</color> 공격력 감소: -{weakenAmount} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ExecuteArmorBreak(AbilityData abilityData, List<BoardSlot> targetSlots)
        {
            foreach (var targetSlot in targetSlots)
            {
                if (targetSlot?.Card != null)
                {
                    int armorBreakAmount = abilityData.value;
                    targetSlot.Card.DecreaseDefense(armorBreakAmount);

                    if (enableDebugLog)
                        Debug.Log($"<color=brown>[ChainAbility]</color> 방어력 감소: -{armorBreakAmount} ({targetSlot.ToSlotLogInfo()})");
                }
            }
            yield return null;
        }

        private IEnumerator ShowVisualFeedback(AbilityData abilityData, BoardSlot targetSlot)
        {
            if (targetSlot == null) yield break;

            string feedbackText = abilityData.abilityType switch
            {
                AbilityType.AddSatiety => $"+{abilityData.value} 만복도",
                AbilityType.AddMana => $"+{abilityData.value} 마나",
                AbilityType.Heal => $"+{abilityData.value} HP",
                AbilityType.AtkUp => $"+{abilityData.value} ATK",
                AbilityType.DefUp => $"+{abilityData.value} DEF",
                _ => $"+{abilityData.value}"
            };

            // FloatingText 표시 (기존 시스템 활용)
            // FloatingTextManager.ShowText(targetSlot.transform.position, feedbackText, Color.cyan);

            if (enableDebugLog)
                Debug.Log($"<color=cyan>[ChainAbility]</color> 피드백 표시: {feedbackText} ({targetSlot.ToSlotLogInfo()})");

            yield return new WaitForSeconds(0.3f);
        }

        // 헬퍼 메서드들 (실제 게임 구조에 맞게 구현 필요)
        private BoardSlot GetPlayerMasterSlot(BoardSlot sourceSlot)
        {
            // 플레이어 마스터 슬롯 반환 로직
            // 실제 BattleManager나 BoardLogic에서 구현
            return null;
        }

        private BoardSlot GetEnemyMasterSlot(BoardSlot sourceSlot)
        {
            // 적 마스터 슬롯 반환 로직
            return null;
        }

        private List<BoardSlot> GetAllFriendlyCreatureSlots(BoardSlot sourceSlot)
        {
            // 모든 아군 크리처 슬롯 반환 로직
            return new List<BoardSlot>();
        }

        private BoardSlot GetRandomEnemySlot(BoardSlot sourceSlot)
        {
            // 랜덤 적 슬롯 반환 로직
            return null;
        }
    }
}