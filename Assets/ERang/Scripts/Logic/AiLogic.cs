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

        public void AiDataAction(AiData aiData, Card self)
        {
            // Debug.Log($"AiDataAction. aiData: {JsonConvert.SerializeObject(aiData)}, cardId: {self.id}");

            // 상대방 카드 리스트
            List<Card> enemyCards = Board.Instance.GetOpponetCards(self);

            // Debug.Log($"AiDataAction. 상대방 카드 리스트: {JsonConvert.SerializeObject(enemyCards)}");

            // AiData 에 설정된 타겟 얻기
            List<Card> aiTargetCards = GetAiDataTargets(aiData, self, enemyCards);

            // Debug.Log($"AiDataAction. 타겟 얻기: {JsonConvert.SerializeObject(aiTargetCards)}");

            if (aiTargetCards.Count == 0)
            {
                Debug.LogWarning($"AiLogic.AiDataAction: 타겟 없음. aiData: {JsonConvert.SerializeObject(aiData)}, self: {self.id}");
                return;
            }

            Debug.Log($"AiDataAction. 타겟 얻기. aiData: {JsonConvert.SerializeObject(aiData)}, self: {self.id}, aiTargetCards: {JsonConvert.SerializeObject(aiTargetCards)}");

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

                Debug.Log($"AiDataAction. {ability.abilityType.ToString()} 어빌리티 적용 전. 타겟 카드: {JsonConvert.SerializeObject(aiTargetCards)}");

                // AbilityType > Damage 타입에는 데미지 값을 넣지 않을게.카드 데이터에 있는게 맞아 2024-08-12
                switch (ability.abilityType)
                {
                    case AbilityType.Damage:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeHp = target.hp;

                            for (int j = 0; j < aiData.atk_Cnt; ++j)
                                target.AddHp(-self.atk);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()}  self: {self.id}, damage: {self.atk}, target: {target.id}, hp: {beforeHp} => {target.hp}");
                        }
                        break;

                    case AbilityType.Heal:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeHp = target.hp;

                            for (int j = 0; j < aiData.atk_Cnt; ++j)
                                target.AddHp(ability.value);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, heal: {ability.value}, target: {target.id}, hp: {beforeHp} => {target.hp}");
                        }
                        break;

                    case AbilityType.AtkUp:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeAtk = target.atk;
                            target.AddAtk(ability.value);

                            // AiDataType 으로 Buff, DeBuff 구분
                            target.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, target.uid);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, atk: {ability.value}, target: {target.id}, atk: {beforeAtk} => {target.atk}, duration: {ability.duration}");
                        }
                        break;

                    case AbilityType.DefUp:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeDef = target.def;
                            target.AddDef(ability.value);

                            // AiDataType 으로 Buff, DeBuff 구분
                            target.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, target.uid);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, def: {ability.value}, target: {target.id}, def: {beforeDef} => {target.def}, duration: {ability.duration}");
                        }
                        break;

                    case AbilityType.BrokenDef:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeDef = target.def;
                            target.AddDef(-ability.value);

                            // AiDataType 으로 Buff, DeBuff 구분
                            target.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, target.uid);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, def: {ability.value}, target: {target.id}, def: {beforeDef} => {target.def}, duration: {ability.duration}");
                        }
                        break;

                    case AbilityType.ChargeDamage:
                        // 이건 duration 이 지나면 타겟한테 데미지를 둔다. 방식이 좀 달라야 될듯
                        // target 은 Enemy 인데 발동은 자신이라서
                        foreach (Card target in aiTargetCards)
                        {
                            self.AddAbilityDuration(aiData.type, ability.abilityType, ability.abilityData_Id, ability.value, ability.duration, target.uid);
                        }
                        break;

                    default:
                        Debug.LogWarning($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} 아직 구현되지 않음.");
                        break;
                }
            }
        }

        /// <summary>
        /// // AiData 에 설정된 타겟 얻기
        /// </summary>
        /// <param name="aiData"></param>
        List<Card> GetAiDataTargets(AiData aiData, Card self, List<Card> enemyCards)
        {
            switch (aiData.target)
            {
                case AiDataTarget.Enemy: return TargetEnemy(aiData, self, enemyCards);
                case AiDataTarget.NearEnemy: return TargetNearEnemy(enemyCards);
                case AiDataTarget.AllEnemy: return TargetAllEnemy(enemyCards);
                case AiDataTarget.AllEnemyCreature: return TargetAllEnemy(enemyCards, true);
                case AiDataTarget.RandomEnemy: return TargetRandomEnemy(enemyCards);
                case AiDataTarget.RandomEnemyCreature: return TargetRandomEnemy(enemyCards, true);
            }

            return null;
        }

        private List<Card> TargetEnemy(AiData aiData, Card self, List<Card> targetCards)
        {
            List<Card> targets = new List<Card>();

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
                    BoardSlot boardSlot = Board.Instance.FindSlotByCardUid(self.uid);

                    if (boardSlot == null)
                    {
                        Debug.LogError($"AiLogic.Enemy: {aiData.ai_Id} - boardSlot is null. self.id: {self.id}");
                        return null;
                    }

                    foreach (var attackRange in aiData.attackRanges)
                    {
                        int targetCardIndex = attackRange - (boardSlot.Index + BOARD_CENTER_OFFSET);

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

        private List<Card> TargetNearEnemy(List<Card> enemyCards)
        {
            if (enemyCards.Count > 0)
                return new List<Card> { enemyCards.FirstOrDefault() };

            return null;
        }

        private List<Card> TargetAllEnemy(List<Card> enemyCards, bool exceptMaster = false)
        {
            if (exceptMaster)
                return enemyCards.Where(x => x.type != CardType.Master || x.type != CardType.EnemyMaster).ToList();

            return enemyCards;
        }

        private List<Card> TargetRandomEnemy(List<Card> enemyCards, bool exceptMaster = false)
        {
            if (exceptMaster)
                enemyCards = enemyCards.Where(x => (x.type != CardType.Master || x.type != CardType.EnemyMaster)).ToList();

            int randomIndex = Random.Range(0, enemyCards.Count);

            if (randomIndex < 0 || randomIndex >= enemyCards.Count)
            {
                Debug.LogError($"AiLogic.RandomEnemy() - randomIndex is out of range. randomIndex: {randomIndex}, enemyCards.Count: {enemyCards.Count}");
                return null;
            }

            return new List<Card> { enemyCards[randomIndex] };
        }
    }
}