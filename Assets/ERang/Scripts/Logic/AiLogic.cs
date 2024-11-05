using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using Newtonsoft.Json;

namespace ERang
{
    public class AiLogic : MonoBehaviour
    {
        public static AiLogic Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 카드의 Ai 그룹을 호출하여 AiData를 가져온다.
        /// </summary>
        /// <returns></returns>
        public int GetCardAiDataId(BaseCard card)
        {
            BSlot boardSlot = BoardSystem.Instance.GetBoardSlot(card.Uid);

            string aiGroupDataTableLog = $"{(boardSlot != null ? Utils.BoardSlotLog(boardSlot) : $"카드({card.Id})")}. <color=#78d641>AiGroupData</color> 테이블 {card.AiGroupId} 데이터 얻기";
            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(card.AiGroupId);

            if (aiGroupData == null)
            {
                Debug.LogError($"{aiGroupDataTableLog} 실패. <color=#ea4123>테이블 데이터 없음</color> - Card.GetCardAiDataId");
                return 0;
            }

            List<int> aiDataIds = new List<int>();

            // 순차적으로 AI 그룹을 호출하고, 마지막 그룹에 도달 시 최초 그룹으로 순환한다.
            if (aiGroupData.aiGroupType == AiGroupType.Repeat)
            {
                if (card.AiGroupIndex >= aiGroupData.ai_Groups.Count)
                    card.AiGroupIndex = 0;

                aiDataIds = aiGroupData.ai_Groups[card.AiGroupIndex];

                card.AiGroupIndex++;
            }
            else if (aiGroupData.aiGroupType == AiGroupType.Random)
            {
                // 나열된 AI 그룹 중 하나를 임의로 선택한다. (선택 확율은 별도 지정 없이 동일한다. n/1)
                aiDataIds = aiGroupData.ai_Groups[Random.Range(0, aiGroupData.ai_Groups.Count)];
            }

            // Debug.Log($"{aiGroupDataTableLog} 성공. aiGroupType {aiGroupData.aiGroupType.ToString()} 으로 aiDataIds [{string.Join(", ", aiDataIds)}] 중 하나 뽑기 - Card.GetCardAiDataId");

            int selectedAiDataId = 0;

            if (aiDataIds.Count == 1)
            {
                selectedAiDataId = aiDataIds[0];
                // Debug.Log($"{id} 카드. aiDataIds [{string.Join(", ", aiDataIds)}] 에서 {selectedAiDataId} 뽑힘 (하나만 설정되어 있음)");
            }
            else
            {
                // aiData 가중치 리스트 설정
                int totalValue = 0;
                List<(int aiDataId, int value)> aiDataList = new List<(int aiDataId, int value)>();

                foreach (int aiDataId in aiDataIds)
                {
                    string aiDataTableLog = $"<color=#78d641>AiGroupData</color> 테이블 {aiDataId} 데이터 얻기";
                    AiData aiData = AiData.GetAiData(aiDataId);

                    if (aiData == null)
                    {
                        Debug.LogError($"{aiDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - Card.GetCardAiDataId");
                        continue;
                    }

                    // Debug.Log($"{aiDataTableLog} 성공. 가중치{(aiDataId, aiData.value)} 추가 - Card.GetCardAiDataId");
                    totalValue += aiData.value;

                    aiDataList.Add((aiDataId, aiData.value));
                };

                string aiDataListLog = $"{card.Id} 카드. aiDataIds {string.Join(", ", aiDataIds)} 중 중 하나 선택";

                // 가중치 리스트 중 하나 뽑기
                int randomValue = Random.Range(0, totalValue);
                int cumulativeValue = 0;

                foreach (var aiData in aiDataList)
                {
                    cumulativeValue += aiData.value;

                    if (randomValue < cumulativeValue)
                    {
                        selectedAiDataId = aiData.aiDataId;
                        // Debug.Log($"{aiDataListLog} - 총 가중치 {totalValue}, randomValue({randomValue}) < cumulativeValue({cumulativeValue}) 로 {selectedAiDataId} 선택({string.Join(", ", aiDataList.Select(x => x.value))})");
                        break;
                    }
                }
            }

            return selectedAiDataId;
        }

        /// <summary>
        /// 타겟 선택 카드 타입 확인
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public (bool IsSelectAttackType, AiData) GetAiAttackInfo(BaseCard card)
        {
            int aiDataId = GetCardAiDataId(card);

            if (aiDataId == 0)
                return (false, null);

            AiData aiData = AiData.GetAiData(aiDataId);

            return (Constants.SelectAttackTypes.Contains(aiData.attackType), aiData);
        }

