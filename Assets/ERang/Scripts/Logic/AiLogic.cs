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
                        Card target = Board.Instance.GetCardByUid(ability.targetCardUid);
                        target?.AddHp(-ability.abilityValue);
                        Debug.Log($"AbilityAction. cardId: {card.id}, {ability.abilityType.ToString()} 어빌리티 적용 해제 - cardId: {card.id}, target: {target.id}, hp: {target.hp}");
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
            Card self = selfSlot.Card;

            if (self == null)
            {
                Debug.LogError($"AiDataAction. boardSlot.Card is null. boardSlot: {JsonConvert.SerializeObject(selfSlot)}");
                return;
            }

            // 상대방 카드 리스트
            List<BoardSlot> enemyCards = Board.Instance.GetOpponentSlots(selfSlot);

            // AiData 에 설정된 타겟 얻기
            List<BoardSlot> aiTargetSlots = GetAiDataTargets(aiData, selfSlot, enemyCards);

            if (aiTargetSlots.Count == 0)
            {
                Debug.LogWarning($"AiLogic.AiDataAction: 타겟 없음. aiData: {JsonConvert.SerializeObject(aiData)}, self: {self.id}");
                return;
            }

            Debug.Log($"AiDataAction. 타겟 얻기. aiData: {JsonConvert.SerializeObject(aiData)}, self: {self.id}, aiTargetSlots: {JsonConvert.SerializeObject(aiTargetSlots)}");

            foreach (int abilityId in aiData.ability_Ids)
            {
                // 이미 어빌리티가 적용 중이면 패스
                Card.DurationAbility durationAbility = self.HasAbilityDuration(aiData.type, abilityId);

                if (durationAbility != null)
                {
                    Debug.LogWarning($"AiDataAction. 카드에 이미 어빌리티가 적용 중 - cardId: {self.id}, ability: {JsonConvert.SerializeObject(durationAbility)}");
                    continue;
                }

                AbilityData ability = AbilityData.GetAbilityData(abilityId);

                Debug.Log($"AiDataAction. {ability.abilityType.ToString()} 어빌리티 적용 전. 타겟 카드: {JsonConvert.SerializeObject(aiTargetSlots)}");

                foreach (BoardSlot targetSlot in aiTargetSlots)
                {
                    if (targetSlot.Card == null)
                    {
                        Debug.LogWarning($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} targetSlot index: {targetSlot.Index} Card is null.");
                        continue;
                    }

                    Card targetCard = targetSlot.Card;

                    switch (ability.abilityType)
                    {
                        case AbilityType.Damage:
                            {
                                int beforeHp = targetCard.hp;

                                for (int j = 0; j < aiData.atk_Cnt; ++j)
                                    targetCard.AddHp(-self.atk);

                                Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()}  self: {self.id}, damage: {self.atk}, target slot index: {targetSlot.Index}, hp: {beforeHp} => {targetSlot.Card.hp}");
                            }
                            break;

                        case AbilityType.Heal:
                            {
                                int beforeHp = targetSlot.Card.hp;

                                for (int j = 0; j < aiData.atk_Cnt; ++j)
                                    targetSlot.Card.AddHp(ability.value);

                                Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, heal: {ability.value}, target slot index: {targetSlot.Index}, hp: {beforeHp} => {targetSlot.Card.hp}");
                            }
                            break;

                        case AbilityType.AtkUp:
                            {
                                int beforeAtk = targetSlot.Card.atk;
                                targetSlot.Card.AddAtk(ability.value);

                                // AiDataType 으로 Buff, DeBuff 구분
                                targetSlot.Card.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetSlot.Card.uid, targetSlot.Index);

                                Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, atk: {ability.value}, target: {targetSlot.Card.id}, atk: {beforeAtk} => {targetSlot.Card.atk}, duration: {ability.duration}");
                            }
                            break;

                        case AbilityType.DefUp:
                            {
                                int beforeDef = targetCard.def;
                                targetCard.AddDef(ability.value);

                                // AiDataType 으로 Buff, DeBuff 구분
                                targetCard.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetCard.uid, targetSlot.Index);

                                Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, def: {ability.value}, target: {targetCard.id}, def: {beforeDef} => {targetCard.def}, duration: {ability.duration}");
                            }
                            break;

                        case AbilityType.BrokenDef:
                            {
                                int beforeDef = targetCard.def;
                                targetCard.AddDef(-ability.value);

                                // AiDataType 으로 Buff, DeBuff 구분
                                targetCard.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetCard.uid, targetSlot.Index);

                                Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, def: {ability.value}, target: {targetCard.id}, def: {beforeDef} => {targetCard.def}, duration: {ability.duration}");
                            }
                            break;

                        case AbilityType.ChargeDamage:
                            // 이건 duration 이 지나면 타겟한테 데미지를 둔다. 방식이 좀 달라야 될듯
                            // target 은 Enemy 인데 발동은 자신이라서
                            foreach (BoardSlot target in aiTargetSlots)
                            {
                                self.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, targetCard.uid, targetSlot.Index);
                            }
                            break;

                        default:
                            Debug.LogWarning($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} 아직 구현되지 않음.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// AiData 에 설정된 타겟 얻기
        /// </summary>
        /// <param name="aiData"></param>
        /// <param name="self"></param>
        /// <param name="opponentSlots"></param>
        /// <returns></returns>
        List<BoardSlot> GetAiDataTargets(AiData aiData, BoardSlot self, List<BoardSlot> opponentSlots)
        {
            switch (aiData.target)
            {
                case AiDataTarget.Enemy: return TargetEnemy(aiData, self, opponentSlots);
                case AiDataTarget.NearEnemy: return TargetNearEnemy(opponentSlots);
                case AiDataTarget.AllEnemy: return TargetAllEnemy(opponentSlots);
                case AiDataTarget.AllEnemyCreature: return TargetAllEnemy(opponentSlots, true);
                case AiDataTarget.RandomEnemy: return TargetRandomEnemy(opponentSlots);
                case AiDataTarget.RandomEnemyCreature: return TargetRandomEnemy(opponentSlots, true);
            }

            return null;
        }

        private List<BoardSlot> TargetEnemy(AiData aiData, BoardSlot self, List<BoardSlot> targetCards)
        {
            List<BoardSlot> targets = new List<BoardSlot>();

            switch (aiData.type)
            {
                case AiDataType.Melee:
                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetCardIndex = attackRange - 1;

                        // 근접 공격 거리가 상대 카드 개수 보다 크면 패스
                        if (targetCardIndex >= targetCards.Count)
                            break;

                        targets.Add(targetCards[targetCardIndex]);
                    }
                    break;

                case AiDataType.Ranged:
                    if (self == null)
                    {
                        Debug.LogError($"AiLogic.Enemy: {aiData.ai_Id} - boardSlot is null. boardSlot index: {self.Index}");
                        return null;
                    }

                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetCardIndex = attackRange - (self.Index + BOARD_CENTER_OFFSET);

                        if (targetCardIndex < 0 || targetCardIndex >= targetCards.Count)
                        {
                            Debug.LogWarning($"AiLogic.Enemy: {aiData.ai_Id} - targetCardIndex is out of range. targetCardIndex: {targetCardIndex}, targetCards.Count: {targetCards.Count}");
                            continue;
                        }

                        targets.Add(targetCards[targetCardIndex]);
                    }
                    break;
            }

            return targetCards;
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
                return opponentSlots.Where(x => x.Card.type != CardType.Master || x.Card.type != CardType.EnemyMaster).ToList();

            return opponentSlots;
        }

        private List<BoardSlot> TargetRandomEnemy(List<BoardSlot> opponentSlots, bool exceptMaster = false)
        {
            if (exceptMaster)
                opponentSlots = opponentSlots.Where(x => (x.Card.type != CardType.Master || x.Card.type != CardType.EnemyMaster)).ToList();

            int randomIndex = Random.Range(0, opponentSlots.Count);

            if (randomIndex < 0 || randomIndex >= opponentSlots.Count)
            {
                Debug.LogError($"AiLogic.RandomEnemy() - randomIndex is out of range. randomIndex: {randomIndex}, opponentSlots.Count: {opponentSlots.Count}");
                return null;
            }

            return new List<BoardSlot> { opponentSlots[randomIndex] };
        }
    }
}