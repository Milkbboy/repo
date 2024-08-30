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
            foreach (Card.DurationAbility ability in card.Abilities)
            {
                ability.duration = ability.duration - 1;

                if (ability.duration > 0)
                    continue;

                // duration 이 0 이면 적용을 해제하거나 다른 처리를 해야 함
                switch (ability.abilityType)
                {
                    case AbilityType.AtkUp:
                        card.AddAtk(-ability.abilityValue);
                        Debug.Log($"AbilityAction. cardId: {card.id}, {ability.abilityType.ToString()} 어빌리티 적용 해제 - cardId: {card.id}, atk: {card.atk}");
                        break;

                    case AbilityType.DefUp:
                        card.AddDef(-ability.abilityValue);
                        Debug.Log($"AbilityAction. cardId: {card.id}, {ability.abilityType.ToString()} 어빌리티 적용 해제 - cardId: {card.id}, def: {card.def}");
                        break;

                    case AbilityType.BrokenDef:
                        card.AddDef(ability.abilityValue);
                        Debug.Log($"AbilityAction. cardId: {card.id}, {ability.abilityType.ToString()} 어빌리티 적용 해제 - cardId: {card.id}, def: {card.def}");
                        break;

                    case AbilityType.ChargeDamage:
                        // target 한테 데미지를 준다.
                        BoardSlot targetSlot = Board.Instance.GetBoardSlot(ability.targetBoardSlot);
                        Card targetCard = targetSlot?.Card;
                        targetCard?.AddHp(-ability.abilityValue);
                        Debug.Log($"AbilityAction. cardId: {card.id}, {ability.abilityType.ToString()} 어빌리티 적용 해제 - cardId: {card.id}, target: {targetCard?.id ?? 0}, hp: {targetCard?.hp ?? 0}");
                        break;
                    default:
                        Debug.LogWarning($"AbilityAction. cardId: {card.id}, {ability.abilityType.ToString()} 아직 구현되지 않음.");
                        break;
                }

                // 어빌리티 삭제
                card.Abilities.Remove(ability);
            }
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
            List<BoardSlot> enemyCards = Board.Instance.GetOpponentSlots(selfSlot);

            Debug.Log($"{selfSlot.Slot}번 슬롯. AiData({aiData.ai_Id})에 설정된 타겟({aiData.target.ToString()}) 얻기 시작 - AiLogic.AiDataAction");

            // AiData 에 설정된 타겟 얻기
            List<BoardSlot> aiTargetSlots = GetAiDataTargets(aiData, selfSlot, enemyCards);

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
                        Debug.LogWarning($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯에 장착된 카드 없음 - AiLogic.AiDataAction");
                        continue;
                    }

                    switch (ability.abilityType)
                    {
                        case AbilityType.Damage:
                            {
                                int beforeHp = targetSlot.Card.hp;

                                for (int j = 0; j < aiData.atk_Cnt; ++j)
                                    targetSlot.SetDamage(selfCard.atk);

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) {aiData.atk_Cnt} 회 적용. damage: {selfCard.atk}, hp: {beforeHp} => {targetSlot.Card.hp}");
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

                                Debug.Log($"{selfSlot.Slot}번 슬롯 카드({selfCard.id}). 타겟 <color=yellow>{targetSlot.Slot}</color>번 슬롯 카드({targetSlot.Card.id}) 번에 {ability.abilityType.ToString()} 어빌리티({abilityId}) 적용. 변화 def: {ability.value}, def: {beforeDef} => {targetSlot.Card.def}, duration: {ability.duration}");
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
                case AiDataTarget.NearEnemy: return TargetNearEnemy(opponentSlots);
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

        private List<BoardSlot> TargetNearEnemy(List<BoardSlot> oppentSlots)
        {
            if (oppentSlots.Count > 0)
                return new List<BoardSlot> { oppentSlots.FirstOrDefault() };

            return null;
        }

        private List<BoardSlot> TargetAllEnemy(List<BoardSlot> opponentSlots, bool exceptMaster = false)
        {
            if (exceptMaster)
                return opponentSlots.Where(x => x.Card != null && (x.Card.type != CardType.Master || x.Card.type != CardType.EnemyMaster)).ToList();

            return opponentSlots;
        }

        private List<BoardSlot> TargetRandomEnemy(List<BoardSlot> opponentSlots, bool exceptMaster = false)
        {
            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => (x.Card.type != CardType.Master || x.Card.type != CardType.EnemyMaster)).ToList();

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