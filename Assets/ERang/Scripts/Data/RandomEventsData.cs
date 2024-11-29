using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class RandomEventsData
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
        public int levelGroupId;
        /// <summary>
        /// 랜덤 이벤트 타입
        /// </summary>
        public RandomEventType eventType;

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

                RandomEventsData randomEventsData = new();

                randomEventsData.Initialize(randomEventsEntity);

                randomEventsDatas.Add(randomEventsData);
                randomEventsDataDict.Add(randomEventsData.randomEventsID, randomEventsData);
            }
        }

        public static List<RandomEventsData> GetRandomEventsDatas()
        {
            return randomEventsDatas;
        }

        public static RandomEventsData GetEventsData(int randomEventsId)
        {
            return randomEventsDataDict.TryGetValue(randomEventsId, out RandomEventsData randomEventsData) ? randomEventsData : null;
        }

        public static RandomEventsData SelectRandomEvent(List<int> selectedEvents)
        {
            int totalRatio = 0;
            List<(int, int)> randomEventsRatioList = new();

            foreach (var randomEventsData in randomEventsDatas)
            {
                int ratio = randomEventsData.ratioValue;

                int selectedCount = selectedEvents.Where(e => e == randomEventsData.randomEventsID).Count();

                // 전에 뽑힌 횟수 만큼 확률을 줄인다.
                if (selectedCount > 0)
                {
                    int reduceRatio = selectedCount * randomEventsData.reductionValue;
                    // Debug.Log($"이벤트 {randomEventsData.randomEventsID} 선택 횟수: {selectedCount}, 확률 {ratio} - {reduceRatio} = {ratio - reduceRatio}");
                    ratio -= reduceRatio;
                }

                if (ratio > 0)
                {
                    totalRatio += ratio;
                    randomEventsRatioList.Add((randomEventsData.randomEventsID, ratio));
                }
            }

            int randomValue = Random.Range(0, totalRatio);
            int cumulativeRatio = 0;

            // Debug.Log($"totalRatio: {totalRatio}, randomValue: {randomValue}, {string.Join(", ", randomEventsRatioList)}");

            foreach (var (eventID, ratio) in randomEventsRatioList)
            {
                cumulativeRatio += ratio;

                if (randomValue < cumulativeRatio)
                {
                    // eventID에 해당하는 RandomEventsData를 반환
                    // Debug.Log($"선택된 이벤트: {eventID}");
                    return randomEventsDatas.FirstOrDefault(e => e.randomEventsID == eventID);
                }
            }

            Debug.LogError($"랜덤 이벤트 선택 실패해서 첫번째 이벤트 {randomEventsDatas[0].randomEventsID} 반환");
            return randomEventsDatas.FirstOrDefault();
        }

        private void Initialize(RandomEventsDataEntity entity)
        {
            randomEventsID = entity.RandomEventsID;
            nameDesc = entity.NameDesc;
            prefab = entity.Prefab;
            ratioValue = entity.RatioValue;
            reductionValue = entity.ReductionValue;
            levelGroupId = entity.LevelGroupID;
            eventType = ConvertRandomEventType(entity.RandomEventsID);
        }

        private RandomEventType ConvertRandomEventType(int eventsId)
        {
            return eventsId switch
            {
                1001 => RandomEventType.RandomBattle,
                1002 => RandomEventType.RandomBattleFix,
                1003 => RandomEventType.Roulette,
                1004 => RandomEventType.Matching,
                1005 => RandomEventType.DecreaseHp,
                1006 => RandomEventType.GetRelics,
                1007 => RandomEventType.GetCards,
                _ => RandomEventType.None,
            };
        }
    }
}