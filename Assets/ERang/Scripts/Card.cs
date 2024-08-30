using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Analytics;

namespace ERang
{
    public class Card
    {
        /// <summary>
        /// 버프 클래스
        /// </summary>
        public class DurationAbility
        {
            public int abilityId; // 어빌리티 Id
            public AiDataType aiType; // Ai 타입
            public AbilityType abilityType; // 어빌리티 타입
            public int abilityValue; // 어빌리티 값
            public int duration; // 현재 지속 시간
            public string targetCardUid; // 대상 카드의 Uid
            public int targetBoardSlot; // 대상 보드 슬롯
        }

        public string uid;
        public int id;
        public CardType type;
        public int costMana; // 소환에 필요한 마나
        public int costGold; // 소환에 필요한 골드
        public int hp; // 체력 값
        public int maxHp; // 최대 체력 값
        public int atk; // 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int def; // 초기 방어력 값
        public bool isExtinction; // Bool 값으로 True 시 해당 카드는 사용 시 해당 전투에서 카드 덱에서 삭제된다.
        public int aiGroupId; // 해당 카드가 가지고 있는 Ai 그룹의 Id 값
        // 현재 설정된 Ai 그룹의 인덱스 값
        private int aiGroupIndex = 0;

        private List<DurationAbility> abilities = new List<DurationAbility>();

        public int BuffCount { get { return abilities.Count(a => a.aiType == AiDataType.Buff); } }
        public int DeBuffCount { get { return abilities.Count(a => a.aiType == AiDataType.DeBuff); } }
        public List<DurationAbility> Abilities { get { return abilities; } }

        public bool HasBuff { get { return BuffCount > 0; } }
        public bool HasDeBuff { get { return DeBuffCount > 0; } }

        public Card(CardData cardData)
        {
            uid = Utils.GenerateShortUniqueID();
            id = cardData.card_id;
            type = cardData.cardType;
            costMana = cardData.costMana;
            costGold = cardData.costGold;
            hp = cardData.hp;
            maxHp = hp;
            atk = cardData.atk;
            def = cardData.def;
            isExtinction = cardData.extinction;
            aiGroupId = cardData.aiGroup_id;
        }

        public Card(int hp, int maxHp, int mana, int maxMana, int atk, int def)
        {
            this.hp = hp;
            this.maxHp = maxHp;
            this.atk = atk;
            this.def = def;
        }

        /// <summary>
        /// 카드의 버프 또는 디버프를 추가한다.
        /// - 턴 종료때 리스트를 체크하고 duration을 감소 0이 되면 리스트에서 삭제
        /// </summary>
        /// <param name="aiType"></param>
        /// <param name="abilityType"></param>
        /// <param name="abilityId"></param>
        /// <param name="abilityValue"></param>
        /// <param name="duration"></param>
        /// <param name="targetCardUid"></param>
        public void AddAbilityDuration(AiDataType aiType, AbilityType abilityType, int abilityId, int abilityValue, int duration, string targetCardUid, int targetSlot)
        {
            DurationAbility durationAbility = new DurationAbility
            {
                abilityId = abilityId,
                aiType = aiType,
                abilityType = abilityType,
                abilityValue = abilityValue,
                duration = duration,
                targetCardUid = targetCardUid,
                targetBoardSlot = targetSlot,
            };

            abilities.Add(durationAbility);
            Debug.Log($"AddAbilityDuration - abilityType: {abilityType.ToString()}, abilityId: {abilityId}, abilityValue: {abilityValue}, duration: {duration}");
        }

        /// <summary>
        /// 카드의 버프 또는 디버프 확인
        /// </summary>
        /// <param name="aiType"></param>
        /// <param name="abilityId"></param>
        /// <returns></returns>
        public DurationAbility HasAbilityDuration(AiDataType aiType, int abilityId)
        {
            return abilities.Find(a => a.aiType == aiType && a.abilityId == abilityId);
        }

        public void SetHp(int hp)
        {
            this.hp = hp;
        }

        /// <summary>
        /// 카드의 체력을 증가 또는 감소시킨다.
        /// </summary>
        /// <param name="value"></param>
        public void AddHp(int value)
        {
            hp += value;

            if (hp > maxHp)
                hp = maxHp;

            if (hp <= 0)
                hp = 0;
        }

        /// <summary>
        /// 카드의 방어력을 증가 또는 감소시킨다.
        /// </summary>
        /// <param name="value"></param>
        public void AddDef(int value)
        {
            def += value;

            if (def < 0)
                def = 0;
        }

        public void AddAtk(int value)
        {
            atk += value;

            if (atk < 0)
                atk = 0;
        }

