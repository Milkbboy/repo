using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Ability
    {
        public AbilityWhereFrom whereFrom; // 어빌리티 적용 위치
        public int atkCount; // 공격 횟수
        public AiDataType aiType; // Ai 타입
        public AiDataAttackType aiAttackType; // Ai 공격 타입
        public int abilityId; // 어빌리티 Id
        public AbilityType abilityType; // 어빌리티 타입
        public AbilityWorkType abilityWorkType; // 어빌리티 작업 타입(HandOn 찾기 위함)
        public int beforeValue; // 어빌리티 적용 전 값
        public int abilityValue; // 어빌리티 값
        public int duration; // 현재 지속 시간
        public int selfBoardSlot; // 어빌리티 발동 보드 슬롯
        public string selfCardUid; // 어빌리티 발동 카드 Uid
        public int targetBoardSlot; // 어빌리티 대상 보드 슬롯
        public int targetCardId; // 어빌리티 대상 카드 Id
        public string targetCardUid; // 어빌리티 대상 카드 Uid
    }

    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

        public List<Ability> abilities = new();
        public Dictionary<AbilityType, IAbility> abilityActions = new();

        void Awake()
        {
            Instance = this;

            // 현재 게임 오브젝트와 모든 자식 게임 오브젝트의 Transform 컴포넌트를 얻음
            Transform[] abilities = GetComponentsInChildren<Transform>();

            // 자식 객체의 이름을 출력
            foreach (Transform abilityTransform in abilities)
            {
                // 현재 게임 오브젝트 자신은 제외
                if (abilityTransform == this.transform)
                    continue;

                IAbility ability = abilityTransform.GetComponent<IAbility>();
                abilityActions.Add(ability.AbilityType, ability);
            }

            // abilityActions 딕셔너리의 값들이 null 확인
            // foreach (var kvp in abilityActions)
            // {
            //     if (kvp.Value != null)
            //         Debug.Log($"AbilityAction[{kvp.Key}] found: {kvp.Value.AbilityType}");
            //     else
            //         Debug.LogError($"AbilityAction[{kvp.Key}] is null.");
            // }
        }

        void Start()
        {

        }

        public List<Ability> GetAbilities()
        {
            return abilities;
        }

        public List<Ability> GetHandOnAbilities()
        {
            return abilities.FindAll(ability => ability.whereFrom == AbilityWhereFrom.TurnStarHandOn);
        }

        public List<Ability> GetBoardSlotCardAbilities()
        {
            return abilities.FindAll(ability => ability.whereFrom != AbilityWhereFrom.TurnStarHandOn);
        }

        /// <summary>
        /// 카드 버프 개수 얻기
        /// </summary>
        public int GetBuffCount(string cardUid)
        {
            return abilities.Count(ability => ability.targetCardUid == cardUid && ability.aiType == AiDataType.Buff);
        }

        /// <summary>
        /// 카드 디버프 개수 얻기
        /// </summary>
        public int GetDebuffCount(string cardUid)
        {
            return abilities.Count(ability => ability.targetCardUid == cardUid && ability.aiType == AiDataType.Debuff);
        }

        /// <summary>
        /// 카드 어빌리티 제거 - card uid
        /// </summary>
        public void RemoveAbility(string cardUid)
        {
            abilities.RemoveAll(ability => ability.targetCardUid == cardUid);
        }

        /// <summary>
        /// 핸드 온 어빌리티 추가
        /// </summary>
        public void AddHandOnAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            // 핸드 온 어빌리티는 턴 종료시 해제되기 때문에 모두 저장
            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }
        }

        /// <summary>
        /// 어빌리티 추가
        /// </summary>
        public void AddAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            foreach (BoardSlot targetSlot in targetSlots)
            {
                Ability durationAbility = abilities.Find(a => a.abilityId == abilityData.abilityId && a.targetBoardSlot == targetSlot.Slot);

                // 이미 어빌리티가 적용 중이면 패스
                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 이미 {Utils.AbilityLog(abilityData)}가 적용 중으로 해당 어빌리티 패스");
                    continue;
                }

                // 효과 지속 시간이 있는 어빌리티만 추가
                if (abilityData.duration <= 0)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }
        }

        public IEnumerator AbilityAction(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            string abilityLog = $"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)} 타겟 (슬롯, 카드): {string.Join(", ", targetSlots.Select(slot => (slot.Slot, slot.Card?.Id)))}";

            List<BoardSlot> passedSlots = new();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                Ability durationAbility = abilities.Find(a => a.abilityId == abilityData.abilityId && a.targetBoardSlot == targetSlot.Slot);

                // 이미 어빌리티가 적용 중이면 패스
                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(targetSlot)} 이미 {Utils.AbilityLog(abilityData)} 가 적용 중 패스");
                    passedSlots.Add(targetSlot);
                    continue;
                }

                // 효과 지속 시간이 있는 어빌리티만 추가
                if (abilityData.duration <= 0)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }

            // passedSlots에 추가된 슬롯은 어빌리티 적용 대상에서 제외
            if (passedSlots.Count > 0)
            {
                // Debug.Log($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)} 적용 제외 슬롯: {string.Join(", ", passedSlots.Select(slot => slot.Slot))}");
                targetSlots = targetSlots.Except(passedSlots).ToList();
            }

            // Debug.Log($"{abilityLog} 적용 시작");

            if (targetSlots.Count > 0)
                yield return StartCoroutine(AbilityAction(aiData, abilityData, selfSlot, targetSlots));
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public IEnumerator AbilityAction(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            IAbility abilityAction = abilityActions.TryGetValue(abilityData.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)} 에 대한 동작이 없음");
                yield break;
            }

            yield return StartCoroutine(abilityAction.Apply(aiData, abilityData, selfSlot, targetSlots));

            if (abilityAction.Changes.Count == 0)
                yield break;

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)}로 {Utils.TargetText(aiData.target)} {Utils.StatChangesText(abilityData.abilityType, abilityAction.Changes)}");
            abilityAction.Changes.Clear();
        }

        /// <summary>
        /// 보드 슬롯에 장착된 카드 ability 해제
        /// </summary>
        public IEnumerator AbilityRelease(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            IAbility abilityAction = abilityActions.TryGetValue(ability.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(ability.abilityType, ability.abilityId)} 에 대한 해제 없음");
                yield break;
            }

            string abilityActionLog = $"{Utils.BoardSlotLog(targetSlot)} {Utils.AbilityLog(ability.abilityType, ability.abilityId)} 효과 지속 시간(<color=yellow>{ability.duration}</color>)";

            Card card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{abilityActionLog} - 카드 없음. 어빌리티 삭제");

                abilityAction.Changes.Clear();
                abilities.Remove(ability);
                yield break;
            }

            yield return StartCoroutine(abilityAction.Release(ability, selfSlot, targetSlot));

            if (abilityAction.Changes.Count > 0)
            {
                Debug.Log($"{abilityActionLog} 해제. {Utils.StatChangesText(ability.abilityType, abilityAction.Changes)}");
                abilityAction.Changes.Clear();
            }

            abilities.Remove(ability);
        }

        /// <summary>
        /// 어빌리티 생성
        /// </summary>
        public Ability MakeAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, BoardSlot targetSlot, AbilityWhereFrom whereFrom)
        {
            int beforeValue = GetOriginStatValue(abilityData.abilityType, targetSlot);
            // Debug.Log($"{Utils.AbilityLog(abilityData)} 생성. 슬롯 {selfSlot.Slot} => {targetSlot.Slot}, beforeValue: {beforeValue}, value: {abilityData.value}, duration: {abilityData.duration}");

            Ability ability = new()
            {
                whereFrom = whereFrom,
                aiType = aiData.type,
                aiAttackType = aiData.attackType,
                atkCount = aiData.atk_Cnt,
                abilityId = abilityData.abilityId,
                abilityType = abilityData.abilityType,
                abilityWorkType = abilityData.workType,
                abilityValue = abilityData.value,
                duration = abilityData.duration,
                beforeValue = beforeValue,
                selfBoardSlot = selfSlot.Slot,
                selfCardUid = selfSlot.Card?.Uid ?? string.Empty,
                targetBoardSlot = targetSlot.Slot,
                targetCardId = targetSlot.Card?.Id ?? 0,
                targetCardUid = targetSlot.Card?.Uid ?? string.Empty
            };

            return ability;
        }

        /// <summary>
        /// 어빌리티 추가
        /// </summary>
        private void AddAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, BoardSlot targetSlot, AbilityWhereFrom whereFrom)
        {
            Ability ability = MakeAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);

            abilities.Add(ability);
        }

        /// <summary>
        /// 어빌리티 타입에 따른 원래 stat 얻기
        /// </summary>
        private int GetOriginStatValue(AbilityType abilityType, BoardSlot boardSlot)
        {
            if (boardSlot.Card == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} <color=#f4872e>{abilityType} 어빌리티</color> 적용 슬롯 카드 없음");
                return 0;
            }

            return abilityType switch
            {
                AbilityType.AtkUp => boardSlot.Card.atk,
                AbilityType.DefUp or AbilityType.BrokenDef => boardSlot.Card.def,
                AbilityType.Heal => boardSlot.Card.hp,
                _ => 0,
            };
        }
    }
}