using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEditor;
using UnityEngine;

namespace ERang.Data
{
    /// <summary>
    /// 레벨 데이터
    /// - 사용하기 편하게 하기 위해 Entity를 이용하여 데이터를 가공
    /// </summary>
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

        public static int GetRandomLevelId(int levelGroupId)
        {
            LevelData levelData = GetRandomLevelData(levelGroupId);

            if (levelData == null)
            {
                Debug.LogError($"LevelData 를 찾지 못해 <color={Colors.Red}>0</color> 반환");
                return 0;
            }

            return levelData.levelId;
        }

        public static LevelData GetRandomLevelData(int levelGroupId)
        {
            List<LevelData> levelDatas = GetLevelDatas(levelGroupId);

            if (levelDatas == null || levelDatas.Count == 0)
            {
                Debug.LogError($"LevelData 를 찾지 못해 <color={Colors.Red}>null</color> 반환");
                return null;
            }

            if (levelDatas.Count == 1)
            {
                Debug.Log($"LevelData 1개인 경우 {levelDatas[0].levelId}. levelDatas.Count: {levelDatas.Count}");
                return levelDatas[0];
            }

            float totalRatio = levelDatas.Sum(x => x.spawnRatio);
            float randomValue = Random.Range(0, totalRatio);
            float cumulativeRatio = 0;

            foreach (var levelData in levelDatas)
            {
                cumulativeRatio += levelData.spawnRatio;

                if (randomValue < cumulativeRatio)
                {
                    Debug.Log($"LevelData 뽑기 성공: {levelData.levelId}. totalRatio: {totalRatio}, randomValue: {randomValue}, cumulativeRatio: {cumulativeRatio}");
                    return levelData;
                }
            }

            Debug.LogWarning($"LevelData 뽑기 실패로 <color={Colors.Red}>안전장치 발동</color>. 첫번째 LevelData 반환");

            return levelDatas[0];
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