        /// <summary>
        /// HandOn 어빌리티를 가진 카드 얻기
        /// </summary>
        public List<(BaseCard card, AiData aiData, List<AbilityData> abilities)> GetHandOnCards(List<BaseCard> handCards)
        {
            List<(BaseCard card, AiData aiData, List<AbilityData> abilities)> handOnCards = new List<(BaseCard, AiData, List<AbilityData>)>();

            foreach (BaseCard handCard in handCards)
            {
                int aiDataId = GetCardAiDataId(handCard);

                AiData handCardAiData = AiData.GetAiData(aiDataId);

                if (handCardAiData == null)
                {
                    Debug.LogWarning($"{Utils.CardLog(handCard)} AiData({aiDataId}) <color=red>테이블에 데이터 없음</color> - AiLogic.GetHandOnCards");
                    continue;
                }

                List<AbilityData> abilities = new List<AbilityData>();

                foreach (int abilityId in handCardAiData.ability_Ids)
                {
                    AbilityData ability = AbilityData.GetAbilityData(abilityId);

                    if (ability == null)
                    {
                        Debug.LogWarning($"{Utils.CardLog(handCard)} AbilityData({abilityId}) <color=red>테이블 데이터 없음</color> - AiLogic.GetHandOnCards");
                        continue;
                    }

                    if (ability.workType == AbilityWorkType.OnHand)
                        abilities.Add(ability);
                }

                if (abilities.Count == 0)
                    continue;

                handOnCards.Add((handCard, handCardAiData, abilities));
            }

            if (handOnCards.Count > 0)
                Debug.Log($"HandOn 어빌리티를 가진 카드 {handOnCards.Count}장({string.Join(", ", handOnCards.Select(handOnCard => handOnCard.card.Id))}) 확인 - AiLogic.GetHandOnCards");

            return handOnCards;
        }

        /// <summary>
        /// 턴 시작 리액션 AiData 얻기
        /// </summary>
        /// <param name="reactionSlot"></param>
        /// <param name="opponentSlots"></param>
        /// <returns></returns>
        public (AiData aiData, List<BSlot> targetSlots) GetReacationAiData(BSlot reactionSlot, List<BSlot> opponentSlots)
        {
            var reactionAiData = (default(AiData), default(List<BSlot>));

            BaseCard card = reactionSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} 장착된 카드가 없어 리액션 패스");
                return reactionAiData;
            }

            List<(AiGroupData.Reaction, ConditionData)> reactionPairs = GetCardReactionPairs(card, ConditionCheckPoint.TurnStart);

