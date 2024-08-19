using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;

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
        /// 컨디션 조건에 맞는 대상 얻기
        /// </summary>
        /// <param name="conditionData"></param>
        /// <param name="self"></param>
        /// <param name="enemyCards"></param>
        /// <returns></returns>
        public List<Card> GetConditionTargets(ConditionData conditionData, Card self, List<Card> enemyCards)
        {
            List<Card> targets = new List<Card>();

            switch (conditionData.target)
            {
                case ConditionTarget.NearEnemy:
                    if (enemyCards.Count > 0)
                        targets.Add(enemyCards.FirstOrDefault());
                    break;
                case ConditionTarget.Self: targets.Add(self); break;
                case ConditionTarget.Enemy1:
                    if (enemyCards.Count > 0 && enemyCards[0] != null)
                        targets.Add(enemyCards[0]);
                    break;
                case ConditionTarget.Enemy2:
                    if (enemyCards.Count > 1 && enemyCards[1] != null)
                        targets.Add(enemyCards[1]);
                    break;
                case ConditionTarget.Enemy3:
                    if (enemyCards.Count > 2 && enemyCards[2] != null)
                        targets.Add(enemyCards[2]);
                    break;
                case ConditionTarget.Enemy4:
                    if (enemyCards.Count > 3 && enemyCards[3] != null)
                        targets.Add(enemyCards[3]);
                    break;
                case ConditionTarget.FriendlyCreature: return Board.Instance.GetOccupiedCreatureCards();
                case ConditionTarget.EnemyCreature: return Board.Instance.GetOccupiedMonsterCards();
                case ConditionTarget.Card:
                    Debug.LogWarning("ConditionLogic.GetConditionTargets() - ConditionTarget.Card is not implemented yet.");
                    break;
            }

            return new List<Card>();
        }

        /// <summary>
        /// 컨디션 타이별 조건 비교
        /// </summary>
        /// <param name="reaction"></param>
        /// <param name="condition"></param>
        /// <param name="targets"></param>
        public int GetReactionConditionAiDataId(AiGroupData.Reaction reaction, ConditionData condition, List<Card> targets)
        {
            Debug.Log($"GetReactionConditionAiDataId - reaction: {reaction.conditionId}, condition: {condition.id}, targets: {targets.Count}");

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                int compareValue = 0;

                switch (condition.type)
                {
                    // 대상의 버프 상태 확인
                    case ConditionType.Buff: compareValue = target.BuffCount; break;
                    // 대상의 디버프 상태 확인
                    case ConditionType.Debuff: compareValue = target.DeBuffCount; break;
                    // 대상의 체력 상태 확인
                    case ConditionType.Hp: compareValue = target.hp; break;
                    // 대상의 모든 턴
                    case ConditionType.EveryTurn:
                        Debug.LogWarning($"GetReactionConditionAiData - ConditionType.EveryTurn aiDataId {reaction.aiDataId} return.");
                        return reaction.aiDataId;
                    case ConditionType.Extinction:
                    case ConditionType.Acquisition:
                        Debug.LogWarning("GetReactionConditionAiData - ConditionType.Extinction, ConditionType.Acquisition is not implemented yet.");
                        break;
                }

                // 해당 조건의 컨디션이 있으면 다음 컨디션은 패스
                if (ConditionCompare(condition, compareValue) && ConditionRatio(reaction.ratio))
                {
                    return reaction.aiDataId;
                }
            }

            return 0;
        }

        /// <summary>
        /// 컨디션 조건 비교
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool ConditionCompare(ConditionData condition, int value)
        {
            Debug.Log($"ConditionCompare - condition: {condition.id}, value({value}) {condition.compare} condition.value({condition.value}), ");

            // 비교 조건이 없으면 true
            if (string.IsNullOrEmpty(condition.compare))
            {
                return true;
            }

            return condition.compare switch
            {
                "==" => value == condition.value,
                "<=" => value <= condition.value,
                ">=" => value >= condition.value,
                _ => false,
            };
        }

        private bool ConditionRatio(float ratio)
        {
            double randomValue = random.NextDouble();

            Debug.Log($"ConditionRatio - ratio: {ratio}, randomValue: {randomValue}");

            return (ratio == 1f || randomValue <= ratio);
        }
    }
}