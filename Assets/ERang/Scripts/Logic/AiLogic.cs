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
        private const int BOARD_CENTER_OFFSET = 3;

        public static AiLogic Instance { get; private set; }

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
            Debug.LogWarning($"{boardSlot.Slot}번 슬롯 카드({card.id}) 어빌리티 {abilityIdsLog} - AiLogic.AbilityAction");

            foreach (Card.DurationAbility ability in card.Abilities)
            {
                ability.duration = ability.duration - 1;

                string abilityActionLog = $"{boardSlot.Slot}번 슬롯 카드({card.id}) {ability.abilityType.ToString()} 어빌리티({ability.abilityId}) 효과 지속 시간 <color=yellow>{ability.duration}</color>";

                if (ability.duration > 0)
                {
                    Debug.LogWarning($"{abilityActionLog}. 유지 - AiLogic.AbilityAction");
                    continue;
                }

                // duration 이 0 이면 적용을 해제하거나 다른 처리를 해야 함
                switch (ability.abilityType)
                {
                    case AbilityType.AtkUp:
                        card.AddAtk(-ability.abilityValue);
                        boardSlot.AddCardAtk(-ability.abilityValue);
                        Debug.Log($"{abilityActionLog} 적용 해제 - atk: {card.atk}");
                        break;
                    case AbilityType.DefUp:
                        card.AddDef(-ability.abilityValue);
                        boardSlot.AddCardDef(-ability.abilityValue);
                        Debug.Log($"{abilityActionLog} 적용 해제 - def: {card.def}");
                        break;
                    case AbilityType.BrokenDef:
                        card.AddDef(ability.abilityValue);
                        boardSlot.AddCardDef(ability.abilityValue);
                        Debug.Log($"{abilityActionLog} 적용 해제 - def: {card.def}");
                        break;
                    case AbilityType.ChargeDamage:
                        // target에 데미지를 준다.
                        BoardSlot targetSlot = Board.Instance.GetBoardSlot(ability.targetBoardSlot);
                        Card targetCard = targetSlot?.Card;
                        targetCard?.AddHp(-ability.abilityValue);
                        Debug.Log($"{abilityActionLog} 실행 - target: {targetCard?.id ?? 0}, hp: {targetCard?.hp ?? 0}");
                        break;
                    default:
                        Debug.LogWarning($"{abilityActionLog} - 아직 구현되지 않음.");
                        break;
                }
            }

            // duration 이 0 이하인 ability 제거
            card.Abilities.RemoveAll(ability => ability.duration <= 0);
        }

        public void AiDataAction(AiData aiData, BoardSlot selfSlot)
        {
            Card selfCard = selfSlot.Card;

            if (selfCard == null)
            {
                Debug.LogError($"{selfSlot.Slot}번 슬롯. 장착된 카드 없음 - AiLogic.AiDataAction");
                return;
            }

            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = Board.Instance.GetOpponentSlots(selfSlot);

            Debug.Log($"{selfSlot.Slot}번 슬롯. AiData({aiData.ai_Id})에 설정된 타겟({aiData.target.ToString()}) 얻기 시작 - AiLogic.AiDataAction");

            // AiData 에 설정된 타겟 얻기
            List<BoardSlot> aiTargetSlots = GetAiDataTargets(aiData, selfSlot, opponentSlots);

            if (aiTargetSlots.Count == 0)
            {
                Debug.LogWarning($"{selfSlot.Slot}번 슬롯. 설정 타겟({aiData.target.ToString()}) 없음 - AiLogic.AiDataAction");
                return;
            }

            Debug.Log($"{selfSlot.Slot}번 슬롯. AiData 에 설정된 어빌리티({string.Join(", ", aiData.ability_Ids)}) 타겟({aiData.target.ToString()}) Slots: <color=yellow>{string.Join(", ", aiTargetSlots.Select(slot => slot.Slot))}</color>번에 적용 - AiLogic.AiDataAction");

            foreach (int abilityId in aiData.ability_Ids)
            {
                // 이미 어빌리티가 적용 중이면 패스
                Card.DurationAbility durationAbility = selfCard.HasAbilityDuration(aiData.type, abilityId);

                if (durationAbility != null)
                {
                    Debug.LogWarning($"{selfSlot.Slot}번 슬롯. 카드({selfCard.id})에 이미 어빌리티({abilityId})가 적용 중으로 해당 어빌리티 패스 - AiLogic.AiDataAction");
                    continue;
                }

                AbilityData ability = AbilityData.GetAbilityData(abilityId);

                foreach (BoardSlot targetSlot in aiTargetSlots)
                {
                    if (targetSlot.Card == null)
                    {
                        Debug.LogWarning($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯에 장착된 카드 없어 {ability.abilityType.ToString()} 어빌리티({abilityId}) 적용 못함.  - AiLogic.AiDataAction");
                        continue;
                    }

                    switch (ability.abilityType)
                    {
                        case AbilityType.Damage:
                            {
                                int beforeHp = targetSlot.Card.hp;
                                int beforeDef = targetSlot.Card.def;

                                for (int j = 0; j < aiData.atk_Cnt; ++j)
                                    targetSlot.SetDamage(selfCard.atk);

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) {aiData.atk_Cnt} 회 적용. damage: {selfCard.atk}, hp: {beforeHp} => {targetSlot.Card.hp}, def: {beforeDef} => {targetSlot.Card.def}");
                            }
                            break;

                        case AbilityType.Heal:
                            {
                                int beforeHp = targetSlot.Card.hp;

                                for (int j = 0; j < aiData.atk_Cnt; ++j)
                                    targetSlot.AddCardHp(ability.value);

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) {aiData.atk_Cnt} 회 적용. heal: {ability.value}, hp: {beforeHp} => {targetSlot.Card.hp}");
                            }
                            break;

                        case AbilityType.AtkUp:
                            {
                                int beforeAtk = targetSlot.Card.atk;
                                targetSlot.AddCardAtk(ability.value);

                                // AiDataType 으로 Buff, DeBuff 구분
                                targetSlot.Card.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetSlot.Card.uid, targetSlot.Slot);

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) 적용. 변화 atk: {ability.value}, atk: {beforeAtk} => {targetSlot.Card.atk}, duration: {ability.duration}");
                            }
                            break;

                        case AbilityType.DefUp:
                            {
                                int beforeDef = targetSlot.Card.def;
                                targetSlot.AddCardDef(ability.value);

                                // AiDataType 으로 Buff, DeBuff 구분
                                targetSlot.Card.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetSlot.Card.uid, targetSlot.Slot);

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) 적용. 변화 def: {ability.value}, def: {beforeDef} => {targetSlot.Card.def}, duration: {ability.duration}");
                            }
                            break;

                        case AbilityType.BrokenDef:
                            {
                                int beforeDef = targetSlot.Card.def;
                                targetSlot.AddCardDef(-ability.value);

                                // AiDataType 으로 Buff, DeBuff 구분
                                targetSlot.Card.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetSlot.Card.uid, targetSlot.Slot);

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) 적용. 변화 def: {-ability.value}, def: {beforeDef} => {targetSlot.Card.def}, duration: {ability.duration}");
                            }
                            break;

                        case AbilityType.ChargeDamage:
                            // 이건 duration 이 지나면 타겟한테 데미지를 둔다. 방식이 좀 달라야 될듯
                            // target 은 Enemy 인데 발동은 자신이라서
                            foreach (BoardSlot target in aiTargetSlots)
                            {
                                selfCard.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetSlot.Card.uid, targetSlot.Slot);
                            }
                            break;

                        default:
                            Debug.LogWarning($"{ability.abilityType.ToString()} 어빌리티({abilityId}) 적용 아직 구현되지 않음.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// AiData 에 설정된 타겟 얻기
        /// - AiData 테이블 Type, Target, Atk_Range 로 타겟을 얻는다
        /// </summary>
        /// <param name="aiData"></param>
        /// <param name="self"></param>
        /// <param name="opponentSlots"></param>
        /// <returns></returns>
        List<BoardSlot> GetAiDataTargets(AiData aiData, BoardSlot self, List<BoardSlot> opponentSlots)
        {
            switch (aiData.target)
            {
                case AiDataTarget.Self: return new List<BoardSlot> { self };
                case AiDataTarget.Enemy: return TargetEnemy(aiData, self, opponentSlots);
                case AiDataTarget.NearEnemy: return TargetNearEnemy(aiData, opponentSlots);
                case AiDataTarget.AllEnemy: return TargetAllEnemy(opponentSlots);
                case AiDataTarget.AllEnemyCreature: return TargetAllEnemy(opponentSlots, true);
                case AiDataTarget.RandomEnemy: return TargetRandomEnemy(opponentSlots);
                case AiDataTarget.RandomEnemyCreature: return TargetRandomEnemy(opponentSlots, true);
            }

            return new List<BoardSlot>();
        }

        private List<BoardSlot> TargetEnemy(AiData aiData, BoardSlot self, List<BoardSlot> targetBoardSlots)
        {
            List<BoardSlot> targets = new List<BoardSlot>();

            switch (aiData.type)
            {
                case AiDataType.Melee:
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetSlotIndex = attackRange - 1;

                        // 근접 공격 거리가 상대 카드 개수 보다 크면 패스
                        if (targetSlotIndex < 0 || targetSlotIndex >= targetBoardSlots.Count)
                            break;

                        targets.Add(targetBoardSlots[targetSlotIndex]);
                    }
                    break;

                case AiDataType.Ranged:
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetSlotIndex = attackRange - (self.Index + BOARD_CENTER_OFFSET);

                        if (targetSlotIndex < 0 || targetSlotIndex >= targetBoardSlots.Count)
                        {
                            Debug.LogWarning($"{aiData.ai_Id} - targetSlotIndex is out of range. targetSlotIndex: {targetSlotIndex}, targetBoardSlots.Count: {targetBoardSlots.Count} - AiLogic.TargetEnemy");
                            continue;
                        }

                        targets.Add(targetBoardSlots[targetSlotIndex]);
                    }
                    break;

                case AiDataType.Explosion:
                    Debug.LogWarning($"{aiData.ai_Id} - AiDataType.Explosion 아직 구현되지 않음 - AiLogic.TargetEnemy");
                    break;
            }

            return targets;
        }

        /// <summary>
        /// 카드가 장착된 첫번째 카드를 타겟으로 설정
        /// </summary>
        /// <param name="oppentSlots"></param>
        /// <returns></returns>
        private List<BoardSlot> TargetNearEnemy(AiData aiData, List<BoardSlot> oppentSlots)
        {
            List<BoardSlot> targets = new List<BoardSlot>();

            // 제일 근접한 타겟 찾기
            BoardSlot targetSlot = oppentSlots.FirstOrDefault(x => x.Card != null);
            int targetIndex = targetSlot.Index;

            Debug.Log($"{aiData.ai_Id} - 제일 근접한 타겟 슬롯 인덱스 {targetIndex} 찾고 attackRanges({(aiData.attackRanges.Count > 0 ? string.Join(", ", aiData.attackRanges) : "없음")}) 에 설정된 타겟 찾기 - AiLogic.TargetNearEnemy");

            if (aiData.attackRanges.Count == 0)
            {
                Debug.LogWarning($"{aiData.ai_Id} - attackRanges 가 설정되지 않아서 제일 근접한 타겟만 찾음 - AiLogic.TargetNearEnemy");
                targets.Add(targetSlot);
                return targets;
            }

            for (int i = 0; i < aiData.attackRanges.Count; ++i)
            {
                int attackRange = aiData.attackRanges[i];
                int targetSlotIndex = targetSlot.Index + (attackRange - 1);

                if (targetSlotIndex < 0 || targetSlotIndex >= oppentSlots.Count)
                {
                    Debug.LogWarning($"{aiData.ai_Id} - {i}번째 타겟 슬롯 인덱스 {targetSlotIndex} 로 패스 (적용 범위 0 ~ 3) - AiLogic.TargetNearEnemy");
                    continue;
                }
                else
                {
                    Debug.Log($"{aiData.ai_Id} - {i}번째 타겟 슬롯 인덱스 {targetSlotIndex} 찾기 - AiLogic.TargetNearEnemy");
                }

                targets.Add(oppentSlots[targetSlotIndex]);
            }

            return targets;
        }

        private List<BoardSlot> TargetAllEnemy(List<BoardSlot> opponentSlots, bool exceptMaster = false)
        {
            if (exceptMaster)
                return opponentSlots.Where(x => x.CardType != CardType.Master || x.CardType != CardType.EnemyMaster).ToList();

            return opponentSlots;
        }

        private List<BoardSlot> TargetRandomEnemy(List<BoardSlot> opponentSlots, bool exceptMaster = false)
        {
            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => x.CardType != CardType.Master || x.CardType != CardType.EnemyMaster).ToList();

            int randomIndex = Random.Range(0, opponentSlots.Count);

            if (randomIndex < 0 || randomIndex >= opponentSlots.Count)
            {
                Debug.LogError($"randomIndex is out of range. randomIndex: {randomIndex}, opponentSlots.Count: {opponentSlots.Count} - AiLogic.TargetRandomEnemy");
                return null;
            }

            return new List<BoardSlot> { opponentSlots[randomIndex] };
        }
    }
}