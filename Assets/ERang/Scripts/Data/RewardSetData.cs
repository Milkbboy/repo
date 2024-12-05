using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class RewardSetData
    {
        public string grade;
        public string nameDesc;
        public int value;
        public CardGrade cardGrade;

        public static List<RewardSetData> rewardSetDatas = new();
        public static Dictionary<string, RewardSetData> rewardSetDataDict = new();

        private static int sumValue = 0;

        public void Initialize(RewardSetDataEntity entity)
        {
            grade = entity.CardGrade;
            nameDesc = entity.NameDesc;
            value = entity.Value;
            cardGrade = ConvertCardGrade(entity.CardGrade);
        }

        public static void Load(string path = "")
        {
            RewardSetDataTable rewardSetDataTable = Resources.Load<RewardSetDataTable>(path);

            if (rewardSetDataTable == null)
            {
                Debug.LogError("RewardSetDataTable is not found");
                return;
            }

            foreach (var rewardSetDataEntity in rewardSetDataTable.items)
            {
                if (rewardSetDataDict.ContainsKey(rewardSetDataEntity.CardGrade))
                    continue;

                RewardSetData rewardSetData = new();
                rewardSetData.Initialize(rewardSetDataEntity);

                sumValue += rewardSetData.value;

                rewardSetDatas.Add(rewardSetData);
                rewardSetDataDict.Add(rewardSetData.grade, rewardSetData);
            }
        }

        public static RewardSetData GetRewardSetData(string grade)
        {
            if (rewardSetDataDict.ContainsKey(grade))
                return rewardSetDataDict[grade];

            Debug.LogError($"RewardSetData is not found: {grade}");

            return null;
        }

        public static RewardSetData GetRewardSetData(CardGrade cardGrade)
        {
            foreach (var rewardSetData in rewardSetDatas)
            {
                if (rewardSetData.cardGrade == cardGrade)
                    return rewardSetData;
            }

            Debug.LogError($"RewardSetData is not found: {cardGrade}");

            return null;
        }

        public static CardGrade PickupCardGrade()
        {
            int randomValue = Random.Range(0, sumValue);
            int tempValue = 0;

            foreach (var rewardSetData in rewardSetDatas)
            {
                tempValue += rewardSetData.value;

                if (randomValue < tempValue)
                    return rewardSetData.cardGrade;
            }

            return rewardSetDatas[rewardSetDatas.Count - 1].cardGrade;
        }

        public CardGrade ConvertCardGrade(string grade)
        {
            return grade switch  {
                "Common" => CardGrade.Common,
                "Rare" => CardGrade.Rare,
                "Legendary" => CardGrade.Legendary,
                _ => CardGrade.None
            };
        }
    }
}