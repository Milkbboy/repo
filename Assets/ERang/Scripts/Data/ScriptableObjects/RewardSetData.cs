using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class RewardSetData
    {
        public RewardType rewardType;
        public CardGrade cardGrade;
        public string nameDesc;
        public int value;
        public bool isOnce;

        public static List<RewardSetData> rewardSetDatas = new();
        public static Dictionary<(RewardType, CardGrade), RewardSetData> rewardSetDataDict = new();

        private static int sumValue = 0;

        public void Initialize(RewardSetDataEntity entity, RewardType rewardType, CardGrade cardGrade)
        {
            this.rewardType = rewardType;
            this.cardGrade = cardGrade;
            nameDesc = entity.NameDesc;
            value = entity.Value;
            isOnce = entity.IsOnce;

            // Debug.Log($"RewardSetData Initialize: {grade}, {nameDesc}, {value}, {isOnce}, {rewardType}");
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
                RewardType rewardType = ConvertRewardType(rewardSetDataEntity.RewardType);
                CardGrade cardGrade = ConvertCardGrade(rewardSetDataEntity.CardGrade);

                if (rewardSetDataDict.ContainsKey((rewardType, cardGrade)))
                    continue;

                RewardSetData rewardSetData = new();
                rewardSetData.Initialize(rewardSetDataEntity, rewardType, cardGrade);

                sumValue += rewardSetData.value;

                rewardSetDatas.Add(rewardSetData);
                rewardSetDataDict.Add((rewardSetData.rewardType, rewardSetData.cardGrade), rewardSetData);

                // Debug.Log($"RewardSetData Load: {rewardSetData.rewardType}, {rewardSetData.cardGrade}, {rewardSetData.value}, {rewardSetData.nameDesc}, {rewardSetData.isOnce}");
            }
        }

        public static RewardSetData GetRewardSetData(RewardType rewardType, CardGrade cardGrade)
        {
            var key = (rewardType, cardGrade);

            if (rewardSetDataDict.ContainsKey(key))
                return rewardSetDataDict[key];

            Debug.LogError($"RewardSetData is not found: {rewardType}, {cardGrade}");

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

        public static List<(RewardType rewardType, CardGrade cardGrade)> GetRewardValues(int count)
        {
            if (rewardSetDatas.Count == 0)
            {
                Debug.LogError("RewardSetDatas 에 값이 없습니다.");
                return new List<(RewardType, CardGrade)>();
            }

            List<(RewardType rewardType, CardGrade cardGrade)> rewardTypes = new();
            HashSet<(RewardType rewardType, CardGrade cardGrade)> onceSelectedTypes = new();

            for (int i = 0; i < count; ++i)
            {
                // 현재 선택 가능한 아이템들의 총 가중치 계산
                int availableSumValue = 0;
                foreach (var data in rewardSetDatas)
                {
                    if (data.isOnce && onceSelectedTypes.Contains((data.rewardType, data.cardGrade)))
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
                    if (rewardSetData.isOnce && onceSelectedTypes.Contains((rewardSetData.rewardType, rewardSetData.cardGrade)))
                        continue;

                    tempValue += rewardSetData.value;

                    if (randomValue < tempValue)
                    {
                        rewardTypes.Add((rewardSetData.rewardType, rewardSetData.cardGrade));

                        // isOnce가 1인 경우 선택된 목록에 추가
                        if (rewardSetData.isOnce)
                            onceSelectedTypes.Add((rewardSetData.rewardType, rewardSetData.cardGrade));

                        break;
                    }
                }
            }

            return rewardTypes;
        }

        private static RewardType ConvertRewardType(string rewardType)
        {
            return rewardType switch
            {
                "Card" => RewardType.Card,
                "HP" => RewardType.HP,
                "Gold" => RewardType.Gold,
                _ => RewardType.None
            };
        }

        private static CardGrade ConvertCardGrade(string cardGrade)
        {
            return cardGrade switch
            {
                "Common" => CardGrade.Common,
                "Rare" => CardGrade.Rare,
                "Legendary" => CardGrade.Legendary,
                _ => CardGrade.None
            };
        }
    }
}