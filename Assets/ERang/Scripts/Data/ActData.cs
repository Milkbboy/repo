using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEditor;
using UnityEngine;

namespace ERang.Data
{
    public class ActData : ScriptableObject
    {
        public int actID;
        public string nameDesc;
        public List<int> areaIds = new();

        public static List<ActData> actDatas = new();
        public static Dictionary<int, ActData> actDataDict = new();

        public static void Load(string path = "")
        {
            ActDataTable actDataTable = Resources.Load<ActDataTable>(path);

            if (actDataTable == null)
            {
                Debug.LogError("ActDataTable is not found");
                return;
            }

            foreach (var actEntity in actDataTable.items)
            {
                if (actDataDict.ContainsKey(actEntity.ActID))
                    continue;

                string assetPath = $"Assets/ERang/Resources/Act/{actEntity.ActID}.asset";
                ActData actData = AssetDatabase.LoadAssetAtPath<ActData>(assetPath);

                if (actData == null)
                {
                    actData = CreateInstance<ActData>();
                    AssetDatabase.CreateAsset(actData, assetPath);
                }
                else
                {
                    actData.areaIds.Clear();
                }

                actData.Initialize(actEntity);

                actDatas.Add(actData);
                actDataDict.Add(actData.actID, actData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<ActData> GetActDatas()
        {
            return actDatas;
        }

        public static ActData GetActData(int actID)
        {
            return actDataDict.TryGetValue(actID, out ActData actData) ? actData : null;
        }

        private void Initialize(ActDataEntity entity)
        {
            actID = entity.ActID;
            nameDesc = entity.NameDesc;
            areaIds = Utils.ParseIntArray(entity.AreaID).ToList();
        }
    }
}