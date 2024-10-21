using System.Collections.Generic;
using ERang.Table;
using UnityEditor;
using UnityEngine;

namespace ERang.Data
{
    public class RandomEventsData : ScriptableObject
    {
        /// <summary>
        /// 랜덤 이벤트 id 값
        /// </summary>
        public int randomEventsID;
        /// <summary>
        /// 참조 데이터로 개발명 입력
        /// </summary>
        public string nameDesc;
        /// <summary>
        /// 해당 이벤트가 선택될 때 로드하는 프리펩
        /// </summary>
        public string prefab;
        /// <summary>
        /// 해당 이벤트가 선택될 비중 값 (EventsData/RandomEventID에 나열된 이벤트의 모든 Ratio 값을 더한 뒤 해당 값을 비중으로 설정)
        /// </summary>
        public int ratioValue;
        /// <summary>
        /// 해당 이벤트가 선택 되었을 경우 차감되는 비중 값
        /// </summary>
        public int reductionValue;
        /// <summary>
        /// 랜덤 이벤트 중 전투 발생 시 입력 (입력이 없을 경우 해당 Area의 배치 데이터를 그대로 사용)
        /// </summary>
        public int levelGroupID;

        public static List<RandomEventsData> randomEventsDatas = new();
        public static Dictionary<int, RandomEventsData> randomEventsDataDict = new();

        public static void Load(string path = "")
        {
            RandomEventsDataTable randomEventsDataTable = Resources.Load<RandomEventsDataTable>(path);

            if (randomEventsDataTable == null)
            {
                Debug.LogError("randomEventsDataTable is not found");
                return;
            }

            foreach (var randomEventsEntity in randomEventsDataTable.items)
            {
                if (randomEventsDataDict.ContainsKey(randomEventsEntity.RandomEventsID))
                    continue;

                string assetPath = $"Assets/ERang/Resources/RandomEvents/{randomEventsEntity.RandomEventsID}.asset";
                RandomEventsData randomEventsData = AssetDatabase.LoadAssetAtPath<RandomEventsData>(assetPath);

                if (randomEventsData == null)
                {
                    randomEventsData = CreateInstance<RandomEventsData>();
                    AssetDatabase.CreateAsset(randomEventsData, assetPath);
                }

                randomEventsData.Initialize(randomEventsEntity);

                randomEventsDatas.Add(randomEventsData);
                randomEventsDataDict.Add(randomEventsData.randomEventsID, randomEventsData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<RandomEventsData> GetRandomEventsDatas()
        {
            return randomEventsDatas;
        }

        public static RandomEventsData GetEventsData(int randomEventsId)
        {
            return randomEventsDataDict.TryGetValue(randomEventsId, out RandomEventsData randomEventsData) ? randomEventsData : null;
        }

        private void Initialize(RandomEventsDataEntity entity)
        {
            randomEventsID = entity.RandomEventsID;
            nameDesc = entity.NameDesc;
            prefab = entity.Prefab;
            ratioValue = entity.RatioValue;
            reductionValue = entity.ReductionValue;
            levelGroupID = entity.LevelGroupID;
        }
    }
}