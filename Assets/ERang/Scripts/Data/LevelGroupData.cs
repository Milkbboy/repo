using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEditor;
using UnityEngine;

namespace ERang.Data
{
    [System.Serializable]
    public class LevelData
    {
        public LevelData(LevelGroupDataEntity entity)
        {
            levelID = entity.LevelID;
            nameDesc = entity.NameDesc;
            spawnRatio = entity.SpawnRatio;
            pos01_CardId = entity.Pos01_CardID;
            pos02_CardId = entity.Pos02_CardID;
            pos03_CardId = entity.Pos03_CardID;
            pos04_CardId = entity.MasterID;
        }

        public int levelID;
        public string nameDesc;
        public int spawnRatio;
        public int pos01_CardId;
        public int pos02_CardId;
        public int pos03_CardId;
        public int pos04_CardId;
    }

    public class LevelGroupData : ScriptableObject
    {
        public int levelGroupID;
        public List<LevelData> levelDatas = new();

        public static List<LevelGroupData> levelGroupDatas = new();
        public static Dictionary<int, LevelGroupData> levelGroupDictionary = new();

        public static void Load(string path = "")
        {
            LevelGroupDataTable levelGroupDataTable = Resources.Load<LevelGroupDataTable>(path);

            if (levelGroupDataTable == null)
            {
                Debug.LogError("LevelGroupDataTable is not found");
                return;
            }

            foreach (var levelGroupEntity in levelGroupDataTable.items)
            {
                if (!levelGroupDictionary.TryGetValue(levelGroupEntity.LevelGroupID, out LevelGroupData levelGroupData))
                {
                    string assetPath = $"Assets/ERang/Resources/LevelGroup/{levelGroupEntity.LevelGroupID}.asset";

                    levelGroupData = Resources.Load<LevelGroupData>(assetPath);

                    if (levelGroupData == null)
                    {
                        levelGroupData = CreateInstance<LevelGroupData>();
                        AssetDatabase.CreateAsset(levelGroupData, assetPath);
                    }

                    levelGroupData.Initialize(levelGroupEntity);
                    levelGroupDictionary[levelGroupEntity.LevelGroupID] = levelGroupData;
                }

                LevelData levelData = new(levelGroupEntity);

                levelGroupData.levelDatas.Add(levelData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void Initialize(LevelGroupDataEntity entity)
        {
            levelGroupID = entity.LevelGroupID;

            LevelData levelData = new(entity);
            levelDatas.Add(levelData);
        }
    }
}