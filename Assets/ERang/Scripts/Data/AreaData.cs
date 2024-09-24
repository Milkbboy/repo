using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEngine;
using UnityEditor;

namespace ERang.Data
{
    public class AreaData : ScriptableObject
    {
        public int areaID;
        public string nameDesc;
        public bool isEnd;
        public int floorMax;
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

            foreach (var areaEntity in areaDataTable.items)
            {
                string assetPath = $"Assets/ERang/Resources/Area/{areaEntity.AreaID}.asset";
                AreaData areaData = AssetDatabase.LoadAssetAtPath<AreaData>(assetPath);

                if (areaData == null)
                {
                    areaData = CreateInstance<AreaData>();
                    AssetDatabase.CreateAsset(areaData, assetPath);
                }

                areaData.Initialize(areaEntity);

                areaDatas.Add(areaData);
                areaDataDict.Add(areaData.areaID, areaData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static AreaData GetAreaData(int areaID)
        {
            return areaDataDict.TryGetValue(areaID, out AreaData areaData) ? areaData : null;
        }

        private void Initialize(AreaDataEntity entity)
        {
            areaID = entity.AreaID;
            nameDesc = entity.NameDesc;
            isEnd = entity.isEnd;
            floorMax = entity.FloorMax;
            levelGroupId = entity.LevelGroupID;
        }
    }
}