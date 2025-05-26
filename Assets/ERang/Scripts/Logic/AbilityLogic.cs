using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

        public Dictionary<AbilityType, IAbility> abilityActions = new();

        void Awake()
        {
            Instance = this;

            // 현재 게임 오브젝트와 모든 자식 게임 오브젝트의 Transform 컴포넌트를 얻음
            Transform[] abilities = GetComponentsInChildren<Transform>();

            GameLogger.Log(LogCategory.DATA, $"어빌리티 시스템 초기화: {abilities.Length}개 컴포넌트 검사");

            // 자식 객체의 이름을 출력
            foreach (Transform abilityTransform in abilities)
            {
                // 현재 게임 오브젝트 자신은 제외
                if (abilityTransform == this.transform)
                    continue;

                IAbility ability = abilityTransform.GetComponent<IAbility>();

                if (abilityActions.ContainsKey(ability.AbilityType))
                {
                    GameLogger.Log(LogCategory.ERROR, $"❌ 어빌리티 {ability.AbilityType} 중복. 어빌리티 스크립트의 AbilityType 확인 필요");
                    continue;
                }

                abilityActions.Add(ability.AbilityType, ability);
                GameLogger.Log(LogCategory.DATA, $"어빌리티 등록: {ability.AbilityType}");
            }

            GameLogger.Log(LogCategory.DATA, $"어빌리티 시스템 초기화 완료: {abilityActions.Count}개 어빌리티 등록");
        }

        /// <summary>
        /// BoardCardEditorWindow 에서 호출되는 어빌리티 액션
        /// </summary>
        public void AbilityAction(int abilityId, BSlot targetSlot)
        {
            AbilityData abilityData = Utils.CheckData(AbilityData.GetAbilityData, "AbilityData", abilityId);

            if (abilityData == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ AbilityData({abilityId}) 테이블 데이터 없음");
                return;
            }

            StartCoroutine(AbilityProcess(null, abilityData, targetSlot, new List<BSlot> { targetSlot }, AbilityWhereFrom.EditorWindow));
        }

        /// <summary>
        /// BoardCardEditorWindow 에서 호출되는 어빌리티 해제
        /// </summary>
        public void ReleaseAction(BSlot targetSlot, CardAbility cardAbility)
        {
            StartCoroutine(AbilityRelease(cardAbility, AbilityWhereFrom.EditorWindow));

            GameCard card = targetSlot.Card;

            card.RemoveCardAbility(cardAbility);
            targetSlot.DrawAbilityIcons();
        }

        /// <summary>
        /// 어빌리티 추가하고 발동까지
        /// - 몇몇 어빌리티는 발동 안함
        /// </summary>
        public IEnumerator AbilityProcess(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            string sourceInfo = selfSlot.Card?.Name ?? $"슬롯{selfSlot.SlotNum}";
            string targetInfo = string.Join(", ", targetSlots.Select(s => s.Card?.Name ?? $"빈슬롯{s.SlotNum}"));

            // 어빌리티 시작 로그
            GameLogger.LogAbility(abilityData.nameDesc, sourceInfo, targetInfo, "시작");
            GameLogger.LogAbilityDetail($"어빌리티 ID: {abilityData.abilityId}, 타입: {abilityData.abilityType}, 값: {abilityData.value}");
            GameLogger.LogAbilityDetail($"발동 출처: {whereFrom}, 지속시간: {abilityData.duration}턴");

            foreach (BSlot targetSlot in targetSlots)
            {
                GameCard card = targetSlot.Card;

                if (card == null)
                {
                    GameLogger.LogAbilityDetail($"타겟 슬롯{targetSlot.SlotNum}에 카드 없음 - 스킵");
                    continue;
                }

                CardAbility cardAbility = card.AbilitySystem.CardAbilities.Find(cardAbility => cardAbility.abilityId == abilityData.abilityId);

                // 어빌리티 발동 여부
                bool isAbilityAction = cardAbility == null;

                cardAbility ??= new()
                {
                    abilityId = abilityData.abilityId,
                    nameDesc = abilityData.nameDesc,
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
                    card.AddCardAbility(cardAbility, BattleLogic.Instance.turnCount, whereFrom);

                    if (abilityData.workType == AbilityWorkType.OnHand)
                    {
                        GameLogger.LogAbilityDetail($"{card.Name}에 핸드 온 어빌리티 추가: {abilityData.nameDesc}");
                    }
                    else
                    {
                        GameLogger.LogAbilityDetail($"{card.Name}에 지속 어빌리티 추가: {abilityData.nameDesc} ({abilityData.duration}턴)");
                    }
                }

                targetSlot.DrawAbilityIcons();

                // 신규 어빌리티인 경우 어빌리티 발동
                if (isAbilityAction)
                {
                    // 행동 전, 후 어빌리티는 여기서 발동 안함
                    // 행동 전 어빌리티는 PriorAbilityAction, 행동 후 어빌리티는 PostAbilityAction 에서 발동
                    if (Constants.CardPriorAbilities.Contains(abilityData.abilityType) || Constants.CardPostAbilities.Contains(abilityData.abilityType))
                    {
                        GameLogger.LogAbilityDetail($"{abilityData.nameDesc}는 행동 전/후 어빌리티라서 여기서 발동 안함");
                        yield break;
                    }

                    yield return StartCoroutine(AbilityAction(cardAbility, selfSlot, targetSlot));
                }
                else
                {
                    GameLogger.LogAbilityDetail($"{card.Name}에 이미 {abilityData.nameDesc} 어빌리티 존재 - 발동 스킵");
                }
            }

            // 어빌리티 완료 로그
            GameLogger.LogAbility(abilityData.nameDesc, sourceInfo, targetInfo, "완료");

        }

        public IEnumerator HandCardAbilityAction(GameCard handCard)
        {
            GameLogger.LogAbility("핸드 카드 어빌리티", handCard.Name, "", "적용 시작");

            foreach (CardAbility cardAbility in handCard.AbilitySystem.HandAbilities)
            {
                IAbility abilityAction = abilityActions.TryGetValue(cardAbility.abilityType, out IAbility action) ? action : null;

                if (abilityAction == null)
                {
                    GameLogger.Log(LogCategory.ERROR, $"❌ {cardAbility.LogText}에 대한 동작이 없음");
                    yield break;
                }

                if (abilityAction is AbilityReducedMana reducedMana)
                {
                    GameLogger.LogAbility("마나 감소", handCard.Name, "", "적용");
                    yield return StartCoroutine(reducedMana.ApplySingle(handCard));
                }
            }
        }

        public IEnumerator HandCardAbilityRelease(GameCard handCard)
        {
            GameLogger.LogAbility("핸드 카드 어빌리티", handCard.Name, "", "해제 시작");

            for (int i = 0; i < handCard.AbilitySystem.HandAbilities.Count; ++i)
            {
                CardAbility cardAbility = handCard.AbilitySystem.HandAbilities[i];

                IAbility abilityAction = abilityActions.TryGetValue(cardAbility.abilityType, out IAbility action) ? action : null;

                if (abilityAction == null)
                {
                    GameLogger.Log(LogCategory.ERROR, $"❌ {cardAbility.LogText}에 대한 해제 없음");
                    yield break;
                }

                if (abilityAction is AbilityReducedMana reducedMana)
                {
                    GameLogger.LogAbility("마나 감소", handCard.Name, "", "해제");
                    yield return StartCoroutine(reducedMana.Release(handCard));

                    handCard.RemoveHandCardAbility(cardAbility);

                    GameLogger.LogAbility("마나 감소", handCard.Name, "", $"해제 완료 - {Utils.StatChangesText(reducedMana.Changes)}");
                }
            }

            GameLogger.LogAbility("핸드 카드 어빌리티", handCard.Name, "", "해제 완료");
        }

        public IEnumerator AbilityAction(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            string sourceInfo = selfSlot?.Card?.Name ?? $"슬롯{selfSlot?.SlotNum ?? -1}";
            string targetInfo = targetSlot?.Card?.Name ?? $"슬롯{targetSlot?.SlotNum ?? -1}";

            IAbility abilityAction = abilityActions.TryGetValue(cardAbility.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {targetInfo} {cardAbility.LogText}에 대한 동작이 없음");
                yield break;
            }

            GameLogger.LogAbility(cardAbility.nameDesc, sourceInfo, targetInfo, "실행 시작");

            yield return StartCoroutine(abilityAction.ApplySingle(cardAbility, selfSlot, targetSlot));

            // 어빌리티 효과 결과 로그
            if (abilityAction.Changes.Count > 0)
            {
                string effectText = Utils.StatChangesText(abilityAction.Changes);
                GameLogger.LogAbility(cardAbility.nameDesc, sourceInfo, targetInfo, $"완료 - {effectText}");

                // 상태 변화 상세 로그
                foreach (var change in abilityAction.Changes)
                {
                    string statName = change.Item1.ToString();
                    int beforeValue = change.Item6;
                    int afterValue = change.Item7;
                    int changeValue = change.Item8;

                    if (beforeValue != afterValue)
                    {
                        GameLogger.LogCardState(targetInfo, statName, beforeValue, afterValue, cardAbility.nameDesc);
                    }
                }
            }
            else
            {
                GameLogger.LogAbility(cardAbility.nameDesc, sourceInfo, targetInfo, "완료 - 상태 변화 없음");
            }

            if (abilityAction.Changes.Count == 0)
                yield break;

            abilityAction.Changes.Clear();
        }

        /// <summary>
        /// 카드 ability 해제 동작
        /// </summary>
        public IEnumerator AbilityRelease(CardAbility cardAbility, AbilityWhereFrom abilityWhereFrom = AbilityWhereFrom.None)
        {
            IAbility abilityAction = abilityActions.TryGetValue(cardAbility.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {cardAbility.LogText}에 대한 해제 없음. {abilityWhereFrom}");
                yield break;
            }

            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.selfSlotNum);

            if (selfSlot == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ selfSlotNum({cardAbility.selfSlotNum}) 보드 슬롯 없음. {abilityWhereFrom}");
                yield return null;
            }

            BSlot targetSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.targetSlotNum);

            if (targetSlot == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ targetSlotNum({cardAbility.targetSlotNum}) 보드 슬롯 없음. {abilityWhereFrom}");
                yield return null;
            }

            string sourceInfo = selfSlot?.Card?.Name ?? $"슬롯{cardAbility.selfSlotNum}";
            string targetInfo = targetSlot?.Card?.Name ?? $"슬롯{cardAbility.targetSlotNum}";

            GameLogger.LogAbility(cardAbility.nameDesc, sourceInfo, targetInfo, $"해제 시작 (지속시간: {cardAbility.duration})");

            yield return StartCoroutine(abilityAction.Release(cardAbility, selfSlot, targetSlot));

            if (abilityAction.Changes.Count > 0)
            {
                string effectText = Utils.StatChangesText(abilityAction.Changes);
                GameLogger.LogAbility(cardAbility.nameDesc, sourceInfo, targetInfo, $"해제 완료 - {effectText}");

                // 해제 시 상태 변화 로그
                foreach (var change in abilityAction.Changes)
                {
                    string statName = change.Item1.ToString();
                    int beforeValue = change.Item6;
                    int afterValue = change.Item7;

                    if (beforeValue != afterValue)
                    {
                        GameLogger.LogCardState(targetInfo, statName, beforeValue, afterValue, $"{cardAbility.nameDesc} 해제");
                    }
                }

                abilityAction.Changes.Clear();
            }

            GameLogger.LogAbility(cardAbility.nameDesc, sourceInfo, targetInfo, $"어빌리티 삭제 완료. {abilityWhereFrom}");
        }

        /// <summary>
        /// 어빌리티 타입에 따른 원래 stat 얻기
        /// </summary>
        private int GetOriginStatValue(AbilityType abilityType, BSlot boardSlot)
        {
            if (boardSlot.Card == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {boardSlot.LogText} {abilityType} 어빌리티 적용 슬롯 카드 없음");
                return 0;
            }

            // MasterCard 가 CreatureCard 를 상속 받았기 때문에 하위 클래스 먼저 확인
            if (boardSlot.Card.CardType == CardType.Master)
            {
                // Debug.Log($"마스터 카드. cardId: {boardSlot.Card.Id} Slot: {boardSlot.SlotNum}, hp: {masterCard.Hp}, mana: {masterCard.Mana}");
                int value = abilityType switch
                {
                    AbilityType.AddMana or AbilityType.SubMana => boardSlot.Card.State.Mana,
                    AbilityType.DefUp or AbilityType.BrokenDef => boardSlot.Card.State.Def,
                    AbilityType.Heal or AbilityType.Damage or AbilityType.ChargeDamage => boardSlot.Card.State.Hp,
                    _ => 0,
                };

                GameLogger.LogAbilityDetail($"마스터 카드 {abilityType} 기준값: {value}");
                return value;
            }

            if (boardSlot.Card.CardType == CardType.Creature)
            {
                int value = abilityType switch
                {
                    AbilityType.AtkUp => boardSlot.Card.State.Atk,
                    AbilityType.DefUp or AbilityType.BrokenDef => boardSlot.Card.State.Def,
                    AbilityType.Heal or AbilityType.Damage or AbilityType.ChargeDamage => boardSlot.Card.State.Hp,
                    _ => 0,
                };

                GameLogger.LogAbilityDetail($"크리쳐 카드 {abilityType} 기준값: {value}");
                return value;
            }

            GameLogger.Log(LogCategory.ERROR, $"❌ {boardSlot.LogText} {abilityType} 어빌리티 - {boardSlot.Card.CardType} 카드에 대한 원래 stat 값 없음");

            return 0;
        }
    }
}