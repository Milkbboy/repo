using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

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

        public void SetAbility(AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            if (selfSlot.Card == null)
            {
                Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} 장착된 카드 없음 - AbilityLogic.AbilityAction");
                return;
            }

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 어빌리티({Utils.NumbersText(aiData.ability_Ids)}) 적용 - AbilityLogic.AbilityAction");

            foreach (int abilityId in aiData.ability_Ids)
            {
                // 이미 어빌리티가 적용 중이면 패스
                Card.DurationAbility durationAbility = selfSlot.Card.HasAbilityDuration(abilityId);

                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 이미 {durationAbility.abilityType} 어빌리티({abilityId})가 적용 중으로 해당 어빌리티 패스 - AbilityLogic.AbilityAction");
                    continue;
                }

                AbilityData ability = AbilityData.GetAbilityData(abilityId);

                if (ability == null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} <color=red>어빌리티 데이터 없음</color> - AbilityLogic.AbilityAction");
                    continue;
                }

                switch (ability.abilityType)
                {
                    case AbilityType.Damage:
                        BoardLogic.Instance.AbilityDamage(aiData, selfSlot, targetSlots);
                        break;
                    case AbilityType.Heal:
                    case AbilityType.AtkUp:
                    case AbilityType.DefUp:
                    case AbilityType.BrokenDef:
                    case AbilityType.ChargeDamage:
                    case AbilityType.AddMana:
                        BoardLogic.Instance.AbilityAffect(aiData, ability, selfSlot, targetSlots);
                        break;
                    case AbilityType.AddGoldPer:
                        BoardLogic.Instance.AbilityAddGoldPer(aiData, ability, selfSlot);
                        break;
                }
            }
        }

        /// <summary>
        /// 보드 슬롯에 장착된 카드 ability 적용
        /// </summary>
        public void DurationAbilityAction(BoardSlot boardSlot)
        {
            Card card = boardSlot.Card;

            string abilityIdsLog = card.Abilities.Count > 0 ? $"{string.Join(", ", card.Abilities.Select(ability => ability.abilityId).ToList())} 확인" : "없음";
            Debug.Log($"{Utils.BoardSlotLog(boardSlot)} 어빌리티 {abilityIdsLog} - AiLogic.AbilityAction");

            foreach (Card.DurationAbility ability in card.Abilities)
            {
                ability.duration = ability.duration - 1;

                string abilityActionLog = $"{Utils.BoardSlotLog(boardSlot)} {ability.abilityType} 어빌리티({ability.abilityId}) 효과 지속 시간(<color=yellow>{ability.duration}</color>)";

                // 지속 시간이 남아 있으면 패스
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
                        BoardSlot targetSlot = Board.Instance.GetBoardSlot(ability.targetBoardSlot);
                        targetSlot.SetDamage(ability.abilityValue);
                        string chargeDamageLog = targetSlot.Card != null ? $"데미지 {ability.abilityValue} 적용" : "적용 해제";
                        Debug.Log($"{abilityActionLog} {chargeDamageLog} - AiLogic.AbilityAction");
                        break;
                    case AbilityType.AddMana:
                        Master.Instance.DecreaseMana(ability.abilityValue);
                        boardSlot.SetMasterMana(Master.Instance.Mana);
                        Debug.Log($"{abilityActionLog} 적용 해제 - mana: {Master.Instance.Mana}");
                        break;
                    default:
                        Debug.LogWarning($"{abilityActionLog} - 아직 구현되지 않음.");
                        break;
                }
            }

            // duration 이 0 이하인 ability 제거
            card.Abilities.RemoveAll(ability => ability.duration <= 0);
        }
    }
}