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
        public List<BSlot> GetAiTargetSlots(AiData aiData, BSlot selfSlot, string whereFrom = "")
        {
            List<BSlot> targetSlots = new List<BSlot>();

            switch (aiData.target)
            {
                case AiDataTarget.Self: targetSlots = new List<BSlot> { selfSlot }; break;
                case AiDataTarget.Enemy: targetSlots = TargetEnemy(aiData, selfSlot); break;
                case AiDataTarget.NearEnemy: targetSlots = TargetNearEnemy(aiData, selfSlot); break;
                case AiDataTarget.AllEnemy: targetSlots = TargetAllEnemy(selfSlot); break;
                case AiDataTarget.AllEnemyCreature: targetSlots = TargetAllEnemy(selfSlot, true); break;
                case AiDataTarget.RandomEnemy: targetSlots = TargetRandomEnemy(selfSlot); break;
                case AiDataTarget.RandomEnemyCreature: targetSlots = TargetRandomEnemy(selfSlot, true); break;
                case AiDataTarget.AllFriendly: targetSlots = TargetAllFriendly(selfSlot, true); break;
                case AiDataTarget.AllFriendlyCreature: targetSlots = TargetAllFriendly(selfSlot); break;
                case AiDataTarget.FirstEnemy: targetSlots = TargetFirstEnemy(selfSlot); break;
                case AiDataTarget.SecondEnemy: targetSlots = TargetSecondEnemy(selfSlot); break;
                case AiDataTarget.None:
                default:
                    Debug.LogWarning($"{aiData.ai_Id} - 대상이 없음. {whereFrom} 에서 호출");
                    break;
            }

            if (targetSlots.Count > 0)
                Debug.Log($"{selfSlot.LogText} AiData({aiData.ai_Id})에 설정된 {aiData.target} 타겟. [{string.Join(", ", targetSlots.Select(slot => (slot.SlotNum, slot.Card?.Id ?? 0)))}] 얻기 완료. ({whereFrom})");

            return targetSlots;
        }

        /// <summary>
        /// AiData AttackType 선택 타입 대상 슬롯 얻기
        /// </summary>
        public List<BSlot> GetSelectAttackTypeTargetSlot(AiDataAttackType aiDataAttackType)
        {
            List<BSlot> targetSlots = new List<BSlot>();

            switch (aiDataAttackType)
            {
                case AiDataAttackType.SelectEnemy:
                case AiDataAttackType.SelectEnemyCreature:
                    targetSlots = BoardSystem.Instance.GetRightBoardSlots();
                    break;
                case AiDataAttackType.SelectFriendly:
                case AiDataAttackType.SelectFriendlyCreature:
                    targetSlots = BoardSystem.Instance.GetLeftBoardSlots();
                    break;
            }

            if (targetSlots.Count > 0)
                Debug.Log($"{aiDataAttackType} 타겟. [{string.Join(", ", targetSlots.Select(slot => (slot.SlotNum, slot.Card?.Id ?? 0)))}] 얻기 완료");

            return targetSlots;
        }

        private List<BSlot> TargetEnemy(AiData aiData, BSlot selfSlot)
        {
            List<BSlot> targets = new List<BSlot>();

            // 상대방 슬롯 리스트
            List<BSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            // 전체 슬롯 리스트
            List<BSlot> allSlots = BoardSystem.Instance.GetAllSlots();

            // 방향
            int direction = selfSlot.SlotCardType == CardType.Creature ? 1 : -1;
            Debug.Log($"{selfSlot.SlotCardType} - {selfSlot.SlotNum}번 슬롯 방향: {(selfSlot.SlotCardType == CardType.Creature ? "left => right" : "right => left")}");

            switch (aiData.type)
            {
                case AiDataType.Melee:
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetSlotIndex = attackRange - 1;

                        // 근접 공격 거리가 상대 카드 개수 보다 크면 패스
                        if (targetSlotIndex < 0 || targetSlotIndex >= opponentSlots.Count)
                        {
                            Debug.LogWarning($"{aiData.ai_Id} - targetSlotIndex is out of range. targetSlotIndex: {targetSlotIndex}, targetBSlots.Count: {opponentSlots.Count}");
                            continue;
                        }

                        targets.Add(opponentSlots[targetSlotIndex]);
                    }
                    break;

                case AiDataType.Ranged:
                    // 3, 4
                    // 3002 - targetSlotIndex is out of range. targetSlotIndex: -3, targetBSlots.Count: 5
                    // Ranged의 경우 자신의 위치를 기준으로 지정된 값 만큼의 거리를 의미한다. 
                    // (Ex 4의 경우 자신의 4칸 앞을 향해 공격한다는 것을 의미, 4와 5가 입력된 경우 자신의 앞 4번째 그리고 5번째 적까지 공격한다는 의미)
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetSlotNum = selfSlot.SlotNum + (attackRange * direction);

                        if (targetSlotNum < 0 || targetSlotNum >= Constants.BoardSlotCount)
                        {
                            Debug.LogWarning($"{aiData.ai_Id} - targetSlotNum({targetSlotNum}) is out of range. targetSlotNum: {targetSlotNum}, targetBSlots.Count: {Constants.BoardSlotCount}");
                            continue;
                        }

                        targets.Add(allSlots[targetSlotNum]);
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
        private List<BSlot> TargetNearEnemy(AiData aiData, BSlot selfSlot)
        {
            List<BSlot> targets = new();

            // 상대방 슬롯 리스트
            List<BSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            // 확인 코드
            // Debug.Log($"{aiData.target}, {string.Join(", ", opponentSlots.Select(x => x.SlotNum))} 에서 타겟 찾기");

            // foreach (BSlot oppentSlot in opponentSlots)
            // {
            //     Debug.Log($"oppentSlot: {oppentSlot.SlotNum}, 카드 {(oppentSlot.Card is null ? "없음" : "있음")}");
            //     Debug.Log($"oppentSlot: {oppentSlot.SlotNum}, oppentSlot.Card: {oppentSlot.Card?.Id ?? 0}");
            // }

            // List<BSlot> cardSlot = opponentSlots.Where(x => x.Card != null).ToList();
            // Debug.Log($"카드가 있는 슬롯: {string.Join(", ", cardSlot.Select(x => x.SlotNum))}");

            // 제일 근접한 타겟 찾기
            BSlot targetSlot = opponentSlots.FirstOrDefault(x => x.Card != null);

            if (targetSlot == null)
            {
                Debug.LogWarning($"{aiData.ai_Id} - 첫번째 카드가 없어서 패스");
                return targets;
            }

            int targetSlotNum = targetSlot.SlotNum;

            // Debug.Log($"{aiData.ai_Id} - 제일 근접한 타겟 SlotNum {targetSlot.SlotNum}, Index: {targetSlot.Index} 찾고 attackRanges({(aiData.attackRanges.Count > 0 ? string.Join(", ", aiData.attackRanges) : "없음")}) 에 설정된 타겟 찾기");

            if (aiData.attackRanges.Count == 0)
            {
                // Debug.LogWarning($"{aiData.ai_Id} - attackRanges 가 설정되지 않아서 제일 근접한 타겟만 찾음");
                targets.Add(targetSlot);

                return targets;
            }

            // Atk_Range: 공격 범위를 설정한다.
            // 1은 바로 앞의 적을 의미하고, 2는 2칸 앞의 적을 의미한다. 
            // Melee의 경우 (Ex 1만 입력된 경우 바로 앞의 적을 공격, 1과 2가 입력된 경우 자신의 앞과 그 뒤의 적까지 공격) 
            // Ranged의 경우 자신의 위치를 기준으로 지정된 값 만큼의 거리를 의미한다. (Ex 4의 경우 자신의 4칸 앞을 향해 공격한다는 것을 의미, 4와 5가 입력된 경우 자신의 앞 4번째 그리고 5번째 적까지 공격한다는 의미)
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

        private List<BSlot> TargetAllEnemy(BSlot selfSlot, bool exceptMaster = false)
        {
            // 상대방 슬롯 리스트
            List<BSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => x.SlotCardType != CardType.Master || x.SlotCardType != CardType.EnemyMaster).ToList();

            // Debug.Log($"TargetAllEnemy - exceptMaster: {exceptMaster}, targetSlots: {string.Join(", ", opponentSlots.Select(x => x.Slot))}");

            return opponentSlots;
        }

        private List<BSlot> TargetAllFriendly(BSlot selfSlot, bool exceptMaster = false)
        {
            // 아군 슬롯 리스트
            List<BSlot> friendlySlots = BoardSystem.Instance.GetFriendlySlots(selfSlot);

            if (exceptMaster)
                friendlySlots = friendlySlots.Where(x => x.SlotCardType != CardType.Master || x.SlotCardType != CardType.EnemyMaster).ToList();

            // Debug.Log($"TargetAllFriendly - exceptMaster: {exceptMaster}, targetSlots: {string.Join(", ", friendlySlots.Select(x => x.Slot))}");

            return friendlySlots;
        }

        private List<BSlot> TargetRandomEnemy(BSlot selfSlot, bool exceptMaster = false)
        {
            // 상대방 슬롯 리스트
            List<BSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);

            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => x.SlotCardType != CardType.Master || x.SlotCardType != CardType.EnemyMaster).ToList();

            int randomIndex = Random.Range(0, opponentSlots.Count);

            if (randomIndex < 0 || randomIndex >= opponentSlots.Count)
            {
                Debug.LogError($"randomIndex is out of range. randomIndex: {randomIndex}, opponentSlots.Count: {opponentSlots.Count}");
                return null;
            }

            return new List<BSlot> { opponentSlots[randomIndex] };
        }

        /// <summary>
        /// 첫번째 적 타겟 설정
        /// </summary>
        /// <param name="slefSlot"></param>
        /// <returns></returns>
        public List<BSlot> TargetFirstEnemy(BSlot slefSlot)
        {
            // 상대방 슬롯 리스트
            List<BSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(slefSlot);

            BSlot targetSlot = opponentSlots.FirstOrDefault(x => x.Card != null);

            Debug.Log($"TargetFirstEnemy - targetSlot: {targetSlot.SlotNum}, opponentSlots: {string.Join(", ", opponentSlots.Select(x => x.SlotNum))}");

            return new List<BSlot> { targetSlot };
        }

        /// <summary>
        /// 두번째 적 타겟 설정
        /// </summary>
        /// <param name="slefSlot"></param>
        /// <returns></returns>
        public List<BSlot> TargetSecondEnemy(BSlot slefSlot)
        {
            // 상대방 슬롯 리스트
            List<BSlot> opponentSlots = BoardSystem.Instance.GetOpponentSlots(slefSlot);

            // 슬롯에 장착된 두 번째 카드
            BSlot secondSlot = opponentSlots.Where(x => x.Card != null).ElementAtOrDefault(1);

            // 두 번째 카드가 없으면 첫 번째 카드
            if (secondSlot == null)
            {
                secondSlot = opponentSlots.FirstOrDefault(x => x.Card != null);
            }

            Debug.Log($"TargetFirstEnemy - secondSlot: {secondSlot.SlotNum}, opponentSlots: {string.Join(", ", opponentSlots.Select(x => x.SlotNum))}");

            // 첫 번째 카드도 없으면 null 반환
            return new List<BSlot> { secondSlot };
        }
    }
}