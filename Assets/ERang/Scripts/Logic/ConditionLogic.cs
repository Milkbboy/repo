using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;
using Newtonsoft.Json;

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
        public (int aiDataId, List<int> targetSlots) GetReactionConditionAiDataId((AiGroupData.Reaction, ConditionData) reactionPairs, BoardSlot selfSlot, List<BoardSlot> opponentSlots)
        {
            var (reaction, condition) = reactionPairs;

            // Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfSlot.Card.id}). 리액션 컨디션({reaction.conditionId}) 확인 시작 - ConditionLogic.GetReactionConditionAiDataId");

            string conditionTargetLog = $"{selfSlot.Slot}번 슬롯 카드({selfSlot.Card.id}). 리액션 컨디션({condition.id}) 타겟 {condition.target.ToString()} 슬롯 찾기";

            List<BoardSlot> targetSlots = GetConditionTargets(condition, selfSlot, opponentSlots);

            if (targetSlots.Count == 0)
            {
                Debug.LogWarning($"{conditionTargetLog} - 실패. 대상 없음 - ConditionLogic.GetReactionConditionAiDataId");
                return (0, new List<int>());
            }

            Debug.Log($"{conditionTargetLog} - 성공. 타겟 슬롯 <color=yellow>{string.Join(", ", targetSlots.Select(slot => slot.Slot))}</color> 에 대한 리액션 발동 확인 - ConditionLogic.GetReactionConditionAiDataId");

            string conditionCheckLog = $"{selfSlot.Slot}번 슬롯 카드({selfSlot.Card.id}).";

            var result = (0, new List<int>());

            foreach (var targetSlot in targetSlots)
            {
                Card targetCard = targetSlot.Card;

                // 슬롯에 카드가 없으면 패스
                if (targetCard == null)
                {
                    Debug.LogWarning($"{conditionCheckLog} 타겟 슬롯 <color=yellow>{targetSlot.Slot}</color>에 대한 리액션 컨디션({condition.id}) 발동 확인 - 실패. 슬롯에 장착된 카드 없음 - ConditionLogic.GetReactionConditionAiDataId");
                    continue;
                }

                string targetConditionLog = $"{conditionCheckLog} 타겟 슬롯 <color=yellow>{targetSlot.Slot}</color> 카드({targetCard.id})에 대한 리액션 컨디션({condition.id}) {condition.type.ToString()} 조건 비교";

                if (condition.type == ConditionType.EveryTurn)
                {
                    if (ConditionRatio(condition.id, reaction.ratio))
                    {
                        result = (reaction.aiDataId, new List<int> { targetSlot.Slot });
                        // Debug.Log($"{targetConditionLog} - 성공 (발생 확률만 비교) 설정된 aiDataId({result.Item1}) - ConditionLogic.GetReactionConditionAiDataId");
                    }
                    else
                    {
                        // Debug.LogWarning($"{targetConditionLog} - 실패 (발생 확률만 비교) - ConditionLogic.GetReactionConditionAiDataId");
                    }
                }
                else
                {
                    int compareValue = 0;

                    switch (condition.type)
                    {
                        // 대상의 버프 상태 확인
                        case ConditionType.Buff:
                            compareValue = targetCard.BuffCount;
                            break;
                        // 대상의 디버프 상태 확인
                        case ConditionType.Debuff:
                            compareValue = targetCard.DeBuffCount;
                            break;
                        // 대상의 체력 상태 확인
                        case ConditionType.Hp:
                            compareValue = targetCard.hp;
                            break;
                        case ConditionType.Extinction:
                        case ConditionType.Acquisition:
                            Debug.LogWarning($"{selfSlot.Slot}번 슬롯 카드({selfSlot.Card.id}). ConditionType.Extinction, ConditionType.Acquisition 아직 구현 전 - ConditionLogic.GetReactionConditionAiDataId");
                            break;
                    }

                    // 해당 조건의 컨디션이 있으면 다음 컨디션은 검사하지 않는다.
                    if (ConditionCompare(condition, compareValue) && ConditionRatio(condition.id, reaction.ratio))
                    {
                        result = (reaction.aiDataId, new List<int> { targetSlot.Slot });
                        // Debug.Log($"{targetConditionLog} - 성공 (조건 비교와 발생 확률 모두 통과). 리액션에 설정된 aiDataId({result.Item1}) - ConditionLogic.GetReactionConditionAiDataId");
                    }
                    else
                    {
                        // Debug.LogWarning($"{targetConditionLog} - 실패 (조건 비교 또는 발생 확률 실패) - ConditionLogic.GetReactionConditionAiDataId");
                    }
                }

                // 리액션 조건이 성공하면 다음 조건은 검사하지 않는다.
                if (result.Item1 != 0)
                    break;
            }

            return result;
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
                    BoardSlot occupiedSlot = opponentSlots.Find(slot => slot.Card != null);

                    // 타겟 슬롯에 카드가 없으면 제일 첫번째 슬롯 반환 (타겟 슬롯 확인 용도)
                    if (occupiedSlot != null)
                        targetSlots.Add(occupiedSlot);
                    else
                        targetSlots.Add(opponentSlots[0]);
                    break;
                case ConditionTarget.Self:
                    targetSlots.Add(selfSlot);
                    break;
                case ConditionTarget.Enemy1:
                    targetSlots.Add(opponentSlots[0]);
                    break;
                case ConditionTarget.Enemy2:
                    targetSlots.Add(opponentSlots[1]);
                    break;
                case ConditionTarget.Enemy3:
                    targetSlots.Add(opponentSlots[2]);
                    break;
                case ConditionTarget.Enemy4:
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
                    Debug.LogWarning("{selfSlot.Slot}번 슬롯. 카드 대상 미구현 - ConditionLogic.GetConditionTargets");
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
                Debug.Log($"컨디션 조건 비교 값 없으면 그냥 성공 - condition: {condition.id}) - ConditionLogic.ConditionCompare");
                return true;
            }

            bool result = condition.compare switch
            {
                "==" => value == condition.value,
                "<=" => value <= condition.value,
                ">=" => value >= condition.value,
                _ => false,
            };

            // Debug.Log($"컨디션({condition.id}) 조건 비교 결과 {result}. target Value({value}) {condition.compare} condition.value({condition.value}) - ConditionLogic:ConditionCompare");

            return result;
        }

        /// <summary>
        /// 컨디션 발생 확률
        /// - AiGroupData 테이블에 정의 되어 있음
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        private bool ConditionRatio(int conditionId, float ratio)
        {
            double randomValue = random.NextDouble();
            bool result = ratio == 1f || randomValue <= ratio;

            // Debug.Log($"컨디션({conditionId}) 발생 확률 결과 {result} - randomValue({randomValue:F1}) <= ratio({ratio}) - ConditionLogic:ConditionRatio");

            return result;
        }
    }
}