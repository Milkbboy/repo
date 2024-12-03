using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class RewardData
    {
        public int rewardId;
        public int masterId;
        public string nameDesc;
        public int cardId;
        public string cardNameDesc;
        public string grade;
        public int weightValue;

        public static List<RewardData> rewardDatas = new();
        public static Dictionary<int, RewardData> rewardDataDict = new();

        public static void Load(string path = "")
        {
            RewardDataTable rewardDataTable = Resources.Load<RewardDataTable>(path);

            if (rewardDataTable == null)
            {
                Debug.LogError("RewardDataTable is not found");
                return;
            }

            foreach (var rewardDataEntity in rewardDataTable.items)
            {
                if (rewardDataDict.ContainsKey(rewardDataEntity.RewardID))
                    continue;

                RewardData rewardData = new()
                {
                    rewardId = rewardDataEntity.RewardID,
                    masterId = rewardDataEntity.Master_Id,
                    nameDesc = rewardDataEntity.NameDesc,
                    cardId = rewardDataEntity.CardId,
                    cardNameDesc = rewardDataEntity.CardNameDesc,
                    grade = rewardDataEntity.Grade,
                    weightValue = rewardDataEntity.WeightValue,
                };

                rewardDatas.Add(rewardData);
                rewardDataDict.Add(rewardData.rewardId, rewardData);
            }
        }

        public static RewardData GetRewardData(int rewardId)
        {
            if (rewardDataDict.ContainsKey(rewardId))
                return rewardDataDict[rewardId];

            Debug.LogError($"RewardData is not found: {rewardId}");

            return null;
        }
    }
}