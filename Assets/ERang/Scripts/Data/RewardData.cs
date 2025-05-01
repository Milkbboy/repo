using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    [System.Serializable]
    public class RewardCardData
    {
        public int id;
        public int cardId;
        public string cardNameDesc;
        public RewardType rewardType;
        public CardGrade cardGrade;
        public int weightValue;
        public int resultValue;
        public int goldMin;
        public int goldMax;
        public int hpMin;
        public int hpMax;
    }

    public class RewardData
    {
        private static Dictionary<(int rewardId, int masterId), int> nextIds = new();
        public int rewardId;
        public int masterId;
        public string nameDesc;
        public List<RewardCardData> rewardCardDatas = new();

        public static List<RewardData> rewardDatas = new();
        public static Dictionary<(int rewardId, int masterId), RewardData> rewardDataDict = new();

        public static void Load(string path = "")
        {
            RewardDataTable rewardDataTable = Resources.Load<RewardDataTable>(path);

            if (rewardDataTable == null)
            {
                Debug.LogError("RewardDataTable is not found");
                return;
            }

            Debug.Log($"Total items in table: {rewardDataTable.items.Count}");

            foreach (var rewardDataEntity in rewardDataTable.items)
            {
                var key = (rewardDataEntity.RewardID, rewardDataEntity.Master_Id);

                if (!rewardDataDict.TryGetValue(key, out RewardData rewardData))
                {
                    rewardData = new()
                    {
                        rewardId = rewardDataEntity.RewardID,
                        masterId = rewardDataEntity.Master_Id,
                        nameDesc = rewardDataEntity.NameDesc,
                    };

                    rewardDataDict.Add(key, rewardData);
                }

                rewardData.Initialize(rewardDataEntity);
            }
        }

        void Initialize(RewardDataEntity entity)
        {
            var key = (entity.RewardID, entity.Master_Id);

            if (!nextIds.ContainsKey(key))
            {
                nextIds[key] = 1;
            }

            RewardCardData rewardCardData = new()
            {
                id = nextIds[key]++,
                cardId = entity.CardId,
                cardNameDesc = entity.CardNameDesc,
                cardGrade = GetCardGrade(entity),
                weightValue = entity.WeightValue,
                resultValue = GetResultValue(entity),
                rewardType = GetRewardType(entity),
                goldMin = entity.GoldMin,
                goldMax = entity.GoldMax,
                hpMin = entity.HpMin,
                hpMax = entity.HpMax,
            };

            rewardCardDatas.Add(rewardCardData);
        }

        public static RewardData GetRewardData(int rewardId, int masterId)
        {
            var key = (rewardId, masterId);

            if (rewardDataDict.ContainsKey(key))
                return rewardDataDict[key];

            Debug.LogError($"RewardData is not found: rewardId: {rewardId}, masterId: {masterId}");

            return null;
        }

        int GetResultValue(RewardDataEntity entity)
        {
            RewardType rewardType = GetRewardType(entity);
            CardGrade cardGrade = GetCardGrade(entity);

            RewardSetData rewardSetData = RewardSetData.GetRewardSetData(rewardType, cardGrade);
            // Debug.Log($"RewardData GetResultValue: id: {entity.RewardID}, rewardType: {rewardType}, cardGrade: {cardGrade}, resetData.value: {rewardSetData.value}, weightValue: {entity.WeightValue}, resultValue: {rewardSetData.value + entity.WeightValue}");

            return rewardSetData.value + entity.WeightValue;
        }

        RewardType GetRewardType(RewardDataEntity entity)
        {
            RewardType rewardType = RewardType.None;

            rewardType = entity.RewardType switch
            {
                "Card" => RewardType.Card,
                "Gold" => RewardType.Gold,
                "HP" => RewardType.HP,
                _ => RewardType.None
            };

            return rewardType;
        }

        CardGrade GetCardGrade(RewardDataEntity entity)
        {
            CardGrade cardGrade = CardGrade.None;

            if (CardData.card_dict.TryGetValue(entity.CardId, out CardData cardData))
            {
                cardGrade = cardData.cardGrade;
            }
            else
            {
                cardGrade = entity.CardGrade switch
                {
                    "Common" => CardGrade.Common,
                    "Rare" => CardGrade.Rare,
                    "Legendary" => CardGrade.Legendary,
                    _ => CardGrade.None
                };
            }

            return cardGrade;
        }
    }
}