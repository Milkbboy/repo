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
        public List<int> areaIDs = new();

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
                string assetPath = $"Assets/ERang/Resources/Act/{actEntity.ActID}.asset";
                ActData actData = AssetDatabase.LoadAssetAtPath<ActData>(assetPath);

                if (actData == null)
                {
                    actData = CreateInstance<ActData>();
                    AssetDatabase.CreateAsset(actData, assetPath);
                }
                else
                {
                    actData.areaIDs.Clear();
                }

                actData.Initialize(actEntity);

                actDatas.Add(actData);
                actDataDict.Add(actData.actID, actData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ActData GetActData(int actID)
        {
            return actDataDict.TryGetValue(actID, out ActData actData) ? actData : null;
        }

        private void Initialize(ActDataEntity entity)
        {
            actID = entity.ActID;
            nameDesc = entity.NameDesc;
            areaIDs = Utils.ParseIntArray(entity.AreaID).ToList();
        }
    }
}