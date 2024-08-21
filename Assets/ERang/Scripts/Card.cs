using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class Card
    {
        /// <summary>
        /// 버프 클래스
        /// </summary>
        public class DurationAbility
        {
            public int aiDataId; // AiData Id
            public int abilityId; // 어빌리티 Id
            public int abilityValue; // 어빌리티 값
            public int duration; // 현재 지속 시간
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

        private List<DurationAbility> deBuffs = new List<DurationAbility>();
        private List<DurationAbility> buffs = new List<DurationAbility>();

        public int BuffCount { get { return buffs.Count; } }
        public int DeBuffCount { get { return deBuffs.Count; } }

        public bool HasBuff { get { return buffs.Count > 0; } }
        public bool HasDeBuff { get { return deBuffs.Count > 0; } }

        public Card(CardData cardData)
        {
            uid = Utils.GenerateShortUniqueID();
            id = cardData.card_id;
            type = cardData.cardType;
            costMana = cardData.costMana;
            costGold = cardData.costGold;
            hp = cardData.hp;
            atk = cardData.atk;
            def = cardData.def;
            isExtinction = cardData.extinction;
            aiGroupId = cardData.aiGroup_id;
        }

        /// <summary>
        /// 카드의 버프 또는 디버프를 추가한다.
        /// - 턴 종료때 리스트를 체크하고 duration을 감소 0이 되면 리스트에서 삭제
        /// </summary>
        /// <param name="aiType"></param>
        /// <param name="abilityId"></param>
        /// <param name="abilityValue"></param>
        /// <param name="duration"></param>
        public void AddAbilityDuration(AiDataType aiType, int abilityId, int abilityValue, int duration)
        {
            DurationAbility durationAbility = new DurationAbility
            {
                abilityId = abilityId,
                abilityValue = abilityValue,
                duration = duration,
            };

            if (aiType == AiDataType.Buff)
            {
                buffs.Add(durationAbility);
                Debug.Log($"AddAbilityDuration - Buff: {id}, abilityId: {abilityId}, abilityValue: {abilityValue}, duration: {duration}");
            }
            else if (aiType == AiDataType.DeBuff)
            {
                deBuffs.Add(durationAbility);
                Debug.Log($"AddAbilityDuration - DeBuff: {id}, abilityId: {abilityId}, abilityValue: {abilityValue}, duration: {duration}");
            }
        }

        /// <summary>
        /// 카드의 버프 또는 디버프 확인
        /// </summary>
        /// <param name="aiType"></param>
        /// <param name="abilityId"></param>
        /// <returns></returns>
        public DurationAbility HasAbilityDuration(AiDataType aiType, int abilityId)
        {
            List<DurationAbility> durationAbilities = (aiType == AiDataType.Buff) ? buffs : deBuffs;

            return durationAbilities.Find(x => x.abilityId == abilityId);
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
        public AiData GetCardAiData()
        {
            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            List<int> aiGroupDataIds = new List<int>();

            // Reaction 조건이 있으면 먼저 확인

            // 순차적으로 AI 그룹을 호출하고, 마지막 그룹에 도달 시 최초 그룹으로 순환한다.
            if (aiGroupData.aiGroupType == AiGroupType.Repeat)
            {
                // Debug.Log($"aiGroupIndex: {aiGroupIndex}, aiGroupData.ai_Groups.Count: {aiGroupData.ai_Groups.Count}");

                if (aiGroupIndex >= aiGroupData.ai_Groups.Count)
                {
                    aiGroupIndex = 0;
                }

                // aiGroupIndex에 해당하는 AI 그룹의 AiData id 리스트를 가져온다.
                aiGroupDataIds = aiGroupData.ai_Groups[aiGroupIndex];

                aiGroupIndex++;
            }
            else if (aiGroupData.aiGroupType == AiGroupType.Random)
            {
                // 나열된 AI 그룹 중 하나를 임의로 선택한다. (선택 확율은 별도 지정 없이 동일한다. n/1)
                aiGroupDataIds = aiGroupData.ai_Groups[Random.Range(0, aiGroupData.ai_Groups.Count)];
            }

            // Debug.Log($"aiGroupId: {aiGroupId}, aiGroupDataIds: {string.Join(", ", aiGroupDataIds)}");

            // AiData의 Value값을 총합하여 비중을 선정하여 하나를 선택한다.
            AiData aiData = SelectAiDataByValue(aiGroupDataIds);

            if (aiData == null)
            {
                Debug.LogError($"AiData is null. aiGroupId: {aiGroupId}");
                return null;
            }

            return aiData;
        }

        /// <summary>
        /// 턴 시작 시 체크해야 하는 컨디션 데이터들 얻기
        /// - AiGroupData 와 ConditionData 쌍으로 반환
        /// </summary>
        /// <returns></returns>
        public List<(AiGroupData.Reaction, ConditionData)> GetTurnStartReaction()
        {
            List<(AiGroupData.Reaction, ConditionData)> reactionConditionPairs = new List<(AiGroupData.Reaction, ConditionData)>();

            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            if (aiGroupData == null)
            {
                Debug.LogError($"Card: aiGroupData is null. cardId: {id}, aiGroupId: {aiGroupId}");
                return reactionConditionPairs;
            }

            if (aiGroupData.reactions.Count == 0)
            {
                Debug.Log($"Card: reactions is empty. cardId: {id}, aiGroupId: {aiGroupId}");
                return reactionConditionPairs;
            }

            foreach (var reaction in aiGroupData.reactions)
            {
                ConditionData conditionData = ConditionData.GetConditionData(reaction.conditionId);

                if (conditionData == null)
                {
                    Debug.LogError($"Card: conditionData is null. cardId: {id}, aiGroupId: {aiGroupId}, conditionId: {reaction.conditionId}");
                    continue;
                }

                // 턴시작 실행되는 리엑션이 아니면 패스
                if (conditionData.checkPoint != ConditionCheckPoint.TurnStart)
                    continue;

                reactionConditionPairs.Add((reaction, conditionData));
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