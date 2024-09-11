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

        private static readonly System.Random random = new System.Random();

        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// HandOn 어빌리티를 가진 카드 얻기
        /// </summary>
        public List<(Card card, AiData aiData, List<AbilityData> abilities)> GetHandOnCards(List<Card> handCards)
        {
            List<(Card card, AiData aiData, List<AbilityData> abilities)> handOnCards = new List<(Card, AiData, List<AbilityData>)>();

            foreach (Card handCard in handCards)
            {
                int aiDataId = handCard.GetCardAiDataId();

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

                    if (ability.type == AbilityWorkType.OnHand)
                        abilities.Add(ability);
                }

                if (abilities.Count == 0)
                    continue;

                handOnCards.Add((handCard, handCardAiData, abilities));
            }

            Debug.Log($"HandOn 어빌리티를 가진 카드 {handOnCards.Count}장({string.Join(", ", handOnCards.Select(handOnCard => handOnCard.card.id))}) 확인 - AiLogic.GetHandOnCards");

            return handOnCards;
        }

        /// <summary>
        /// 턴 시작 리액션 AiData 얻기
        /// </summary>
        /// <param name="reactionSlot"></param>
        /// <param name="opponentSlots"></param>
        /// <returns></returns>
        public (AiData aiData, List<BoardSlot> targetSlots) GetReacationAiData(BoardSlot reactionSlot, List<BoardSlot> opponentSlots)
        {
            var reactionAiData = (default(AiData), default(List<BoardSlot>));

            Card card = reactionSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} 장착된 카드가 없어 리액션 패스 - AiLogic.TurnStartReaction");
                return reactionAiData;
            }

            List<(AiGroupData.Reaction, ConditionData)> reactionPairs = GetCardReactionPairs(card, ConditionCheckPoint.TurnStart);

            if (reactionPairs.Count == 0)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} AiGroupData({card.aiGroupId})에 해당하는 <color=red>리액션 데이터 없음</color> - AiLogic.TurnStartReaction");
                return reactionAiData;
            }

            foreach (var (reaction, condition) in reactionPairs)
            {
                //reactionTargetSlots 은 화면 표시를 위해 사용 - 실제 타겟은 AiData 에서 얻음
                var (aiDataId, reactionTargetSlots) = ConditionLogic.Instance.GetReactionConditionAiDataId((reaction, condition), reactionSlot, opponentSlots);

                if (aiDataId == 0)
                {
                    Debug.Log($"{Utils.BoardSlotLog(reactionSlot)} 이번 턴 리액션 컨디션({condition.id}) 없음 - AiLogic.TurnStartReaction");
                    continue;
                }

                AiData aiData = AiData.GetAiData(aiDataId);

                string aiGroupDataTableLog = $"{Utils.BoardSlotLog(reactionSlot)} <color=#78d641>AiData</color> 테이블 {aiDataId} 데이터 얻기";

                if (aiData == null)
                {
                    Debug.LogWarning($"{aiGroupDataTableLog} - 실패. <color=red>테이블에 데이터 없음</color> - AiLogic.TurnStartReaction");
                    continue;
                }

                List<BoardSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, reactionSlot);

                if (aiTargetSlots.Count == 0)
                    continue;

                Debug.Log($"{aiGroupDataTableLog} 성공 - {Utils.BoardSlotLog(reactionSlot)}. 리액션 컨디션({condition.id}) AiData({aiDataId}) 작동 - AiLogic.TurnStartReaction");

                reactionAiData = (aiData, aiTargetSlots);
                break;
            }

            return reactionAiData;
        }

        /// <summary>
        /// 카드 리액션 조건 데이터 얻기
        /// </summary>
        /// <param name="card"></param>
        /// <param name="checkPoint"></param>
        /// <returns></returns>
        public List<(AiGroupData.Reaction, ConditionData)> GetCardReactionPairs(Card card, ConditionCheckPoint checkPoint)
        {
            int aiGroupId = card.aiGroupId;

            List<(AiGroupData.Reaction, ConditionData)> reactionConditionPairs = new List<(AiGroupData.Reaction, ConditionData)>();

            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            string aiGroupDataTableLog = $"{card.id} 카드. <color=#78d641>AiGroupData</color> 테이블 {aiGroupId} 데이터 얻기";

            if (aiGroupData == null)
            {
                Debug.LogError($"{aiGroupDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - AiLogic.GetTurnStartReaction");
                return reactionConditionPairs;
            }

            // Debug.Log($"{aiGroupDataTableLog} 성공 - Card.GetTurnStartReaction");

            if (aiGroupData.reactions.Count == 0)
            {
                Debug.LogWarning($"{card.id} 카드. <color=#78d641>AiGroupData</color> 테이블 {aiGroupId}에 reaction 설정 없음 - AiLogic.GetTurnStartReaction");
                return reactionConditionPairs;
            }

            foreach (var reaction in aiGroupData.reactions)
            {
                ConditionData conditionData = ConditionData.GetConditionData(reaction.conditionId);

                string conditionDataTableLog = $"{card.id} 카드. <color=#78d641>ConditionData</color> 테이블 {reaction.conditionId} 데이터 얻기";

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
    }
}