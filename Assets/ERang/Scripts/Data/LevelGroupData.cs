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
            levelId = entity.LevelID;
            nameDesc = entity.NameDesc;
            spawnRatio = entity.SpawnRatio;
            cardIds.Add(entity.Pos01_CardID);
            cardIds.Add(entity.Pos02_CardID);
            cardIds.Add(entity.Pos03_CardID);
            cardIds.Add(entity.MasterID);
        }

        public int levelId;
        public string nameDesc;
        public int spawnRatio;
        public List<int> cardIds = new();
    }

    public class LevelGroupData : ScriptableObject
    {
        public int levelGroupId;
        public List<LevelData> levelDatas = new();

        public static List<LevelGroupData> levelGroupDatas = new();
        public static Dictionary<int, LevelGroupData> levelGroupDictionary = new();
        public static Dictionary<int, LevelData> levelDataDictionary = new();

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
                    levelGroupData = AssetDatabase.LoadAssetAtPath<LevelGroupData>(assetPath);

                    if (levelGroupData == null)
                    {
                        levelGroupData = CreateInstance<LevelGroupData>();
                        AssetDatabase.CreateAsset(levelGroupData, assetPath);
                    }
                    else
                    {
                        levelGroupData.levelDatas.Clear();
                    }

                    levelGroupDictionary[levelGroupEntity.LevelGroupID] = levelGroupData;
                }

                levelGroupData.Initialize(levelGroupEntity);

                EditorUtility.SetDirty(levelGroupData); // 데이터가 변경되었음을 Unity에 알림
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static LevelGroupData GetLevelGroupData(int levelGroupId)
        {
            return levelGroupDictionary.TryGetValue(levelGroupId, out LevelGroupData levelGroupData) ? levelGroupData : null;
        }

        public static LevelData GetLevelData(int levelId)
        {
            return levelDataDictionary.TryGetValue(levelId, out LevelData levelData) ? levelData : null;
        }

        /// <summary>
        /// 레벨 그룹 아이디로 레벨 데이터들 가져오기
        /// </summary>
        /// <param name="levelGroupId"></param>
        /// <returns></returns>
        public static List<LevelData> GetLevelDatas(int levelGroupId)
        {
            return levelGroupDictionary.TryGetValue(levelGroupId, out LevelGroupData levelGroupData) ? levelGroupData.levelDatas : null;
        }

        public void Initialize(LevelGroupDataEntity entity)
        {
            levelGroupId = entity.LevelGroupID;

            LevelData levelData = new(entity);

            if (levelDatas.Any(data => data.levelId == levelData.levelId) == false)
                levelDatas.Add(levelData);

            levelDataDictionary[levelData.levelId] = levelData;
        }
    }
}