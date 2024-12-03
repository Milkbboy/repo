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

        public static List<RewardSetData> rewardSetDatas = new();
        public static Dictionary<string, RewardSetData> rewardSetDataDict = new();

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

                RewardSetData rewardSetData = new()
                {
                    grade = rewardSetDataEntity.CardGrade,
                    nameDesc = rewardSetDataEntity.NameDesc,
                    value = rewardSetDataEntity.Value
                };

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
    }
}