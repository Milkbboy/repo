using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEngine;
using UnityEditor;

namespace ERang.Data
{
    public class AreaData
    {
        public int areaID;
        public string nameDesc;
        public bool isEnd;
        public int floorStart;
        public int floorMax;
        public int floorCount;
        public int levelGroupId;

        public static List<AreaData> areaDatas = new();
        public static Dictionary<int, AreaData> areaDataDict = new();

        public static void Load(string path = "")
        {
            AreaDataTable areaDataTable = Resources.Load<AreaDataTable>(path);

            if (areaDataTable == null)
            {
                Debug.LogError("AreaDataTable is not found");
                return;
            }

            int prevFloorMax = 0;

            foreach (var areaEntity in areaDataTable.items)
            {
                if (areaDataDict.ContainsKey(areaEntity.AreaID))
                    continue;

                AreaData areaData = new();

                areaData.Initialize(areaEntity, prevFloorMax);

                prevFloorMax = areaData.floorMax;

                areaDatas.Add(areaData);
                areaDataDict.Add(areaData.areaID, areaData);
            }
        }

        public static AreaData GetAreaDataFromFloor(int floor)
        {
            return areaDatas.Find(areaData => areaData.floorStart <= floor && floor <= areaData.floorMax);
        }

        public static AreaData GetAreaData(int areaID)
        {
            return areaDataDict.TryGetValue(areaID, out AreaData areaData) ? areaData : null;
        }

        public static List<AreaData> GetAreaDatas()
        {
            return areaDatas;
        }

        /// <summary>
        /// 보스 배틀이 있는 지역 데이터 찾기
        /// </summary>
        /// <param name="areaIds"></param>
        /// <returns></returns>
        public static AreaData FindBossBattleAreaData(List<int> areaIds)
        {
            return areaDatas.Find(areaData => areaIds.Contains(areaData.areaID) && areaData.isEnd);
        }

        public static AreaData FindBossBattleAreaData(int actId)
        {
            ActData actData = ActData.GetActData(actId);

            return FindBossBattleAreaData(actData.areaIds);
        }

        private void Initialize(AreaDataEntity entity, int prevFloorMax)
        {
            areaID = entity.AreaID;
            nameDesc = entity.NameDesc;
            isEnd = entity.isEnd;
            floorStart = prevFloorMax + 1;
            floorMax = entity.isEnd ? floorStart : entity.FloorMax;
            floorCount = floorMax - floorStart + 1;
            levelGroupId = entity.LevelGroupID;
        }
    }
}