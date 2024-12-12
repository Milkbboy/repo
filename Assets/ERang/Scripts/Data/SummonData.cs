using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class SummonCardData
    {
        public SummonCardData(int cardId, int value)
        {
            this.cardId = cardId;
            this.value = value;
        }

        /// <summary>
        /// 소환될 카드의 id
        /// </summary>
        public int cardId;
        /// <summary>
        /// 해당 카드가 소환될 비중 값
        /// </summary>
        public int value;
    }

    public class SummonData
    {
        /// <summary>
        /// AbilityData에서 소환하는 그룹 Id
        /// </summary>
        public int summonGroupID;
        /// <summary>
        /// 참조 값
        /// </summary>
        public string nameDesc;
        /// <summary>
        /// 소환 카드 리스트
        /// </summary>
        public List<SummonCardData> summonCardDatas = new();

        public static List<SummonData> summonDatas = new();
        public static Dictionary<int, SummonData> summonDataDict = new();

        public static void Load(string path = "")
        {
            SummonDataTable summonDataTable = Resources.Load<SummonDataTable>(path);

            if (summonDataTable == null)
            {
                Debug.LogError("summonDataTable is not found");
                return;
            }

            foreach (var summonEntity in summonDataTable.items)
            {
                if (!summonDataDict.TryGetValue(summonEntity.Summon_GroupId, out SummonData summonData))
                {
                    summonData = new()
                    {
                        summonGroupID = summonEntity.Summon_GroupId,
                        nameDesc = summonEntity.NameDesc,
                        summonCardDatas = new() { new SummonCardData(summonEntity.Card_Id, summonEntity.Value) },
                    };

                    summonDatas.Add(summonData);
                    summonDataDict.Add(summonData.summonGroupID, summonData);
                }

                summonData.Initialize(summonEntity);
            }
        }

        void Initialize(SummonDataEntity entity)
        {
            summonCardDatas.Add(new SummonCardData(entity.Card_Id, entity.Value));
        }

        public static SummonData GetSummonData(int summonGroupID)
        {
            if (summonDataDict.TryGetValue(summonGroupID, out SummonData summonData))
            {
                return summonData;
            }

            return null;
        }
    }
}