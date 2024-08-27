using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;
using RogueEngine.UI;

namespace ERang
{
    public class ConditionLogic : MonoBehaviour
    {
        public static ConditionLogic Instance { get; private set; }

        private static readonly System.Random random = new System.Random();

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 리액션 컨디션 aiDataId 얻기
        /// </summary>
        /// <param name="reactionPairs"></param>
        /// <param name="selfSlot"></param>
        /// <param name="opponentSlots"></param>
        /// <returns></returns>
        public int GetReactionConditionAiDataId((AiGroupData.Reaction, ConditionData) reactionPairs, BoardSlot selfSlot, List<BoardSlot> opponentSlots)
        {
            Card selfCard = selfSlot.Card;
            var (reaction, condition) = reactionPairs;

            List<BoardSlot> targetSlots = GetConditionTargets(condition, selfSlot, opponentSlots);

            Debug.Log($"ConditionLogic.GetReactionConditionAiDataId: 리액션 컨디션 aiDataId 얻기 - reaction: {reaction.conditionId}, condition: {condition.id}, targets: {targetSlots.Count}");

            foreach (var targetSlot in targetSlots)
            {
                Card targetCard = targetSlot.Card;

                // 슬롯
                if (targetCard == null)
                    continue;

                int compareValue = 0;

                switch (condition.type)
                {
                    // 대상의 버프 상태 확인
                    case ConditionType.Buff: compareValue = targetCard.BuffCount; break;
                    // 대상의 디버프 상태 확인
                    case ConditionType.Debuff: compareValue = targetCard.DeBuffCount; break;
                    // 대상의 체력 상태 확인
                    case ConditionType.Hp: compareValue = targetCard.hp; break;
                    // 대상의 모든 턴
                    case ConditionType.EveryTurn:
                        if (ConditionRatio(reaction.ratio))
                            return reaction.aiDataId;
                        return 0;
                    case ConditionType.Extinction:
                    case ConditionType.Acquisition:
                        Debug.LogWarning("ConditionLogic.GetReactionConditionAiDataId: ConditionType.Extinction, ConditionType.Acquisition 아직 구현 전.");
                        break;
                }

                // 해당 조건의 컨디션이 있으면 다음 컨디션은 검사하지 않는다.
                if (ConditionCompare(condition, compareValue) && ConditionRatio(reaction.ratio))
                    return reaction.aiDataId;
            }

            Debug.Log($"ConditionLogic.GetReactionConditionAiDataId: 리액션 컨디션 aiDataId 없음. cardId: {selfCard.id}, aiGroupId: {selfCard.aiGroupId}, reaction: {reaction.conditionId}");

            return 0;
        }

        /// <summary>
        /// 컨디션 조건에 맞는 대상 얻기
        /// </summary>
        /// <param name="conditionData"></param>
        /// <param name="selfSlot">내 보드 슬롯</param>
        /// <param name="opponentCards">상대(적)</param>
        /// <returns></returns>
        public List<BoardSlot> GetConditionTargets(ConditionData conditionData, BoardSlot selfSlot, List<BoardSlot> opponentSlots)
        {
            List<BoardSlot> targetSlots = new List<BoardSlot>();

            switch (conditionData.target)
            {
                case ConditionTarget.NearEnemy:
                    if (opponentSlots.Count > 0)
                        targetSlots.Add(opponentSlots.FirstOrDefault());
                    break;
                case ConditionTarget.Self:
                    targetSlots.Add(selfSlot);
                    break;
                case ConditionTarget.Enemy1:
                    if (opponentSlots.Count > 0 && opponentSlots[0] != null)
                        targetSlots.Add(opponentSlots[0]);
                    break;
                case ConditionTarget.Enemy2:
                    if (opponentSlots.Count > 1 && opponentSlots[1] != null)
                        targetSlots.Add(opponentSlots[1]);
                    break;
                case ConditionTarget.Enemy3:
                    if (opponentSlots.Count > 2 && opponentSlots[2] != null)
                        targetSlots.Add(opponentSlots[2]);
                    break;
                case ConditionTarget.Enemy4:
                    if (opponentSlots.Count > 3 && opponentSlots[3] != null)
                        targetSlots.Add(opponentSlots[3]);
                    break;
                case ConditionTarget.FriendlyCreature:
                    if (selfSlot.CardType == CardType.Creature)
                        targetSlots = Board.Instance.GetCreatureBoardSlots();
                    if (selfSlot.CardType == CardType.Monster)
                        targetSlots = Board.Instance.GetMonsterBoardSlots();
                    break;
                case ConditionTarget.EnemyCreature:
                    if (selfSlot.CardType == CardType.Creature)
                        targetSlots = Board.Instance.GetMonsterBoardSlots();
                    if (selfSlot.CardType == CardType.Monster)
                        targetSlots = Board.Instance.GetCreatureBoardSlots();
                    break;
                case ConditionTarget.Card:
                    Debug.LogWarning("ConditionLogic.GetConditionTargetSlots: 카드 대상 미구현.");
                    break;
            }

            return targetSlots;
        }

        /// <summary>
        /// 컨디션 조건 비교
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool ConditionCompare(ConditionData condition, int value)
        {
            // 비교 조건이 없으면 true
            if (string.IsNullOrEmpty(condition.compare))
            {
                Debug.Log($"ConditionLogic.ConditionCompare: 컨디션 조건 비교 값 없으면 그냥 성공 - condition: {condition.id}), ");
                return true;
            }

            bool result = condition.compare switch
            {
                "==" => value == condition.value,
                "<=" => value <= condition.value,
                ">=" => value >= condition.value,
                _ => false,
            };

            Debug.Log($"ConditionLogic:ConditionCompare. 컨디션 조건 비교 결과 {result} - conditionId: {condition.id}, value({value}) {condition.compare} condition.value({condition.value}), ");

            return result;
        }

        /// <summary>
        /// 컨디션 발생 확률
        /// - AiGroupData 테이블에 정의 되어 있음
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        private bool ConditionRatio(float ratio)
        {
            double randomValue = random.NextDouble();
            bool result = (ratio == 1f || randomValue <= ratio);

            Debug.Log($"ConditionLogic:ConditionRatio. 컨디션 발생 확률 결과 {result} - ratio: {ratio}, randomValue: {randomValue}");

            return result;
        }
    }
}