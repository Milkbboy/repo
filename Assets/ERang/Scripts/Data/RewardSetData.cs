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
        public RewardType rewardType;
        public int isOnce;

        public static List<RewardSetData> rewardSetDatas = new();
        public static Dictionary<string, RewardSetData> rewardSetDataDict = new();

        private static int sumValue = 0;

        public void Initialize(RewardSetDataEntity entity)
        {
            grade = entity.RewardType;
            nameDesc = entity.NameDesc;
            value = entity.Value;
            isOnce = entity.IsOnce;
            rewardType = ConvertRewardType(entity.RewardType);
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

        public static RewardSetData GetRewardSetData(RewardType rewardType)
        {
            foreach (var rewardSetData in rewardSetDatas)
            {
                if (rewardSetData.rewardType == rewardType)
                    return rewardSetData;
            }

            Debug.LogError($"RewardSetData is not found: {rewardType}");
            return null;
        }

        public static List<RewardType> GetRewardTypes(int count)
        {
            if (rewardSetDatas.Count == 0)
            {
                Debug.LogError("RewardSetDatas 에 값이 없습니다.");
                return new List<RewardType>();
            }

            List<RewardType> rewardTypes = new();
            HashSet<RewardType> onceSelectedTypes = new();

            for (int i = 0; i < count; ++i)
            {
                // 현재 선택 가능한 아이템들의 총 가중치 계산
                int availableSumValue = 0;
                foreach (var data in rewardSetDatas)
                {
                    if (data.isOnce == 1 && onceSelectedTypes.Contains(data.rewardType))
                        continue;
                    availableSumValue += data.value;
                }

                if (availableSumValue <= 0)
                {
                    // 선택 가능한 아이템이 없는 경우, 모든 아이템을 다시 선택 가능하게 함
                    onceSelectedTypes.Clear();
                    continue; // 현재 반복을 다시 시도
                }

                int randomValue = Random.Range(0, availableSumValue);
                int tempValue = 0;

                foreach (var rewardSetData in rewardSetDatas)
                {
                    // isOnce가 1이고 이미 선택된 아이템은 스킵
                    if (rewardSetData.isOnce == 1 && onceSelectedTypes.Contains(rewardSetData.rewardType))
                        continue;

                    tempValue += rewardSetData.value;

                    if (randomValue < tempValue)
                    {
                        rewardTypes.Add(rewardSetData.rewardType);

                        // isOnce가 1인 경우 선택된 목록에 추가
                        if (rewardSetData.isOnce == 1)
                            onceSelectedTypes.Add(rewardSetData.rewardType);

                        break;
                    }
                }
            }

            return rewardTypes;
        }

        public RewardType ConvertRewardType(string grade)
        {
            return grade switch
            {
                "Common" => RewardType.Common,
                "Rare" => RewardType.Rare,
                "Legendary" => RewardType.Legendary,
                "HP" => RewardType.HP,
                "Gold" => RewardType.Gold,
                _ => RewardType.None
            };
        }
    }
}