        /// <summary>
        /// 카드의 Ai 그룹을 호출하여 AiData를 가져온다.
        /// </summary>
        /// <returns></returns>
        public int GetCardAiDataId(int slot)
        {
            string aiGroupDataTableLog = $"{slot}번 슬롯 {id} 카드. <color=#78d641>AiGroupData</color> 테이블 {aiGroupId} 데이터 얻기";
            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            if (aiGroupData == null)
            {
                Debug.LogError($"{aiGroupDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - Card.GetCardAiDataId");
                return 0;
            }

            List<int> aiDataIds = new List<int>();

            // 순차적으로 AI 그룹을 호출하고, 마지막 그룹에 도달 시 최초 그룹으로 순환한다.
            if (aiGroupData.aiGroupType == AiGroupType.Repeat)
            {
                if (aiGroupIndex >= aiGroupData.ai_Groups.Count)
                    aiGroupIndex = 0;

                aiDataIds = aiGroupData.ai_Groups[aiGroupIndex];

                aiGroupIndex++;
            }
            else if (aiGroupData.aiGroupType == AiGroupType.Random)
            {
                // 나열된 AI 그룹 중 하나를 임의로 선택한다. (선택 확율은 별도 지정 없이 동일한다. n/1)
                aiDataIds = aiGroupData.ai_Groups[Random.Range(0, aiGroupData.ai_Groups.Count)];
            }

            Debug.Log($"{aiGroupDataTableLog} 성공. aiGroupType {aiGroupData.aiGroupType.ToString()} 으로 aiDataIds [{string.Join(", ", aiDataIds)}] 중 하나 뽑기 - Card.GetCardAiDataId");

            int selectedAiDataId = 0;

            if (aiDataIds.Count == 1)
            {
                selectedAiDataId = aiDataIds[0];
                Debug.Log($"{id} 카드. aiDataIds [{string.Join(", ", aiDataIds)}] 에서 {selectedAiDataId} 뽑힘 (하나만 설정되어 있음)");
            }
            else
            {
                // aiData 가중치 리스트 설정
                int totalValue = 0;
                List<(int aiDataId, int value)> aiDataList = new List<(int aiDataId, int value)>();

                foreach (int aiDataId in aiDataIds)
                {
                    string aiDataTableLog = $"<color=#78d641>AiGroupData</color> 테이블 {aiDataId} 데이터 얻기";
                    AiData aiData = AiData.GetAiData(aiDataId);

                    if (aiData == null)
                    {
                        Debug.LogWarning($"{aiDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - Card.GetCardAiDataId");
                        continue;
                    }

                    Debug.Log($"{aiDataTableLog} 성공. 가중치({(aiDataId, aiData.value)}) 추가 - Card.GetCardAiDataId");
                    totalValue += aiData.value;

                    aiDataList.Add((aiDataId, aiData.value));
                };

                string aiDataListLog = $"{id} 카드. aiDataIds {string.Join(", ", aiDataIds)} 중 중 하나 선택";

                // 가중치 리스트 중 하나 뽑기
                int randomValue = Random.Range(0, totalValue);
                int cumulativeValue = 0;

                foreach (var aiData in aiDataList)
                {
                    cumulativeValue += aiData.value;

                    if (randomValue < cumulativeValue)
                    {
                        selectedAiDataId = aiData.aiDataId;
                        Debug.Log($"{aiDataListLog} - 총 가중치 {totalValue}, randomValue({randomValue}) < cumulativeValue({cumulativeValue}) 로 {selectedAiDataId} 선택({string.Join(", ", aiDataList.Select(x => x.value))})");
                        break;
                    }
                }
            }

            return selectedAiDataId;
        }

        /// <summary>
        /// 카드의 Ai 그룹을 호출하여 AiData를 가져온다.
        /// </summary>
        /// <returns></returns>
        public AiData GetCardAiData()
        {
            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            string aiGroupDataTableLog = $"{id} 카드. <color=#78d641>AiGroupData</color> 테이블 {aiGroupId} 데이터 얻기";

            if (aiGroupData == null)
            {
                Debug.LogError($"{aiGroupDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - Card.GetCardAiData");
                return null;
            }

            List<int> aiDataIds = new List<int>();

            // 순차적으로 AI 그룹을 호출하고, 마지막 그룹에 도달 시 최초 그룹으로 순환한다.
            if (aiGroupData.aiGroupType == AiGroupType.Repeat)
            {
                // aiGroupIndex에 해당하는 AI 그룹의 AiData id 리스트를 가져온다.
                aiDataIds = aiGroupData.ai_Groups[aiGroupIndex];
                aiGroupIndex = (aiGroupIndex >= aiGroupData.ai_Groups.Count) ? 0 : aiGroupIndex + 1;
            }
            else if (aiGroupData.aiGroupType == AiGroupType.Random)
            {
                // 나열된 AI 그룹 중 하나를 임의로 선택한다. (선택 확율은 별도 지정 없이 동일한다. n/1)
                aiDataIds = aiGroupData.ai_Groups[Random.Range(0, aiGroupData.ai_Groups.Count)];
            }

            Debug.Log($"{aiGroupDataTableLog} 성공. {aiGroupData.aiGroupType.ToString()} aiDataIds({string.Join(", ", aiDataIds)}) 얻기 - Card.GetTurnStartReaction");

            string aiDataIdLog = $"{id} 카드. {string.Join(", ", aiDataIds)} 중 하나의 AiData 선택";

            AiData slectedAiData = null;

            if (aiDataIds.Count == 1)
            {
                slectedAiData = AiData.GetAiData(aiDataIds[0]);
                Debug.Log($"{aiDataIdLog} - AiData 하나만 존재");
            }
            else
            {
                int totalValue = 0;
                List<AiData> aiDataList = new List<AiData>();

                // Calculate total value and populate aiDataList
                foreach (int id in aiDataIds)
                {
                    AiData aiData = AiData.GetAiData(id);
                    aiDataList.Add(aiData);
                    totalValue += aiData.value;
                }

                // Generate a random value
                int randomValue = Random.Range(0, totalValue);
                int cumulativeValue = 0;

                // Select AiData based on random value
                foreach (var aiData in aiDataList)
                {
                    cumulativeValue += aiData.value;
                    if (randomValue < cumulativeValue)
                    {
                        return aiData;
                    }
                }
            }

            return slectedAiData;
        }

