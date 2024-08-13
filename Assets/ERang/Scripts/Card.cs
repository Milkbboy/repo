using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class Card
    {
        public string uid;
        public int id;
        public CardType type;
        public int costMana; // 소환에 필요한 마나
        public int costGold; // 소환에 필요한 골드
        public int hp; // 체력 값
        public int atk; // 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int def; // 초기 방어력 값
        public bool isExtinction; // Bool 값으로 True 시 해당 카드는 사용 시 해당 전투에서 카드 덱에서 삭제된다.
        public int aiGroupId; // 해당 카드가 가지고 있는 Ai 그룹의 Id 값
        // 현재 설정된 Ai 그룹의 인덱스 값
        private int aiGroupIndex = 0;

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

        /*
         * 카드의 Ai 그룹을 호출하여 AiData를 가져온다.
         */
        public AiData GetCardAiData()
        {
            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);

            List<int> aiGroupDataIds = new List<int>();

            // 순차적으로 AI 그룹을 호출하고, 마지막 그룹에 도달 시 최초 그룹으로 순환한다.
            if (aiGroupData.aiGroupType == AiGroupType.Repeat)
            {
                Debug.Log($"aiGroupIndex: {aiGroupIndex}, aiGroupData.ai_Groups.Count: {aiGroupData.ai_Groups.Count}");

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

            Debug.Log($"aiGroupId: {aiGroupId}, aiGroupDataIds: {string.Join(", ", aiGroupDataIds)}");

            // AiData의 Value값을 총합하여 비중을 선정하여 하나를 선택한다.
            AiData aiData = SelectAiDataByValue(aiGroupDataIds);

            if (aiData == null)
            {
                Debug.LogError($"AiData is null. aiGroupId: {aiGroupId}");
                return null;
            }

            return aiData;
        }

        /*
         * AiData의 Value값을 총합하여 비중을 선정하여 하나를 선택한다.
         */
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