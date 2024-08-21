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

        public void AiDataAction(AiData aiData, Card self)
        {
            // 상대방 카드 리스트
            List<Card> enemyCards = Board.Instance.GetOpponetCards(self);

            // AiData 에 설정된 타겟 얻기
            List<Card> aiTargetCards = GetTargets(aiData, self, enemyCards);

            if (aiTargetCards.Count == 0)
            {
                Debug.LogWarning($"AiDataAction. 타겟이 없습니다. aiData: {JsonConvert.SerializeObject(aiData)}, self: {self.id}");
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
                            target.AddAbilityDuration(aiData.type, ability.abilityData_Id, ability.value, ability.duration);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, atk: {ability.value}, target: {target.id}, atk: {target.atk}, duration: {ability.duration}");
                        }
                        break;

                    case AbilityType.DefUp:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeDef = target.def;
                            target.AddDef(ability.value);

                            // AiDataType 으로 Buff, DeBuff 구분
                            target.AddAbilityDuration(aiData.type, ability.abilityData_Id, ability.value, ability.duration);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, def: {ability.value}, target: {target.id}, def: {beforeDef} => {target.def}, duration: {ability.duration}");
                        }
                        break;

                    case AbilityType.BrokenDef:
                        foreach (Card target in aiTargetCards)
                        {
                            int beforeDef = target.def;
                            target.AddDef(-ability.value);

                            // AiDataType 으로 Buff, DeBuff 구분
                            target.AddAbilityDuration(aiData.type, ability.abilityData_Id, ability.value, ability.duration);

                            Debug.Log($"AiDataAction. 어빌리티 적용 - {ability.abilityType.ToString()} self: {self.id}, def: {ability.value}, target: {target.id}, def: {beforeDef} => {target.def}, duration: {ability.duration}");
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// // AiData 에 설정된 타겟 얻기
        /// </summary>
        /// <param name="aiData"></param>
        List<Card> GetTargets(AiData aiData, Card self, List<Card> enemyCards)
        {
            switch (aiData.target)
            {
                case AiDataTarget.Enemy: return Enemy(aiData, self, enemyCards);
                case AiDataTarget.NearEnemy: return NearEnemy(enemyCards);
                case AiDataTarget.AllEnemy: return AllEnemy(enemyCards);
                case AiDataTarget.AllEnemyCreature: return AllEnemy(enemyCards, true);
                case AiDataTarget.RandomEnemy: return RandomEnemy(enemyCards);
                case AiDataTarget.RandomEnemyCreature: return RandomEnemy(enemyCards, true);
            }

            return null;
        }

        private List<Card> Enemy(AiData aiData, Card self, List<Card> targetCards)
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

        private List<Card> NearEnemy(List<Card> enemyCards)
        {
            if (enemyCards.Count > 0)
                return new List<Card> { enemyCards.FirstOrDefault() };

            return null;
        }

        private List<Card> AllEnemy(List<Card> enemyCards, bool exceptMaster = false)
        {
            if (exceptMaster)
                return enemyCards.Where(x => x.type != CardType.Master || x.type != CardType.EnemyMaster).ToList();

            return enemyCards;
        }

        private List<Card> RandomEnemy(List<Card> enemyCards, bool exceptMaster = false)
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