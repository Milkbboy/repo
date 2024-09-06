using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

        public class Ability
        {
            public int abilityId; // 어빌리티 Id
            public AiDataType aiType; // Ai 타입
            public AbilityType abilityType; // 어빌리티 타입
            public AbilityWorkType abilityWorkType; // 어빌리티 작업 타입(HandOn 찾기 위함)
            public int beforeValue; // 어빌리티 적용 전 값
            public int abilityValue; // 어빌리티 값
            public int duration; // 현재 지속 시간
            public string targetCardUid; // 대상 카드의 Uid
            public int selfBoardSlot; // 어빌리티 발동 보드 슬롯
            public int targetBoardSlot; // 어빌리티 대상 보드 슬롯
        }

        public List<Ability> abilities = new List<Ability>();

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

        public void SetBoardSlotAbility(AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
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
                Card.DurationAbility durationAbility = selfSlot.Card.FindDurationAbility(abilityId);

                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 이미 {durationAbility.abilityType} 어빌리티({abilityId})가 적용 중으로 해당 어빌리티 패스 - AbilityLogic.AbilityAction");
                    continue;
                }

                AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

                if (abilityData == null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} <color=red>어빌리티 데이터 없음</color> - AbilityLogic.AbilityAction");
                    continue;
                }

                // duration 이 있는 어빌리티는 상태 해제를 위해 어빌리티를 저장
                if ((aiData.type == AiDataType.Buff || aiData.type == AiDataType.DeBuff) && abilityData.duration > 0)
                {
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                        {
                            Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 타겟 슬롯 {Utils.BoardSlotLog(targetSlot)}가 없어 어빌리티 적용 패스 - AbilityLogic.AbilityAffect");
                            continue;
                        }

                        abilities.Add(new Ability
                        {
                            abilityId = abilityData.abilityData_Id,
                            aiType = aiData.type,
                            abilityType = abilityData.abilityType,
                            abilityWorkType = abilityData.type,
                            beforeValue = GetOriginStatValue(abilityData.abilityType, targetSlot),
                            abilityValue = abilityData.value,
                            duration = abilityData.duration,
                            targetCardUid = targetSlot.Card.uid,
                            selfBoardSlot = selfSlot.Slot,
                            targetBoardSlot = targetSlot.Slot
                        });
                    }
                }

                switch (abilityData.abilityType)
                {
                    case AbilityType.Damage: BoardLogic.Instance.AbilityDamage(aiData, selfSlot, targetSlots); break;
                    case AbilityType.Heal: StartCoroutine(BoardLogic.Instance.AbilityHp(targetSlots, abilityData.value)); break;
                    case AbilityType.AtkUp: StartCoroutine(BoardLogic.Instance.AbilityAtk(targetSlots, abilityData.value)); break;
                    case AbilityType.DefUp: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, abilityData.value)); break;
                    case AbilityType.BrokenDef: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, -abilityData.value)); break;
                    case AbilityType.ChargeDamage: StartCoroutine(BoardLogic.Instance.AbilityChargeDamage(selfSlot)); break;
                    case AbilityType.AddMana: StartCoroutine(BoardLogic.Instance.AbilityAddMana(selfSlot, abilityData.value)); break;
                    case AbilityType.AddGoldPer: BoardLogic.Instance.AbilityAddGoldPer(aiData, abilityData, selfSlot); break;
                }
            }
        }

        public List<Ability> GetDurationAbilities()
        {
            // 어빌리티 대상이 사라진 보드 슬롯
            List<BoardSlot> removeCardSlots = abilities.Select(ability => Board.Instance.GetBoardSlot(ability.targetBoardSlot)).Where(boardSlot => boardSlot.Card == null).ToList();
            Debug.Log($"어빌리티 대상이 사라진 보드 슬롯: {string.Join(", ", removeCardSlots.Select(boardSlot => boardSlot.Slot))}");

            // 어빌리티 대상이 사라진 보드 슬롯 제거
            abilities.RemoveAll(ability => Board.Instance.GetBoardSlot(ability.targetBoardSlot).Card == null);

            return abilities;
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

        /// <summary>
        /// 어빌리티 타입에 따른 원래 stat 얻기
        /// </summary>
        private int GetOriginStatValue(AbilityType abilityType, BoardSlot boardSlot)
        {
            int originValue = 0;

            switch (abilityType)
            {
                case AbilityType.AtkUp:
                    originValue = boardSlot.Card.atk;
                    break;
                case AbilityType.DefUp:
                case AbilityType.BrokenDef:
                    originValue = boardSlot.Card.def;
                    break;
                case AbilityType.Heal:
                    originValue = boardSlot.Card.hp;
                    break;
            }

            return originValue;
        }
    }
}