            if (reactionPairs.Count == 0)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} AiGroupData({card.AiGroupId})에 해당하는 <color=red>리액션 데이터 없음</color>");
                return reactionAiData;
            }

            foreach (var (reaction, condition) in reactionPairs)
            {
                //reactionTargetSlots 은 화면 표시를 위해 사용 - 실제 타겟은 AiData 에서 얻음
                var (aiDataId, reactionTargetSlots) = ConditionLogic.Instance.GetReactionConditionAiDataId((reaction, condition), reactionSlot, opponentSlots);

                if (aiDataId == 0)
                {
                    Debug.Log($"{Utils.BoardSlotLog(reactionSlot)} 이번 턴 리액션 컨디션({condition.id}) 없음");
                    continue;
                }

                AiData aiData = AiData.GetAiData(aiDataId);

                string aiGroupDataTableLog = $"{Utils.BoardSlotLog(reactionSlot)} <color=#78d641>AiData</color> 테이블 {aiDataId} 데이터 얻기";

                if (aiData == null)
                {
                    Debug.LogWarning($"{aiGroupDataTableLog} - 실패. <color=red>테이블에 데이터 없음</color>");
                    continue;
                }

                List<BSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, reactionSlot);

                if (aiTargetSlots.Count == 0)
                {
                    Debug.LogWarning($"{aiGroupDataTableLog} - 실패. 대상 슬롯 없음");
                    continue;
                }

                Debug.Log($"{aiGroupDataTableLog} 성공 - {Utils.BoardSlotLog(reactionSlot)}. 리액션 컨디션({condition.id}) AiData({aiDataId}) 작동");

                reactionAiData = (aiData, aiTargetSlots);
                break;
            }

            return reactionAiData;
        }

        /// <summary>
        /// 턴 시작 액션 AiData id 얻기
        /// </summary>
        public int GetTurnStartActionAiDataId(BSlot selfSlot, List<BSlot> opponentSlots)
        {
            BaseCard card = selfSlot.Card;

            List<(AiGroupData.Reaction, ConditionData)> reactionPairs = GetCardReactionPairs(card, ConditionCheckPoint.TurnStart);

            if (reactionPairs.Count == 0)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} AiGroupData({card.AiGroupId})에 해당하는 <color=red>리액션 데이터 없음</color>");
                return 0;
            }

            foreach (var (reaction, condition) in reactionPairs)
            {
                // reactionTargetSlots 은 화면 표시를 위해 사용 - 실제 타겟은 AiData 에서 얻음
                var (aiDataId, reactionTargetSlots) = ConditionLogic.Instance.GetReactionConditionAiDataId((reaction, condition), selfSlot, opponentSlots);

                if (aiDataId == 0)
                {
                    // Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 이번 턴 리액션 컨디션({condition.id}) 동작하지 않음");
                    continue;
                }

                AiData aiData = AiData.GetAiData(aiDataId);

                string aiGroupDataTableLog = $"{Utils.BoardSlotLog(selfSlot)} <color=#78d641>AiData</color> 테이블 리액션 {aiDataId} 얻기";

                if (aiData == null)
                {
                    Debug.LogWarning($"{aiGroupDataTableLog} - 실패. <color=red>테이블에 데이터 없음</color>");
                    continue;
                }

                List<BSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot);

                if (aiTargetSlots.Count == 0)
                {
                    Debug.LogWarning($"{aiGroupDataTableLog} - 실패. 대상 슬롯 없음");
                    continue;
                }

                Debug.Log($"{aiGroupDataTableLog} 성공 - 리액션 컨디션({condition.id})에 해당하는 AiData({aiDataId})의 <color=#f4872e>어빌리티({string.Join(", ", aiData.ability_Ids)})</color>{(aiData.ability_Ids.Count > 1 ? " 중 하나" : "")} 작동");

                return aiData.ai_Id;
            }

            return 0;
        }

        /// <summary>
        /// 카드 리액션 조건 데이터 얻기
        /// </summary>
        /// <param name="card"></param>
        /// <param name="checkPoint"></param>
        /// <returns></returns>
        public List<(AiGroupData.Reaction, ConditionData)> GetCardReactionPairs(BaseCard card, ConditionCheckPoint checkPoint)
        {
            List<(AiGroupData.Reaction, ConditionData)> reactionConditionPairs = new();

            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(card.AiGroupId);

            string aiGroupDataTableLog = $"{card.Id} 카드. <color=#78d641>AiGroupData</color> 테이블 {card.AiGroupId} 데이터 얻기";

            if (aiGroupData == null)
            {
                Debug.LogError($"{aiGroupDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - AiLogic.GetTurnStartReaction");
                return reactionConditionPairs;
            }

            // Debug.Log($"{aiGroupDataTableLog} 성공 - Card.GetTurnStartReaction");

            if (aiGroupData.reactions.Count == 0)
            {
                Debug.LogWarning($"{card.Id} 카드. <color=#78d641>AiGroupData</color> 테이블 {card.AiGroupId}에 reaction 설정 없음 - AiLogic.GetTurnStartReaction");
                return reactionConditionPairs;
            }

            foreach (var reaction in aiGroupData.reactions)
            {
                ConditionData conditionData = ConditionData.GetConditionData(reaction.conditionId);

                string conditionDataTableLog = $"{card.Id} 카드. <color=#78d641>ConditionData</color> 테이블 {reaction.conditionId} 데이터 얻기";

                if (conditionData == null)
                {
                    Debug.LogError($"{conditionDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - AiLogic.GetTurnStartReaction");
                    continue;
                }

                // Debug.Log($"{conditionDataTableLog} 성공 - Card.GetTurnStartReaction");

                // 리액션 조건이 아니면 패스
                if (conditionData.checkPoint != checkPoint)
                    continue;

                reactionConditionPairs.Add((reaction, conditionData));
            }

            // 리액션 데이터 로그
            if (reactionConditionPairs.Count > 0)
            {
                string logs = string.Join("\n", reactionConditionPairs.Select((x, index) =>
                $"[{index}] reaction: {JsonConvert.SerializeObject(x.Item1)}, condition: {JsonConvert.SerializeObject(x.Item2)}"));

                // Debug.Log($"{id} 카드. {reactionConditionPairs.Count}개 리액션 컨디션({string.Join(" ,", reactionConditionPairs.Select(pair => pair.Item2.id).ToList())}) 있음");
            }

            return reactionConditionPairs;
        }

        /// <summary>
        /// 어빌리티 데이터 얻기
        /// </summary>
        /// <param name="abilityIds"></param>
        public List<AbilityData> GetAbilityDatas(List<int> abilityIds)
        {
            List<AbilityData> abilityDatas = new();

            foreach (int abilityId in abilityIds)
            {
                AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

                if (abilityData == null)
                {
                    Debug.LogError($"AbilityData({abilityId}) <color=red>테이블에 데이터 없음</color> - BattleLogic.TurnStartReaction");
                    continue;
                }

                abilityDatas.Add(abilityData);
            }

            return abilityDatas;
        }
    }
}