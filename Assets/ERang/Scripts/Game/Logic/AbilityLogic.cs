using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;


namespace ERang
{
    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

        // 타입 안전성을 위한 읽기 전용 딕셔너리
        public IReadOnlyDictionary<AbilityType, IAbility> AbilityActions => abilityActions;
        private readonly Dictionary<AbilityType, IAbility> abilityActions = new();
        private readonly Dictionary<AbilityType, IHandAbility> handAbilityActions = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // IAbility 컴포넌트 검색 (성능 개선)
            IAbility[] abilities = GetComponentsInChildren<IAbility>();

            Debug.Log($"<color=cyan>[AbilityLogic]</color> 초기화 시작. 발견된 어빌리티: {abilities.Length}개");

            foreach (IAbility ability in abilities)
            {
                try
                {
                    if (ability == null)
                        throw new InvalidOperationException("어빌리티가 null입니다.");

                    if (abilityActions.ContainsKey(ability.AbilityType))
                    {
                        throw new InvalidOperationException($"어빌리티 {ability.AbilityType} 중복 등록 감지! " +
                            $"Abilities 게임 오브젝트에서 중복된 어빌리티를 제거해주세요.");
                    }

                    abilityActions.Add(ability.AbilityType, ability);

                    // 핸드 어빌리티 별도 등록
                    if (ability is IHandAbility handAbility)
                    {
                        handAbilityActions.Add(ability.AbilityType, handAbility);
                        Debug.Log($"<color=green>[AbilityLogic]</color> 핸드 어빌리티 등록: {ability.AbilityType}");
                    }

                    Debug.Log($"<color=green>[AbilityLogic]</color> 어빌리티 등록 완료: {ability.AbilityType}");

                }
                catch (Exception ex)
                {
                    // 중복 등록 시 게임 중단
                    Debug.LogError($"<color=red>[AbilityLogic]</color> 어빌리티 등록 실패: {ex.Message}");
                    throw; // 게임 중단
                }
            }
        }

        /// <summary>
        /// 어빌리티 추가하고 발동까지
        /// - 몇몇 어빌리티는 발동 안함
        /// </summary>
        public IEnumerator AbilityProcess(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            if (!ValidateAbilityProcess(abilityData, selfSlot, targetSlots))
                yield break;

            FlatLogger.LogAbility(abilityData.abilityId, abilityData.nameDesc);

            // 효과 로깅
            LogAbilityEffects(abilityData, targetSlots);

            List<Coroutine> runningCoroutines = new List<Coroutine>();

            foreach (BSlot targetSlot in targetSlots.Where(slot => slot?.Card != null))
            {
                BaseCard card = targetSlot.Card;

                CardAbility cardAbility = card.AbilitySystem.CardAbilities.Find(cardAbility => cardAbility.abilityId == abilityData.abilityId);

                // 어빌리티 발동 여부
                bool isNewAbility = cardAbility == null;

                cardAbility ??= new()
                {
                    abilityId = abilityData.abilityId,
                    aiDataId = aiData?.ai_Id ?? 0,
                    selfSlotNum = selfSlot.SlotNum,
                    targetSlotNum = targetSlot.SlotNum,

                    aiType = aiData?.type ?? AiDataType.None,
                    abilityType = abilityData.abilityType,
                    abilityValue = abilityData.value,
                    workType = abilityData.workType,
                    duration = abilityData.duration,
                };

                // 핸드 온 어빌리티는 턴 종료시 해제되기 때문에 저장. 효과 지속 시간이 있는 어빌리티 저장.
                if (abilityData.workType == AbilityWorkType.OnHand || abilityData.duration > 0)
                {
                    card.AbilitySystem.AddCardAbility(cardAbility, TurnManager.Instance.TurnCount, whereFrom);
                }

                targetSlot.DrawAbilityIcons();

                // 신규 어빌리티이고 즉시 발동 타입인 경우 실행
                if (isNewAbility && ShouldActivateImmediately(abilityData))
                {
                    if (aiData?.isSameTime ?? false)
                    {
                        runningCoroutines.Add(StartCoroutine(AbilityAction(cardAbility, selfSlot, targetSlot)));
                    }
                    else
                    {
                        yield return StartCoroutine(AbilityAction(cardAbility, selfSlot, targetSlot));
                    }
                }
            }

            // isSameTime이 true일 경우 모든 코루틴이 완료될 때까지 대기
            if (runningCoroutines.Count > 0)
            {
                foreach (var coroutine in runningCoroutines)
                {
                    yield return coroutine;
                }
            }
        }

        private bool ValidateAbilityProcess(AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            if (abilityData == null)
            {
                Debug.LogError("<color=red>[AbilityProcess]</color> AbilityData가 null입니다.");
                return false;
            }

            if (selfSlot == null)
            {
                Debug.LogError("<color=red>[AbilityProcess]</color> selfSlot이 null입니다.");
                return false;
            }

            if (targetSlots == null || targetSlots.Count == 0)
            {
                Debug.LogError("<color=red>[AbilityProcess]</color> targetSlots가 비어있습니다.");
                return false;
            }

            return true;
        }

        // Burn(행동 전), Poison(행동 후) 어빌리티는 즉시 발동하지 않음
        // BattleLogic.BoardTurnEnd에서 별도 타이밍에 실행됨
        private bool ShouldActivateImmediately(AbilityData abilityData)
        {
            return !Constants.CardPriorAbilities.Contains(abilityData.abilityType) &&
                   !Constants.CardPostAbilities.Contains(abilityData.abilityType);
        }

        public IEnumerator HandCardAbilityAction(BaseCard handCard)
        {
            if (handCard?.AbilitySystem?.HandAbilities == null || handCard.AbilitySystem.HandAbilities.Count == 0)
                yield break;

            Debug.Log($"<color=yellow>[HandAbility]</color> {handCard.LogText} 핸드 어빌리티 적용 시작");

            foreach (CardAbility cardAbility in handCard.AbilitySystem.HandAbilities)
            {
                if (handAbilityActions.TryGetValue(cardAbility.abilityType, out IHandAbility handAbility))
                {
                    yield return StartCoroutine(handAbility.ApplySingle(handCard));
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[HandAbility]</color> {cardAbility.abilityType}에 대한 핸드 어빌리티 동작이 없음");
                }
            }
        }

        public IEnumerator HandCardAbilityRelease(BaseCard handCard)
        {
            if (handCard?.AbilitySystem?.HandAbilities == null || handCard.AbilitySystem.HandAbilities.Count == 0)
                yield break;

            Debug.Log($"<color=yellow>[HandAbility]</color> {handCard.LogText} 핸드 어빌리티 해제 시작");

            var abilitiesToRemove = handCard.AbilitySystem.HandAbilities.ToList();

            foreach (CardAbility cardAbility in abilitiesToRemove)
            {
                if (handAbilityActions.TryGetValue(cardAbility.abilityType, out IHandAbility handAbility))
                {
                    yield return StartCoroutine(handAbility.Release(handCard));

                    handCard.AbilitySystem.RemoveHandCardAbility(cardAbility);

                    if (handAbility is IAbility ability && ability.Changes.Count > 0)
                        Debug.Log($"<color=yellow>[HandAbility]</color> {handCard.LogText} {cardAbility.LogText} 해제 완료. 효과: {Utils.StatChangesText(ability.Changes)}");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[HandAbility]</color> {cardAbility.abilityType}에 대한 핸드 어빌리티 해제가 없음");
                }
            }
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public IEnumerator AbilityAction(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            if (!abilityActions.TryGetValue(cardAbility.abilityType, out IAbility abilityAction))
            {
                Debug.LogWarning($"<color=orange>[AbilityAction]</color> {targetSlot?.LogText ?? "targetSlot 없음."} {cardAbility.LogText}에 대한 동작이 없음.");
                yield break;
            }

            yield return StartCoroutine(abilityAction.ApplySingle(cardAbility, selfSlot, targetSlot));

            if (abilityAction.Changes.Count > 0)
                Debug.Log($"<color=lime>[AbilityAction]</color> {targetSlot?.LogText ?? "targetSlot 없음."} {cardAbility.LogText} 실행 완료. 효과: {Utils.StatChangesText(abilityAction.Changes)}");

            abilityAction.Changes.Clear();
        }

        /// <summary>
        /// 어빌리티 해제
        /// </summary>
        public IEnumerator AbilityRelease(CardAbility cardAbility, AbilityWhereFrom abilityWhereFrom = AbilityWhereFrom.None)
        {
            if (!abilityActions.TryGetValue(cardAbility.abilityType, out IAbility abilityAction))
            {
                Debug.LogWarning($"<color=orange>[AbilityRelease]</color> {cardAbility.LogText}에 대한 해제가 없음. {abilityWhereFrom}");
                yield break;
            }

            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.selfSlotNum);

            if (selfSlot == null)
            {
                Debug.LogError($"<color=red>[AbilityRelease]</color> selfSlotNum({cardAbility.selfSlotNum}) 보드 슬롯 없음. {abilityWhereFrom}");
                yield break;
            }

            BSlot targetSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.targetSlotNum);

            if (targetSlot == null)
            {
                Debug.LogError($"<color=red>[AbilityRelease]</color> targetSlotNum({cardAbility.targetSlotNum}) 보드 슬롯 없음. {abilityWhereFrom}");
                yield break;
            }

            yield return StartCoroutine(abilityAction.Release(cardAbility, selfSlot, targetSlot));

            string abilityActionLog = $"{targetSlot.LogText} {cardAbility.LogText} 지속시간(<color=yellow>{cardAbility.duration}</color>)";

            if (abilityAction.Changes.Count > 0)
                Debug.Log($"<color=orange>[AbilityRelease]</color> {abilityActionLog} 해제. 효과: {Utils.StatChangesText(abilityAction.Changes)}");

            Debug.Log($"<color=orange>[AbilityRelease]</color> {targetSlot.LogText} {cardAbility.LogText} 삭제. 출처: {abilityWhereFrom}");
            abilityAction.Changes.Clear();
        }

        /// <summary>
        /// 어빌리티 타입에 따른 원래 stat 얻기
        /// </summary>
        private int GetOriginStatValue(AbilityType abilityType, BSlot boardSlot)
        {
            if (boardSlot.Card == null)
            {
                Debug.LogWarning($"{boardSlot.LogText} <color=#f4872e>{abilityType} 어빌리티</color> 적용 슬롯 카드 없음");
                return 0;
            }

            // MasterCard 가 CreatureCard 를 상속 받았기 때문에 하위 클래스 먼저 확인
            if (boardSlot.Card is MasterCard masterCard)
            {
                // Debug.Log($"마스터 카드. cardId: {boardSlot.Card.Id} Slot: {boardSlot.SlotNum}, hp: {masterCard.Hp}, mana: {masterCard.Mana}");
                return abilityType switch
                {
                    AbilityType.AddMana or AbilityType.SubMana => masterCard.Mana,
                    AbilityType.DefUp or AbilityType.BrokenDef => masterCard.Def,
                    AbilityType.Heal or AbilityType.Damage or AbilityType.ChargeDamage => masterCard.Hp,
                    _ => 0,
                };
            }

            if (boardSlot.Card is CreatureCard creatureCard)
            {
                return abilityType switch
                {
                    AbilityType.AtkUp => creatureCard.Atk,
                    AbilityType.DefUp or AbilityType.BrokenDef => creatureCard.Def,
                    AbilityType.Heal or AbilityType.Damage or AbilityType.ChargeDamage => creatureCard.Hp,
                    _ => 0,
                };
            }

            Debug.LogWarning($"{boardSlot.LogText} <color=#f4872e>{abilityType} 어빌리티</color> {boardSlot.Card.CardType} 카드에 대한 원래 stat 값 없음");

            return 0;
        }

        /// <summary>
        /// 어빌리티 효과 로깅
        /// </summary>
        private void LogAbilityEffects(AbilityData abilityData, List<BSlot> targetSlots)
        {
            foreach (BSlot targetSlot in targetSlots.Where(slot => slot?.Card != null))
            {
                FlatLogger.LogEffect(targetSlot.ToSlotLogInfo(), abilityData.GetEffectDescription());
            }
        }
    }
}