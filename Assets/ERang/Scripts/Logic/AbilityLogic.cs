using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class CardAbility
    {
        public AbilityWhereFrom whereFrom; // 어빌리티 적용 위치

        public string abilityUid; // 어빌리티 고유 번호. abilityId + startTurn + abilityCount
        public int abilityId; // 어빌리티 Id
        public AbilityWorkType workType; // 어빌리티 작업 타입(HandOn 찾기 위함)

        public int aiDataId;
        public AiDataType aiType; // Ai 타입. Buff, Debuff 구분

        public int duration;
        public int totalDuration;
        public int startTurn;

        public int selfSlotNum;
        public int targetSlotNum;
        
        public List<BSlot> targetSlots = new();

        public string LogText => Utils.AbilityLog(abilityId, abilityUid);
    }

    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

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

        public IEnumerator AbilityAction(List<int> abilityIds, BSlot selfSlot, List<BSlot> targetSlots)
        {
            List<AbilityData> abilityDatas = abilityIds.Select(id => AbilityData.GetAbilityData(id)).ToList();

            foreach (AbilityData abilityData in abilityDatas)
            {
                string abilityLog = $"{selfSlot.LogText} {abilityData.LogText} 타겟 (슬롯, 카드): {string.Join(", ", targetSlots.Select(slot => (slot.SlotNum, slot.Card?.Id)))}";
            }

            yield return null;
        }

        public void AbilityAction(int aiDataId, int abilityId, BSlot selfSlot, List<BSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            AiData aiData = AiData.GetAiData(aiDataId);
            AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

            StartCoroutine(AbilityAction(aiData, abilityData, selfSlot, targetSlots, whereFrom));
        }

        public IEnumerator AbilityAction(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            string abilityLog = $"{selfSlot.LogText} {abilityData.LogText} 타겟 (슬롯, 카드): {string.Join(", ", targetSlots.Select(slot => (slot.SlotNum, slot.Card?.Id)))}";

            foreach (BSlot targetSlot in targetSlots)
            {
                // 핸드 온 어빌리티는 턴 종료시 해제되기 때문에 모두 저장
                if (abilityData.workType == AbilityWorkType.OnHand)
                {
                    AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
                    continue;
                }

                // 효과 지속 시간이 있는 어빌리티만 추가
                if (abilityData.duration <= 0)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }

            if (targetSlots.Count > 0)
                yield return StartCoroutine(AbilityAction(aiData, abilityData, selfSlot, targetSlots));
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public IEnumerator AbilityAction(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            IAbility abilityAction = abilityActions.TryGetValue(abilityData.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                Debug.LogWarning($"{selfSlot.LogText} {abilityData.LogText} 에 대한 동작이 없음");
                yield break;
            }

            yield return StartCoroutine(abilityAction.Apply(aiData, abilityData, selfSlot, targetSlots));

            Debug.Log($"{selfSlot.LogText} {abilityData.LogText} 실행. {Utils.TargetText(aiData.target)} {Utils.StatChangesText(abilityAction.Changes)}");

            if (abilityAction.Changes.Count == 0)
                yield break;

            abilityAction.Changes.Clear();
        }

        /// <summary>
        /// 보드 슬롯에 장착된 카드 ability 해제
        /// </summary>
        public IEnumerator AbilityRelease(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);

            IAbility abilityAction = abilityActions.TryGetValue(abilityData.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                Debug.LogWarning($"{selfSlot.LogText} {abilityData.LogText} 에 대한 해제 없음");
                yield break;
            }

            string abilityActionLog = $"{targetSlot.LogText} {abilityData.LogText} 효과 지속 시간(<color=yellow>{cardAbility.duration}</color>)";

            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{abilityActionLog} - 카드 없음. 어빌리티 삭제");

                abilityAction.Changes.Clear();
                yield break;
            }

            yield return StartCoroutine(abilityAction.Release(cardAbility, selfSlot, targetSlot));

            if (abilityAction.Changes.Count > 0)
            {
                Debug.Log($"{abilityActionLog} 해제. {Utils.StatChangesText(abilityAction.Changes)}");
                abilityAction.Changes.Clear();
            }

            Debug.Log($"어빌리티 삭제: {targetSlot.LogText} {abilityData.LogText}");
        }

        /// <summary>
        /// 어빌리티 추가
        /// </summary>
        private void AddAbility(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot, AbilityWhereFrom whereFrom)
        {
            if (targetSlot.Card == null)
            {
                // Debug.LogWarning($"{targetSlot.LogText} {abilityData.LogText} 적용 슬롯 카드 없음");
                return;
            }

            int beforeValue = GetOriginStatValue(abilityData.abilityType, targetSlot);

            BaseCard card = targetSlot.Card;

            CardAbility cardAbility = new()
            {
                whereFrom = whereFrom,
                abilityUid = $"{abilityData.abilityId}_{BattleLogic.Instance.turnCount}_{card.Abilities.Count}",
                aiType = aiData.type,
                abilityId = abilityData.abilityId,
                workType = abilityData.workType,
                duration = abilityData.duration,
                totalDuration = abilityData.duration,
                startTurn = BattleLogic.Instance.turnCount,
                aiDataId = aiData.ai_Id,
                selfSlotNum = selfSlot.SlotNum,
                targetSlotNum = targetSlot.SlotNum,
            };

            Debug.Log($"{targetSlot.LogText} {cardAbility.LogText} 추가. beforeValue: {beforeValue}, value: {abilityData.value}, duration: {abilityData.duration}, workType: {abilityData.workType}");

            card.Abilities.Add(cardAbility);
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
    }
}