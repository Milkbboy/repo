using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEditor;
using UnityEngine;

namespace ERang.Data
{
    public class EventsData : ScriptableObject
    {
        /// <summary>
        /// 이벤트 id 값
        /// </summary>
        public int eventsID;
        /// <summary>
        /// 참조 데이터로 개발명 입력
        /// </summary>
        public string nameDesc;
        /// <summary>
        /// 맵에 배치되는 이벤트 타입을 입력 (우선 3개의 타입을 제작 Store, EliteBattle, Random Event)
        /// </summary>
        public EventType eventType;
        /// <summary>
        /// 해당 이벤트가 배치될 때 배치되면 안되는 층을 입력
        /// </summary>
        public List<int> excludeFloors = new();
        /// <summary>
        /// 해당 맵의 최종 층도 제외할 것인지 입력 (True 제외, False 포함)
        /// </summary>
        public bool excludeEndFloor;
        /// <summary>
        /// 해당 맵의 특정 층에 배치될 때, 배치될 층을 복수로 입력
        /// </summary>
        public List<int> includeFloors = new();
        /// <summary>
        /// 해당 이벤트 발생시 로드할 프리팹 입력 (Random Event는 해당 시트에서 입력)
        /// </summary>
        public string prefab;
        /// <summary>
        /// 최소 배치 값
        /// </summary>
        public int minValue;
        /// <summary>
        /// 최대 배치 값
        /// </summary>
        public int maxValue;
        /// <summary>
        /// 앨리트 전투 타입에 사용. 앨리트 배치 데이터 입력 (LevelGroupData의 id 참조)
        /// </summary>
        public int eliteBattleLevelGroupID;
        /// <summary>
        /// 랜덤 이벤트에 들어갈 이벤트 리스트 입력 (RandomEventsData id 참조)
        /// </summary>
        public List<int> randomEventIDs = new();

        public static List<EventsData> eventsDatas = new();
        public static Dictionary<int, EventsData> eventsDataDict = new();

        public static void Load(string path = "")
        {
            EventsDataTable eventsDataTable = Resources.Load<EventsDataTable>(path);

            if (eventsDataTable == null)
            {
                Debug.LogError("eventsDataTable is not found");
                return;
            }

            foreach (var eventsEntity in eventsDataTable.items)
            {
                if (eventsDataDict.ContainsKey(eventsEntity.EventsID))
                    continue;

                string assetPath = $"Assets/ERang/Resources/Events/{eventsEntity.EventsID}.asset";
                EventsData eventsData = AssetDatabase.LoadAssetAtPath<EventsData>(assetPath);

                if (eventsData == null)
                {
                    eventsData = CreateInstance<EventsData>();
                    AssetDatabase.CreateAsset(eventsData, assetPath);
                }
                else
                {
                    eventsData.excludeFloors.Clear();
                    eventsData.includeFloors.Clear();
                    eventsData.randomEventIDs.Clear();
                }

                eventsData.Initialize(eventsEntity);

                eventsDatas.Add(eventsData);
                eventsDataDict.Add(eventsData.eventsID, eventsData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<EventsData> GetEventsDatas()
        {
            return eventsDatas;
        }

        public static EventsData GetEventsData(int eventsId)
        {
            return eventsDataDict.TryGetValue(eventsId, out EventsData eventsData) ? eventsData : null;
        }

        private void Initialize(EventsDataEntity entity)
        {
            eventsID = entity.EventsID;
            nameDesc = entity.NameDesc;
            eventType = ConvertEventType(entity.EventType);
            excludeFloors = Utils.ParseIntArray(entity.ExcludeFloor).ToList();
            includeFloors = Utils.ParseIntArray(entity.IncludeFloor).ToList();
            excludeEndFloor = entity.ExcludeEndFloor;
            prefab = entity.Prefab;
            minValue = entity.MinValue;
            maxValue = entity.MaxValue;
            eliteBattleLevelGroupID = entity.EliteBattleLevelGroupID;
            randomEventIDs = Utils.ParseIntArray(entity.RandomEventID).ToList();
        }

        public EventType ConvertEventType(string eventType)
        {
            return eventType switch
            {
                "Store" => EventType.Store,
                "EliteBattle" => EventType.EliteBattle,
                "RandomEvent" => EventType.RandomEvent,
                _ => EventType.None,
            };
        }
    }
}