        /// <summary>
        /// 카드에 설정된 리액션 데이터 Pair 리스트 얻기
        /// - AiGroupData 테이블과 ConditionData 테이블을 참조하여 리액션 데이터를 한쌍으로 반환 (tuple 형태)
        /// </summary>
        /// <returns></returns>
        public List<(AiGroupData.Reaction, ConditionData)> GetCardReactionPairs(ConditionCheckPoint checkPoint)
        {
            List<(AiGroupData.Reaction, ConditionData)> reactionConditionPairs = new List<(AiGroupData.Reaction, ConditionData)>();

            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            string aiGroupDataTableLog = $"{id} 카드. <color=#78d641>AiGroupData</color> 테이블 {aiGroupId} 데이터 얻기";

            if (aiGroupData == null)
            {
                Debug.LogError($"{aiGroupDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - Card.GetTurnStartReaction");
                return reactionConditionPairs;
            }

            Debug.Log($"{aiGroupDataTableLog} 성공 - Card.GetTurnStartReaction");

            if (aiGroupData.reactions.Count == 0)
            {
                Debug.LogWarning($"{id} 카드. <color=#78d641>AiGroupData</color> 테이블 {aiGroupId}에 reaction 설정 없음 - Card.GetTurnStartReaction");
                return reactionConditionPairs;
            }

            foreach (var reaction in aiGroupData.reactions)
            {
                ConditionData conditionData = ConditionData.GetConditionData(reaction.conditionId);

                string conditionDataTableLog = $"{id} 카드. <color=#78d641>ConditionData</color> 테이블 {reaction.conditionId} 데이터 얻기";

                if (conditionData == null)
                {
                    Debug.LogError($"{conditionDataTableLog} 실패. <color=red>테이블 데이터 없음</color> - Card.GetTurnStartReaction");
                    continue;
                }

                Debug.Log($"{conditionDataTableLog} 성공 - Card.GetTurnStartReaction");

                // 리액션 조건이 아니면 패스
                if (conditionData.checkPoint != checkPoint)
                    continue;

                reactionConditionPairs.Add((reaction, conditionData));
            }

            // 리액션 데이터 로그
            if (reactionConditionPairs.Count > 0)
            {
                string logs = string.Join("\n", reactionConditionPairs.Select((x, index) =>
                $"[{index}] reaction: {JsonConvert.SerializeObject(x.Item1)}, condition: {JsonConvert.SerializeObject(x.Item2)}"));

                Debug.Log($"{id} 카드. {reactionConditionPairs.Count}개 리액션 컨디션({string.Join(" ,", reactionConditionPairs.Select(pair => pair.Item2.id).ToList())}) 있음");
            }

            return reactionConditionPairs;
        }

        /// <summary>
        /// AiData의 Value값을 총합하여 비중을 선정하여 하나를 선택한다.
        /// </summary>
        /// <param name="aiGroupDataIds"></param>
        /// <returns></returns>
        private AiData SelectAiDataByValue(List<int> aiGroupDataIds)
        {
            if (aiGroupDataIds.Count == 1)
            {
                return AiData.GetAiData(aiGroupDataIds[0]);
            }

            int totalValue = 0;
            List<AiData> aiDataList = new List<AiData>();

            // Calculate total value and populate aiDataList
            foreach (int id in aiGroupDataIds)
            {
                AiData aiData = AiData.GetAiData(id);
                aiDataList.Add(aiData);
                totalValue += aiData.value;
            }

            // Generate a random value
            int randomValue = Random.Range(0, totalValue);
            int cumulativeValue = 0;

            // Select AiData based on random value
            foreach (var aiData in aiDataList)
            {
                cumulativeValue += aiData.value;
                if (randomValue < cumulativeValue)
                {
                    return aiData;
                }
            }

            return null; // This should never happen if the input list is not empty
        }
    }
}