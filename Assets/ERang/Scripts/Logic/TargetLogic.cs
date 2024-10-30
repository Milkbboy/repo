using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class TargetLogic : MonoBehaviour
    {
        public static TargetLogic Instance { get; private set; }

        private const int BOARD_CENTER_OFFSET = 3;

        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// AiData 테이블의 대상 슬롯 얻기
        /// </summary>
        public List<BoardSlot> GetAiTargetSlots(AiData aiData, BoardSlot selfSlot)
        {
            List<BoardSlot> targetSlots = new List<BoardSlot>();

            switch (aiData.target)
            {
                case AiDataTarget.Self: return new List<BoardSlot> { selfSlot };
                case AiDataTarget.Enemy: return TargetEnemy(aiData, selfSlot);
                case AiDataTarget.NearEnemy: return TargetNearEnemy(aiData, selfSlot);
                case AiDataTarget.AllEnemy: return TargetAllEnemy(selfSlot);
                case AiDataTarget.AllEnemyCreature: return TargetAllEnemy(selfSlot, true);
                case AiDataTarget.RandomEnemy: return TargetRandomEnemy(selfSlot);
                case AiDataTarget.RandomEnemyCreature: return TargetRandomEnemy(selfSlot, true);
                case AiDataTarget.AllFriendly: return TargetAllFriendly(selfSlot, true);
                case AiDataTarget.AllFriendlyCreature: return TargetAllFriendly(selfSlot);
                case AiDataTarget.FirstEnemy: return TargetFirstEnemy(selfSlot);
                case AiDataTarget.SecondEnemy: return TargetSecondEnemy(selfSlot);
                case AiDataTarget.None:
                default:
                    Debug.LogWarning($"{aiData.ai_Id} - 대상이 없음");
                    break;
            }

            if (targetSlots.Count > 0)
                Debug.Log($"{Utils.BoardSlotLog(selfSlot)} AiData({aiData.ai_Id})에 설정된 타겟({aiData.target})({string.Join(", ", targetSlots.Select(slot => slot.Card.Id))}) 얻기 완료");

            return targetSlots;
        }

        /// <summary>
        /// AiData AttackType 선택 타입 대상 슬롯 얻기
        /// </summary>
        public List<BoardSlot> GetSelectAttackTypeTargetSlot(AiDataAttackType aiDataAttackType)
        {
            List<BoardSlot> targetSlots = new List<BoardSlot>();

            switch (aiDataAttackType)
            {
                case AiDataAttackType.SelectEnemy:
                case AiDataAttackType.SelectEnemyCreature:
                    targetSlots = BoardSystem.Instance.GetMonsterBoardSlots();
                    break;
                case AiDataAttackType.SelectFriendly:
                case AiDataAttackType.SelectFriendlyCreature:
                    targetSlots = BoardSystem.Instance.GetCreatureBoardSlots();
                    break;
            }

            return targetSlots;
        }

        private List<BoardSlot> TargetEnemy(AiData aiData, BoardSlot selfSlot)
        {
            List<BoardSlot> targets = new List<BoardSlot>();

            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            switch (aiData.type)
            {
                case AiDataType.Melee:
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetSlotIndex = attackRange - 1;

                        // 근접 공격 거리가 상대 카드 개수 보다 크면 패스
                        if (targetSlotIndex < 0 || targetSlotIndex >= opponentSlots.Count)
                        {
                            Debug.LogWarning($"{aiData.ai_Id} - targetSlotIndex is out of range. targetSlotIndex: {targetSlotIndex}, targetBoardSlots.Count: {opponentSlots.Count}");
                            continue;
                        }

                        targets.Add(opponentSlots[targetSlotIndex]);
                    }
                    break;

                case AiDataType.Ranged:
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetSlotIndex = attackRange - (selfSlot.Index + BOARD_CENTER_OFFSET);

                        if (targetSlotIndex < 0 || targetSlotIndex >= opponentSlots.Count)
                        {
                            Debug.LogWarning($"{aiData.ai_Id} - targetSlotIndex is out of range. targetSlotIndex: {targetSlotIndex}, targetBoardSlots.Count: {opponentSlots.Count}");
                            continue;
                        }

                        targets.Add(opponentSlots[targetSlotIndex]);
                    }
                    break;

                case AiDataType.Explosion:
                    Debug.LogWarning($"{aiData.ai_Id} - AiDataType.Explosion 아직 구현되지 않음");
                    break;
            }

            return targets;
        }

        /// <summary>
        /// 카드가 장착된 첫번째 카드를 타겟으로 설정
        /// </summary>
        private List<BoardSlot> TargetNearEnemy(AiData aiData, BoardSlot selfSlot)
        {
            List<BoardSlot> targets = new List<BoardSlot>();

            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            // 제일 근접한 타겟 찾기
            BoardSlot targetSlot = opponentSlots.FirstOrDefault(x => x.Card != null);
            int targetIndex = targetSlot.Index;

            // Debug.Log($"{aiData.ai_Id} - 제일 근접한 타겟 슬롯 인덱스 {targetIndex} 찾고 attackRanges({(aiData.attackRanges.Count > 0 ? string.Join(", ", aiData.attackRanges) : "없음")}) 에 설정된 타겟 찾기");

            if (aiData.attackRanges.Count == 0)
            {
                // Debug.LogWarning($"{aiData.ai_Id} - attackRanges 가 설정되지 않아서 제일 근접한 타겟만 찾음");
                targets.Add(targetSlot);

                return targets;
            }

            for (int i = 0; i < aiData.attackRanges.Count; ++i)
            {
                int attackRange = aiData.attackRanges[i];
                int targetSlotIndex = targetSlot.Index + (attackRange - 1);

                if (targetSlotIndex < 0 || targetSlotIndex >= opponentSlots.Count)
                {
                    // Debug.LogWarning($"{aiData.ai_Id} - {i}번째 타겟 슬롯 인덱스 {targetSlotIndex} 로 패스 (적용 범위 0 ~ 3)");
                    continue;
                }
                else
                {
                    // Debug.Log($"{aiData.ai_Id} - {i}번째 타겟 슬롯 인덱스 {targetSlotIndex} 찾기");
                }

                targets.Add(opponentSlots[targetSlotIndex]);
            }

            return targets;
        }

        private List<BoardSlot> TargetAllEnemy(BoardSlot selfSlot, bool exceptMaster = false)
        {
            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => x.CardType != CardType.Master || x.CardType != CardType.EnemyMaster).ToList();

            // Debug.Log($"TargetAllEnemy - exceptMaster: {exceptMaster}, targetSlots: {string.Join(", ", opponentSlots.Select(x => x.Slot))}");

            return opponentSlots;
        }

        private List<BoardSlot> TargetAllFriendly(BoardSlot selfSlot, bool exceptMaster = false)
        {
            // 아군 슬롯 리스트
            List<BoardSlot> friendlySlots = BoardSystem.Instance.GetFriendlySlots(selfSlot);

            if (exceptMaster)
                friendlySlots = friendlySlots.Where(x => x.CardType != CardType.Master || x.CardType != CardType.EnemyMaster).ToList();

            // Debug.Log($"TargetAllFriendly - exceptMaster: {exceptMaster}, targetSlots: {string.Join(", ", friendlySlots.Select(x => x.Slot))}");

            return friendlySlots;
        }

        private List<BoardSlot> TargetRandomEnemy(BoardSlot selfSlot, bool exceptMaster = false)
        {
            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => x.CardType != CardType.Master || x.CardType != CardType.EnemyMaster).ToList();

            int randomIndex = Random.Range(0, opponentSlots.Count);

            if (randomIndex < 0 || randomIndex >= opponentSlots.Count)
            {
                Debug.LogError($"randomIndex is out of range. randomIndex: {randomIndex}, opponentSlots.Count: {opponentSlots.Count}");
                return null;
            }

            return new List<BoardSlot> { opponentSlots[randomIndex] };
        }

        /// <summary>
        /// 첫번째 적 타겟 설정
        /// </summary>
        /// <param name="slefSlot"></param>
        /// <returns></returns>
        public List<BoardSlot> TargetFirstEnemy(BoardSlot slefSlot)
        {
            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(slefSlot);

            BoardSlot targetSlot = opponentSlots.FirstOrDefault(x => x.Card != null);

            Debug.Log($"TargetFirstEnemy - targetSlot: {targetSlot.Slot}, opponentSlots: {string.Join(", ", opponentSlots.Select(x => x.Slot))}");

            return new List<BoardSlot> { targetSlot };
        }

        /// <summary>
        /// 두번째 적 타겟 설정
        /// </summary>
        /// <param name="slefSlot"></param>
        /// <returns></returns>
        public List<BoardSlot> TargetSecondEnemy(BoardSlot slefSlot)
        {
            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(slefSlot);

            // 슬롯에 장착된 두 번째 카드
            BoardSlot secondSlot = opponentSlots.Where(x => x.Card != null).ElementAtOrDefault(1);

            // 두 번째 카드가 없으면 첫 번째 카드
            if (secondSlot == null)
            {
                secondSlot = opponentSlots.FirstOrDefault(x => x.Card != null);
            }

            Debug.Log($"TargetFirstEnemy - secondSlot: {secondSlot.Slot}, opponentSlots: {string.Join(", ", opponentSlots.Select(x => x.Slot))}");

            // 첫 번째 카드도 없으면 null 반환
            return new List<BoardSlot> { secondSlot };
        }
    }
}