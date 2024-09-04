using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;
using Newtonsoft.Json;


namespace ERang
{
    public class AiLogic : MonoBehaviour
    {
        public static AiLogic Instance { get; private set; }

        private const int BOARD_CENTER_OFFSET = 3;
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
        /// 카드에 설정된 ability 적용
        /// </summary>
        /// <param name="card"></param>
        public void AbilityAction(Card card)
        {
            // 카드가 장착된 보드 슬롯 얻기
            BoardSlot boardSlot = Board.Instance.GetBoardSlot(card.uid);

            string abilityIdsLog = card.Abilities.Count > 0 ? $"{string.Join(", ", card.Abilities.Select(ability => ability.abilityId).ToList())} 확인" : "없음";
            Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} 어빌리티 {abilityIdsLog} - AiLogic.AbilityAction");

            foreach (Card.DurationAbility ability in card.Abilities)
            {
                ability.duration = ability.duration - 1;

                string abilityActionLog = $"{Utils.BoardSlotLog(boardSlot)} {ability.abilityType.ToString()} 어빌리티({ability.abilityId}) 효과 지속 시간(<color=yellow>{ability.duration}</color>)";

                if (ability.duration > 0)
                {
                    Debug.LogWarning($"{abilityActionLog}. 유지 - AiLogic.AbilityAction");
                    continue;
                }

                // duration 이 0 이면 적용을 해제하거나 다른 처리를 해야 함
                switch (ability.abilityType)
                {
                    case AbilityType.AtkUp:
                        {
                            int beforeAtk = card.atk;
                            boardSlot.SetCardAtk(ability.beforeValue);
                            Debug.Log($"{abilityActionLog} 적용 해제 - atk: {beforeAtk} => {card.atk}");
                        }
                        break;
                    case AbilityType.DefUp:
                        boardSlot.SetCardDef(ability.beforeValue);
                        Debug.Log($"{abilityActionLog} 적용 해제 - def: {card.def}");
                        break;
                    case AbilityType.BrokenDef:
                        boardSlot.SetCardDef(ability.beforeValue);
                        Debug.Log($"{abilityActionLog} 적용 해제 - def: {card.def}");
                        break;
                    case AbilityType.ChargeDamage:
                        // target에 데미지를 준다.
                        BoardSlot targetSlot = Board.Instance.GetBoardSlot(ability.targetBoardSlot);
                        targetSlot.SetDamage(ability.abilityValue);
                        string chargeDamageLog = targetSlot.Card != null ? $"데미지 {ability.abilityValue} 적용" : "적용 해제";
                        Debug.Log($"{abilityActionLog} {chargeDamageLog} - AiLogic.AbilityAction");
                        break;
                    default:
                        Debug.LogWarning($"{abilityActionLog} - 아직 구현되지 않음.");
                        break;
                }
            }

            // duration 이 0 이하인 ability 제거
            card.Abilities.RemoveAll(ability => ability.duration <= 0);
        }

        /// <summary>
        /// AiData 에 설정된 어빌리티 적용
        /// </summary>
        public void AiDataAction(AiData aiData, BoardSlot selfSlot)
        {
            Card selfCard = selfSlot.Card;

            if (selfCard == null)
            {
                Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} 장착된 카드 없음 - AiLogic.AiDataAction");
                return;
            }

            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = Board.Instance.GetOpponentSlots(selfSlot);

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} AiData({aiData.ai_Id})에 설정된 타겟({aiData.target.ToString()}) 얻기 시작 - AiLogic.AiDataAction");

            // AiData 에 설정된 타겟 얻기
            List<BoardSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot);

            if (aiTargetSlots.Count == 0)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 설정 타겟({aiData.target.ToString()}) 없음 - AiLogic.AiDataAction");
                return;
            }

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} AiData 에 설정된 어빌리티({string.Join(", ", aiData.ability_Ids)}) 타겟({aiData.target.ToString()}) Slots: <color=yellow>{string.Join(", ", aiTargetSlots.Select(slot => slot.Slot))}</color>번에 적용 - AiLogic.AiDataAction");

            // 어빌리티 적용
            AbilityLogic.Instance.SetAbility(aiData, selfSlot, aiTargetSlots);
        }
    }
}