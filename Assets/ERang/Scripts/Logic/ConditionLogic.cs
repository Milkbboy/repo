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
        /// 컨디션 타입별 조건 비교
        /// </summary>
        /// <param name="reaction"></param>
        /// <param name="condition"></param>
        /// <param name="targets"></param>
        public int GetReactionConditionAiDataId((AiGroupData.Reaction, ConditionData) reactionPairs, Card selfCard, List<Card> opponentCards)
        {
            var (reaction, condition) = reactionPairs;
            List<Card> targetCards = GetConditionTargets(condition, selfCard, opponentCards);

            Debug.Log($"ConditionLogic.GetReactionConditionAiDataId: 리액션 컨디션 aiDataId 얻기 - reaction: {reaction.conditionId}, condition: {condition.id}, targets: {targetCards.Count}");

            foreach (var targetCard in targetCards)
            {
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
        /// <param name="selfCard">나(자신)</param>
        /// <param name="opponentCards">상대(적)</param>
        /// <returns></returns>
        public List<Card> GetConditionTargets(ConditionData conditionData, Card selfCard, List<Card> opponentCards)
        {
            List<Card> targets = new List<Card>();

            switch (conditionData.target)
            {
                case ConditionTarget.NearEnemy:
                    if (opponentCards.Count > 0)
                        targets.Add(opponentCards.FirstOrDefault());
                    break;
                case ConditionTarget.Self:
                    targets.Add(selfCard);
                    break;
                case ConditionTarget.Enemy1:
                    if (opponentCards.Count > 0 && opponentCards[0] != null)
                        targets.Add(opponentCards[0]);
                    break;
                case ConditionTarget.Enemy2:
                    if (opponentCards.Count > 1 && opponentCards[1] != null)
                        targets.Add(opponentCards[1]);
                    break;
                case ConditionTarget.Enemy3:
                    if (opponentCards.Count > 2 && opponentCards[2] != null)
                        targets.Add(opponentCards[2]);
                    break;
                case ConditionTarget.Enemy4:
                    if (opponentCards.Count > 3 && opponentCards[3] != null)
                        targets.Add(opponentCards[3]);
                    break;
                case ConditionTarget.FriendlyCreature:
                    return Board.Instance.GetOccupiedCreatureCards();
                case ConditionTarget.EnemyCreature:
                    return Board.Instance.GetOccupiedMonsterCards();
                case ConditionTarget.Card:
                    Debug.LogWarning("ConditionLogic.GetConditionTargets: 카드 대상 미구현.");
                    break;
            }

            return targets;
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