using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    [System.Serializable]
    public class RewardCardData
    {
        public int cardId;
        public string cardNameDesc;
        public CardGrade cardGrade;
        public int weightValue;
        public int resultValue;
        public RewardType rewardType;
    }

    public class RewardData
    {
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
            RewardCardData rewardCardData = new()
            {
                cardId = entity.CardId,
                cardNameDesc = entity.CardNameDesc,
                cardGrade = GetCardGrade(entity),
                weightValue = entity.WeightValue,
                resultValue = GetResultValue(entity),
                rewardType = GetRewardType(entity),
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

        CardGrade GetCardGrade(RewardDataEntity entity)
        {
            CardGrade cardGrade = CardGrade.None;

            if (CardData.card_dict.TryGetValue(entity.CardId, out CardData cardData))
            {
                cardGrade = cardData.cardGrade;
            }
            else
            {
                cardGrade = entity.Grade switch
                {
                    "Common" => CardGrade.Common,
                    "Rare" => CardGrade.Rare,
                    "Legendary" => CardGrade.Legendary,
                    _ => CardGrade.None
                };
            }

            return cardGrade;
        }

        int GetResultValue(RewardDataEntity entity)
        {
            CardGrade cardGrade = GetCardGrade(entity);

            RewardSetData rewardSetData = RewardSetData.GetRewardSetData(cardGrade);

            if (rewardSetData == null)
            {
                Debug.LogError($"RewardSetData is not found: {cardGrade}");
                return entity.WeightValue;
            }

            return rewardSetData.value + entity.WeightValue;
        }

        RewardType GetRewardType(RewardDataEntity entity)
        {
            RewardType rewardType = RewardType.None;

            rewardType = entity.RewardType switch
            {
                "Card" => RewardType.Card,
                "Gold" => RewardType.Gold,
                "Mana" => RewardType.Mana,
                _ => RewardType.None
            };

            return rewardType;
        }
    }
}