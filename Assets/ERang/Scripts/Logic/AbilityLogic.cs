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

            // Debug.Log($"AbilityLogic Awake. abilities: {abilities.Length}");

            // 자식 객체의 이름을 출력
            foreach (Transform abilityTransform in abilities)
            {
                // 현재 게임 오브젝트 자신은 제외
                if (abilityTransform == this.transform)
                    continue;

                IAbility ability = abilityTransform.GetComponent<IAbility>();

                if (abilityActions.ContainsKey(ability.AbilityType))
                {
                    Debug.LogError($"어빌리티 {ability.AbilityType} 중복. 어빌리티 스크립트의 AbilityType 확인 필요");
                    continue;
                }

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

        public void AbilityAction(int aiDataId, int abilityId, BSlot selfSlot, List<BSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            AiData aiData = AiData.GetAiData(aiDataId);
            AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

            StartCoroutine(AbilityProcess(aiData, abilityData, selfSlot, targetSlots, whereFrom));
        }

        /// <summary>
        /// 어빌리티 추가하고 발동까지
        /// - 몇몇 어빌리티는 발동 안함
        /// </summary>
        public IEnumerator AbilityProcess(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            Debug.Log($"{selfSlot.LogText} {abilityData.LogText} 실행 - AbilityProcess");

            foreach (BSlot targetSlot in targetSlots)
            {
                BaseCard card = targetSlot.Card;

                if (card == null)
                    continue;

                CardAbility cardAbility = card.CardAbilities.Find(cardAbility => cardAbility.abilityId == abilityData.abilityId);

                // 어빌리티 발동 여부
                bool isAbilityAction = cardAbility == null;

                cardAbility ??= new()
                {
                    abilityId = abilityData.abilityId,
                    aiDataId = aiData.ai_Id,
                    selfSlotNum = selfSlot.SlotNum,
                    targetSlotNum = targetSlot.SlotNum,

                    aiType = aiData.type,
                    abilityType = abilityData.abilityType,
                    abilityValue = abilityData.value,
                    workType = abilityData.workType,
                    duration = abilityData.duration,
                };

                // 핸드 온 어빌리티는 턴 종료시 해제되기 때문에 저장. 효과 지속 시간이 있는 어빌리티 저장.
                if (abilityData.workType == AbilityWorkType.OnHand || abilityData.duration > 0)
                {
                    card.AddCardAbility(cardAbility, BattleLogic.Instance.turnCount, whereFrom);
                }

                targetSlot.DrawAbilityIcons();

                // 신규 어빌리티인 경우 어빌리티 발동
                if (isAbilityAction)
                {
                    // 행동 전, 후 어빌리티는 여기서 발동 안함
                    // 행동 전 어빌리티는 PriorAbilityAction, 행동 후 어빌리티는 PostAbilityAction 에서 발동
                    if (Constants.CardPriorAbilities.Contains(abilityData.abilityType) || Constants.CardPostAbilities.Contains(abilityData.abilityType))
                        yield break;

                    yield return StartCoroutine(AbilityAction(cardAbility, selfSlot, targetSlot));
                }
            }
        }

        public IEnumerator AbilityAction(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            IAbility abilityAction = abilityActions.TryGetValue(cardAbility.abilityType, out IAbility action) ? action : null;

            if (abilityAction == null)
            {
                Debug.LogWarning($"{targetSlot?.LogText ?? "targetSlot 없음"} {cardAbility.LogText} 에 대한 동작이 없음");
                yield break;
            }

            yield return StartCoroutine(abilityAction.ApplySingle(cardAbility, selfSlot, targetSlot));

            Debug.Log($"{targetSlot?.LogText ?? "targetSlot 없음"} {cardAbility.LogText} 실행. 어빌리티 효과: {Utils.StatChangesText(abilityAction.Changes)}");

            if (abilityAction.Changes.Count == 0)
                yield break;

            abilityAction.Changes.Clear();
        }

        /// <summary>
        /// 카드 어빌리티 해제 동작
        /// </summary>
        public IEnumerator ReleaseCardAbilityAction(CardAbility cardAbility)
        {
            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.selfSlotNum);

            if (selfSlot == null)
            {
                Debug.LogError($"selfSlotNum({cardAbility.selfSlotNum}) 보드 슬롯 없음 - ReleaseCardAbilityAction");
                yield return null;
            }

            AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);

            if (abilityData == null)
            {
                Debug.LogError($"AbilityData({cardAbility.abilityId}) {Utils.RedText("테이블 데이터 없음")} - ReleaseCardAbilityAction");
                yield return null;
            }

            Debug.Log($"{selfSlot.LogText} {cardAbility.LogText} Duration: {cardAbility.duration} 으로 해제 - ReleaseCardAbilityAction");

            BSlot selfBoardSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.selfSlotNum);
            BSlot targetBoardSlot = BoardSystem.Instance.GetBoardSlot(cardAbility.targetSlotNum);

            yield return StartCoroutine(AbilityRelease(cardAbility, selfBoardSlot, targetBoardSlot));
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
                Debug.LogWarning($"{selfSlot.LogText} {abilityData.LogText} 에 대한 해제 없음 - AbilityRelease");
                yield break;
            }

            string abilityActionLog = $"{targetSlot.LogText} {abilityData.LogText} 효과 지속 시간(<color=yellow>{cardAbility.duration}</color>)";

            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{abilityActionLog} 카드 없음. AbilityRelease");

                abilityAction.Changes.Clear();
                yield break;
            }

            yield return StartCoroutine(abilityAction.Release(cardAbility, selfSlot, targetSlot));

            if (abilityAction.Changes.Count > 0)
            {
                Debug.Log($"{abilityActionLog} 해제. {Utils.StatChangesText(abilityAction.Changes)}");
                abilityAction.Changes.Clear();
            }

            Debug.Log($"삭제 어빌리티 동작. {targetSlot.LogText} {abilityData.LogText} - AbilityRelease");
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