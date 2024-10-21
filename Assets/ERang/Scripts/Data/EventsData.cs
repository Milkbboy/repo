using System.Collections.Generic;
using System.Linq;
using ERang.Table;
using UnityEditor;
using UnityEngine;

namespace ERang.Data
{
    public class EventsData : ScriptableObject
    {
        public int eventsID;
        public string nameDesc;
        public EventType eventType;
        public List<int> excludeFloors = new();
        public List<int> includeFloors = new();
        public bool excludeEndFloor;
        public string prefab;
        public int minValue;
        public int maxValue;
        public int eliteBattleLevelGroupID;
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

        public static List<EventsData> GetActDatas()
        {
            return eventsDatas;
        }

        public static EventsData GetEventsData(int actID)
        {
            return eventsDataDict.TryGetValue(actID, out EventsData eventsData) ? eventsData : null;
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