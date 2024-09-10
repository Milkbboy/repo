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
            public AbilityWhereFrom whereFrom; // 어빌리티 적용 위치
            public AiDataType aiType; // Ai 타입
            public AbilityType abilityType; // 어빌리티 타입
            public AbilityWorkType abilityWorkType; // 어빌리티 작업 타입(HandOn 찾기 위함)
            public int beforeValue; // 어빌리티 적용 전 값
            public int abilityValue; // 어빌리티 값
            public int duration; // 현재 지속 시간
            public int actorBoardSlot; // 어빌리티 발동 보드 슬롯
            public int ownerCardId; // 어빌리티 소유자 카드 Id
            public int ownerBoardSlot; // 어빌리티 소유자 보드 슬롯
            public string ownerCardUid; // 어빌리티 소유자 카드 Uid
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

        public List<Ability> GetHandOnAbilities()
        {
            return abilities.FindAll(ability => ability.whereFrom == AbilityWhereFrom.TurnStarHandOn);
        }

        /// <summary>
        /// 카드 버프 개수 얻기
        /// </summary>
        public int GetBuffCount(string cardUid)
        {
            return abilities.Count(ability => ability.ownerCardUid == cardUid && ability.aiType == AiDataType.Buff);
        }

        /// <summary>
        /// 카드 디버프 개수 얻기
        /// </summary>
        public int GetDebuffCount(string cardUid)
        {
            return abilities.Count(ability => ability.ownerCardUid == cardUid && ability.aiType == AiDataType.Debuff);
        }

        /// <summary>
        /// 카드 어빌리티 제거 - card uid
        /// </summary>
        public void RemoveAbility(string cardUid)
        {
            abilities.RemoveAll(ability => ability.ownerCardUid == cardUid);
        }

        /// <summary>
        /// 핸드 온 어빌리티 동작
        /// </summary>
        public void HandOnAbilityAction(AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots, List<AbilityData> abilityDatas)
        {
            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 핸드 온 어빌리티({Utils.NumbersText(aiData.ability_Ids)}) 적용 - AbilityLogic.HandOnAbilityAction");

            foreach (AbilityData abilityData in abilityDatas)
            {
                foreach (BoardSlot targetSlot in targetSlots)
                {
                    if (targetSlot.Card == null)
                    {
                        // Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 타겟 슬롯 {Utils.BoardSlotLog(targetSlot)}가 없어 어빌리티 적용 패스 - AbilityLogic.AbilityAffect");
                        continue;
                    }

                    abilities.Add(MakeAbility(abilityData, aiData, selfSlot, targetSlot, AbilityWhereFrom.TurnStarHandOn));
                }

                switch (abilityData.abilityType)
                {
                    case AbilityType.Damage: BoardLogic.Instance.AbilityDamage(aiData, selfSlot, targetSlots); break;
                    case AbilityType.Heal: StartCoroutine(BoardLogic.Instance.AbilityHp(targetSlots, abilityData.value)); break;
                    case AbilityType.AtkUp: StartCoroutine(BoardLogic.Instance.AbilityAtk(targetSlots, abilityData.value)); break;
                    case AbilityType.DefUp: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, abilityData.value)); break;
                    case AbilityType.BrokenDef: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, -abilityData.value)); break;
                    case AbilityType.ChargeDamage: StartCoroutine(BoardLogic.Instance.AbilityChargeDamage(aiData, selfSlot, targetSlots, abilityData.value)); break;
                    case AbilityType.AddMana: StartCoroutine(BoardLogic.Instance.AbilityAddMana(selfSlot, abilityData.value)); break;
                    case AbilityType.AddGoldPer: BoardLogic.Instance.AbilityAddGoldPer(aiData, abilityData, selfSlot); break;
                }
            }
        }

        /// <summary>
        /// 핸드 온 어빌리티 해제
        /// </summary>
        public void HandOnAbilityCut(Ability ability)
        {
            BoardSlot targetSlot = Board.Instance.GetBoardSlot(ability.ownerBoardSlot);

            if (targetSlot.Card == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(targetSlot)} 어빌리티 대상이 없어 어빌리티 적용 해제 패스 - AbilityLogic.HandOnAbilityCut");
                return;
            }

            AbilityAction(targetSlot, ability);
        }

        public void HandUseAbilityAction(AbilityWhereFrom whereFrom, AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            if (selfSlot.Card == null)
            {
                Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} 장착된 카드 없음 - AbilityLogic.HandUseAbilityAction");
                return;
            }

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 어빌리티({Utils.NumbersText(aiData.ability_Ids)}) 적용 - AbilityLogic.HandUseAbilityAction");

            foreach (int abilityId in aiData.ability_Ids)
            {
                // 이미 어빌리티가 적용 중이면 패스
                Ability durationAbility = abilities.Find(a => a.abilityId == abilityId);

                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 이미 {durationAbility.abilityType} 어빌리티({abilityId})가 적용 중으로 해당 어빌리티 패스 - AbilityLogic.HandUseAbilityAction");
                    continue;
                }

                AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

                if (abilityData == null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} <color=red>어빌리티 데이터 없음</color> - AbilityLogic.HandUseAbilityAction");
                    continue;
                }

                // duration 이 있는 어빌리티는 상태 해제를 위해 어빌리티를 저장
                if (abilityData.duration > 0)
                {
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                        {
                            Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 타겟 슬롯 {Utils.BoardSlotLog(targetSlot)}가 없어 어빌리티 적용 패스 - AbilityLogic.HandUseAbilityAction");
                            continue;
                        }

                        abilities.Add(MakeAbility(abilityData, aiData, selfSlot, targetSlot, whereFrom));
                    }
                }

                switch (abilityData.abilityType)
                {
                    case AbilityType.Damage: BoardLogic.Instance.AbilityDamage(aiData, selfSlot, targetSlots, abilityData.value); break;
                    case AbilityType.Heal: StartCoroutine(BoardLogic.Instance.AbilityHp(targetSlots, abilityData.value)); break;
                    case AbilityType.AtkUp: StartCoroutine(BoardLogic.Instance.AbilityAtk(targetSlots, abilityData.value)); break;
                    case AbilityType.DefUp: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, abilityData.value)); break;
                    case AbilityType.BrokenDef: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, -abilityData.value)); break;
                    case AbilityType.ChargeDamage: StartCoroutine(BoardLogic.Instance.AbilityChargeDamage(aiData, selfSlot, targetSlots, abilityData.value)); break;
                    case AbilityType.AddMana: StartCoroutine(BoardLogic.Instance.AbilityAddMana(selfSlot, abilityData.value)); break;
                    case AbilityType.AddGoldPer: BoardLogic.Instance.AbilityAddGoldPer(aiData, abilityData, selfSlot); break;
                }
            }
        }

        /// <summary>
        /// aiData 에 설정된 어빌리티들 적용
        /// </summary>
        public void SetBoardSlotAbility(AbilityWhereFrom whereFrom, AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
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
                Ability durationAbility = abilities.Find(a => a.abilityId == abilityId);

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
                if (abilityData.duration > 0)
                {
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                        {
                            Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 타겟 슬롯 {Utils.BoardSlotLog(targetSlot)}가 없어 어빌리티 적용 패스 - AbilityLogic.AbilityAffect");
                            continue;
                        }

                        abilities.Add(MakeAbility(abilityData, aiData, selfSlot, targetSlot, whereFrom));
                    }
                }

                switch (abilityData.abilityType)
                {
                    case AbilityType.Damage: BoardLogic.Instance.AbilityDamage(aiData, selfSlot, targetSlots); break;
                    case AbilityType.Heal: StartCoroutine(BoardLogic.Instance.AbilityHp(targetSlots, abilityData.value)); break;
                    case AbilityType.AtkUp: StartCoroutine(BoardLogic.Instance.AbilityAtk(targetSlots, abilityData.value)); break;
                    case AbilityType.DefUp: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, abilityData.value)); break;
                    case AbilityType.BrokenDef: StartCoroutine(BoardLogic.Instance.AbilityDef(targetSlots, -abilityData.value)); break;
                    case AbilityType.ChargeDamage: StartCoroutine(BoardLogic.Instance.AbilityChargeDamage(aiData, selfSlot, targetSlots, abilityData.value)); break;
                    case AbilityType.AddMana: StartCoroutine(BoardLogic.Instance.AbilityAddMana(selfSlot, abilityData.value)); break;
                    case AbilityType.AddGoldPer: BoardLogic.Instance.AbilityAddGoldPer(aiData, abilityData, selfSlot); break;
                }
            }
        }

        public List<Ability> GetDurationAbilities()
        {
            // 어빌리티 대상이 사라진 보드 슬롯
            List<BoardSlot> removeCardSlots = abilities.Select(ability => Board.Instance.GetBoardSlot(ability.ownerBoardSlot)).Where(boardSlot => boardSlot.Card == null).ToList();
            if (removeCardSlots.Count > 0)
                Debug.Log($"어빌리티 대상이 사라진 보드 슬롯: {string.Join(", ", removeCardSlots.Select(boardSlot => boardSlot.Slot))}");

            // 어빌리티 대상이 사라진 보드 슬롯 제거
            abilities.RemoveAll(ability => Board.Instance.GetBoardSlot(ability.ownerBoardSlot).Card == null);

            return abilities;
        }

        /// <summary>
        /// 보드 슬롯에 장착된 카드 ability 적용
        /// </summary>
        public void AbilityAction(BoardSlot boardSlot, Ability ability)
        {
            string abilityActionLog = $"{Utils.BoardSlotLog(boardSlot)} {ability.abilityType} 어빌리티({ability.abilityId}) 효과 지속 시간(<color=yellow>{ability.duration}</color>)";

            // 어빌리티는 카드에 적용
            Card card = boardSlot.Card;

            // 버프, 디버프는 적용 해제, 차징 공격은 타겟 슬롯에 데미지 적용
            switch (ability.abilityType)
            {
                case AbilityType.AtkUp:
                    {
                        int beforeAtk = card.atk;
                        boardSlot.AddCardAtk(-ability.abilityValue);
                        Debug.Log($"{abilityActionLog} 적용 해제 - atk: {beforeAtk} => {card.atk}");
                    }
                    break;
                case AbilityType.DefUp:
                    boardSlot.AddCardDef(-ability.abilityValue);
                    Debug.Log($"{abilityActionLog} 적용 해제 - def: {card.def}");
                    break;
                case AbilityType.BrokenDef:
                    boardSlot.AddCardDef(ability.abilityValue);
                    Debug.Log($"{abilityActionLog} 적용 해제 - beforeValue: {ability.beforeValue} def: {card.def}");
                    break;
                case AbilityType.ChargeDamage:
                    BoardSlot targetSlot = Board.Instance.GetBoardSlot(ability.ownerBoardSlot);
                    targetSlot.SetDamage(ability.abilityValue);
                    string chargeDamageLog = targetSlot.Card != null ? $"데미지 {ability.abilityValue} 적용" : "적용 해제";
                    Debug.Log($"{abilityActionLog} {chargeDamageLog} - AiLogic.AbilityAction");
                    break;
                case AbilityType.AddMana:
                    Master.Instance.DecreaseMana(ability.abilityValue);
                    boardSlot.SetMasterMana(Master.Instance.Mana);
                    Debug.Log($"{abilityActionLog} 적용 해제 - <color=#257dca>mana: {Master.Instance.Mana}</color>");
                    break;
                default:
                    Debug.LogWarning($"{abilityActionLog} - 아직 구현되지 않음.");
                    break;
            }
        }

        public void ClearHandOnAbilities()
        {
            var removedAbilities = abilities.Where(ability => ability.whereFrom == AbilityWhereFrom.TurnStarHandOn).ToList();
            foreach (var ability in removedAbilities)
            {
                Debug.Log($"HandOn 어빌리티 제거 - 타입: {ability.abilityType}, 값: {ability.abilityValue}");
            }

            abilities.RemoveAll(ability => ability.whereFrom == AbilityWhereFrom.TurnStarHandOn);
        }

        public void ClearDurationAbilities()
        {
            var removedAbilities = abilities.Where(ability => ability.duration <= 0).ToList();
            foreach (var ability in removedAbilities)
            {
                Debug.Log($"Duration 어빌리티 제거 - 타입: {ability.abilityType}, 값: {ability.abilityValue}");
            }

            abilities.RemoveAll(ability => ability.duration <= 0);
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

        private Ability MakeAbility(AbilityData abilityData, AiData aiData, BoardSlot selfSlot, BoardSlot targetSlot, AbilityWhereFrom whereFrom)
        {
            Debug.Log($"어빌리티 추가. beforeValue: {GetOriginStatValue(abilityData.abilityType, targetSlot)}, abilityValue: {abilityData.value}");
            return new Ability
            {
                abilityId = abilityData.abilityData_Id,
                whereFrom = whereFrom,
                aiType = aiData.type,
                abilityType = abilityData.abilityType,
                abilityWorkType = abilityData.type,
                beforeValue = GetOriginStatValue(abilityData.abilityType, targetSlot),
                abilityValue = abilityData.value,
                duration = abilityData.duration,
                actorBoardSlot = selfSlot.Slot,
                ownerCardId = targetSlot.Card.id,
                ownerBoardSlot = targetSlot.Slot,
                ownerCardUid = targetSlot.Card.uid
            };
        }
    }
}