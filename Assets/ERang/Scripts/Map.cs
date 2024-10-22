using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using Newtonsoft.Json;

namespace ERang
{
    public class MapLocation
    {
        public int seed;
        public int depth;
        public int index;
        public EventType eventType;

        public List<(int, string)> directions = new();
        public List<int> adjacency = new();

        public int ID { get { return GetID(depth, index); } }

        public MapLocation(int seed, int depth, int index, EventType eventType = EventType.None)
        {
            this.seed = seed;
            this.depth = depth;
            this.index = index;
            this.eventType = eventType;
        }

        public void AddAdjacent(MapLocation loc, string direction = "")
        {
            if (loc != null && !adjacency.Contains(loc.ID))
            {
                adjacency.Add(loc.ID);
                directions.Add((loc.ID, direction));
            }
        }

        public bool IsAdjacent(MapLocation loc)
        {
            return adjacency.Contains(loc.ID);
        }

        public static int GetID(int d, int i)
        {
            return d * 100 + i;
        }
    }

    public class Map : MonoBehaviour
    {
        public static Map Instance { get; private set; }

        public Dictionary<int, MapLocation> locations = new();
        public Dictionary<int, int> depthWidths = new();
        // 층에서 고른 인덱스 (표시용)
        public Dictionary<int, int> selectedDepthIndies = new();

        private int actId;
        private int areaId;
        private int floor;
        private int maxFloor;
        private int lastLocationId;
        private System.Random random = new();

        private MapViewer viewer;

        void Start()
        {
            Instance = this;

            actId = PlayerPrefsUtility.GetInt("ActId", 0);
            areaId = PlayerPrefsUtility.GetInt("AreaId", 0);
            floor = PlayerPrefsUtility.GetInt("Floor", 1);
            maxFloor = PlayerPrefsUtility.GetInt("MaxFloor", 0);
            lastLocationId = PlayerPrefsUtility.GetInt("LastLocationId", 0);

            viewer = GetComponent<MapViewer>();

            AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

            LoadMapData();

            Debug.Log($"LoadMap Success. floor: {floor}, areaId: {areaId}, areaData.areaID: {areaData.areaID}, locations.Count: {locations.Count}");

            if (floor != 2 && (areaId != areaData.areaID || locations.Count == 0))
            {
                // 맵 새로 생성되면 처음 부터 시작
                lastLocationId = 0;
                GenerateMap();
            }

            foreach (var depthEntry in selectedDepthIndies)
            {
                int depth = depthEntry.Key;
                int index = depthEntry.Value;

                // Debug.Log($"Selected Depth: {depth}, Index: {index}");
            }

            viewer.DrawMap(floor, selectedDepthIndies);
        }

        void OnDisable()
        {
            SaveMapData();
        }

        public void ClickLocation(int locationId)
        {
            int floor = locationId / 100;
            int floorIndex = locationId % 100;

            if (floor != this.floor)
            {
                return;
            }

            locations.TryGetValue(locationId, out MapLocation loc);

            if (loc == null)
            {
                Debug.LogError($"Location not found: {locationId}");
                return;
            }

            // Debug.Log($"Click Location: {locationId}, lastLocationId: {lastLocationId}, floor: {floor}, floorIndex: {floorIndex}");

            if (locations.TryGetValue(lastLocationId, out MapLocation currentLoc))
            {
                if (!currentLoc.adjacency.Contains(locationId))
                {
                    Debug.LogError($"Not adjacent location: {locationId}");
                    return;
                }
            }

            viewer.SelectFloorIndex(floor, floorIndex);
            selectedDepthIndies[floor] = floorIndex;

            AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

            if (areaData == null)
                areaData = AreaData.GetAreaDatas().First();

            LevelGroupData levelGroupData = LevelGroupData.GetLevelGroupData(areaData.levelGroupId);
            LevelData randomLevelData = GetRandomLevelData(levelGroupData.levelDatas);

            PlayerPrefsUtility.SetInt("ActId", actId);
            PlayerPrefsUtility.SetInt("AreaId", areaData.areaID);
            PlayerPrefsUtility.SetInt("Floor", floor);
            PlayerPrefsUtility.SetInt("LevelId", randomLevelData.levelId);
            PlayerPrefsUtility.SetInt("LastLocationId", locationId);

            // Save depthIndies
            string selectedDepthIndiesJson = JsonConvert.SerializeObject(selectedDepthIndies, Formatting.None);
            PlayerPrefsUtility.SetString("SelectedDepthIndies", selectedDepthIndiesJson);

            // for logging
            List<(int, string, int)> cardDataList = new();

            for (int i = 0; i < randomLevelData.cardIds.Count(); ++i)
            {
                int pos = i + 1;
                int cardId = randomLevelData.cardIds[i];

                if (cardId == 0)
                {
                    cardDataList.Add((pos, "빈자리", 0));
                    continue;
                }

                CardData monsterCardData = MonsterCardData.GetCardData(cardId);

                if (monsterCardData == null)
                {
                    Debug.LogError($"카드 데이터 없음: {cardId}");
                    continue;
                }

                cardDataList.Add((pos, monsterCardData.nameDesc, monsterCardData.card_id));
            }

            Debug.Log($"Level ID: {randomLevelData.levelId}. 등장 카드들 {string.Join(", ", cardDataList.Select(x => $"{x.Item1}: {x.Item2}({x.Item3})").ToList())}");
        }

        public void GenerateMap()
        {
            ActData actData = actId == 0 ? ActData.GetActDatas().First() : ActData.GetActData(actId);
            AreaData areaData = areaId == 0 ? AreaData.areaDatas.First() : AreaData.GetAreaDataFromFloor(floor);

            int floorStart = 1;
            int floorEnd = Random.Range(actData.mapSizeMin, actData.mapSizeMax);
            int floorCount = floorEnd - floorEnd + 1;

            Debug.Log($"GenerateMap areaID: {areaData.areaID}, floorStart: {floorStart}, floorEnd: {floorEnd} floorCount: {floorCount}, eventIds: {string.Join(", ", actData.eventIds)}");

            Dictionary<int, List<EventType>> depthEventsMap = new();

            for (int i = 0; i < actData.eventIds.Count; i++)
            {
                int eventsId = actData.eventIds[i];

                // 맵 이벤트 순서대로 필요한 이벤트 뽑기
                EventsData eventsData = EventsData.GetEventsData(eventsId);

                // 이벤트 개수 뽑기
                int eventCount = Random.Range(eventsData.minValue, eventsData.maxValue + 1);
                // Debug.Log($"<color={Colors.Yellow}>{eventsId} 이벤트</color>는 <color={Colors.Red}>{eventCount}</color>개 생성됩니다.");

                // 이벤트 배치할 층 뽑기 (excludeFloors, 마지막 층 제외)
                List<int> availableFloors = Enumerable.Range(1, floorEnd - 1).Except(eventsData.excludeFloors).ToList();

                if (availableFloors.Count == 0)
                {
                    Debug.LogError($"<color={Colors.Yellow}>{eventsId} 이벤트</color>는 모든 층이 제외되었습니다.");
                    continue;
                }

                // 층 리스트에서 랜덤으로 뽑기
                for (int j = 0; j < eventCount; j++)
                {
                    int randomFloor = 0;

                    if (j < eventsData.includeFloors.Count)
                    {
                        randomFloor = eventsData.includeFloors[j];
                        // Debug.Log($"<color={Colors.Yellow}>{eventsId} 이벤트</color>는 <color={Colors.Green}>{randomFloor}</color> 층에 고정되었습니다.");
                    }
                    else
                    {
                        randomFloor = availableFloors[Random.Range(0, availableFloors.Count)];
                        // Debug.Log($"<color={Colors.Yellow}>{eventsId} 이벤트</color>는 <color={Colors.Green}>{randomFloor}</color> 층에 랜덤으로 배치되었습니다.");
                    }

                    if (depthEventsMap.TryGetValue(randomFloor, out List<EventType> eventDatas))
                        eventDatas.Add(eventsData.eventType);
                    else
                        depthEventsMap.Add(randomFloor, new List<EventType> { eventsData.eventType });
                }
            }

            // var sortedDepthEventsMap = depthEventsMap.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            // foreach (var kvp in sortedDepthEventsMap)
            // {
            //     int d = kvp.Key;
            //     List<EventType> events = kvp.Value;
            //     Debug.Log($"확인 {Utils.FloorText(d)} 이벤트 <color={Colors.Red}>{events.Count()}</color>개 {string.Join(", ", events)}");
            // }

            int seed = random.Next(int.MinValue, int.MaxValue);

            System.Random seedRand = new(seed);
            System.Random genRand = new(seed + "gen".GetHashCode());

            for (int d = floorStart; d <= floorEnd; d++)
            {
                // 마지막 층은 보스 층 설정
                if (d == floorEnd)
                {
                    depthWidths[d] = 1;
                    MapLocation loc = new(seedRand.Next(), d, 0, EventType.BossBattle);
                    locations.Add(loc.ID, loc);

                    continue;
                }

                // 층별 위치 개수 랜덤 뽑기
                int locationCount = genRand.Next(actData.widthMin, actData.widthMax + 1);
                depthWidths[d] = locationCount;

                // 해당 층의 이벤트가 있는 경우
                if (depthEventsMap.TryGetValue(d, out List<EventType> events))
                {
                    // Debug.Log($"{Utils.FloorText(d)} 위치는 <color={Colors.Yellow}>{depthWidths[d]}</color> 개. 이벤트는 <color={Colors.Red}>{events.Count}</color>개 {string.Join(", ", events)}");

                    // 이벤트 개수가 뽑힌 위치 개수보다 많으면 위치 개수를 이벤트 개수로 변경
                    if (events.Count > locationCount)
                    {
                        depthWidths[d] = locationCount = events.Count;
                    }
                    else
                    {
                        // 이벤트 개수가 적으면 locationCount 개수 만큼 events 에 EventType.None 추가
                        for (int i = events.Count; i < locationCount; i++)
                            events.Add(EventType.None);
                    }

                    // 이벤트 랜덤 섞기
                    Utils.Shuffle(events);

                    // Debug.Log($"{Utils.FloorText(d)} 이벤트 랜덤 섞기: {string.Join(", ", events)}");

                    for (int i = 0; i < events.Count; i++)
                    {
                        EventType locationEvent = events[i];
                        // Debug.Log($"{Utils.FloorText(d)} {i}번째 위치에 <color={Colors.Red}>{locationEvent}</color> 이벤트 배치");

                        MapLocation loc = new(seedRand.Next(), d, i, locationEvent);
                        locations.Add(loc.ID, loc);
                    }
                }
                else
                {
                    for (int i = 0; i < locationCount; i++)
                    {
                        MapLocation loc = new(seedRand.Next(), d, i);
                        locations.Add(loc.ID, loc);
                    }
                }
            }

            // 층 연결 설정
            for (int d = floorStart; d <= floorEnd - 1; d++)
            {
                int depth = d;
                int nextDepth = d + 1;
                int currLocationCount = depthWidths[depth];
                int nextLocationCount = depthWidths[nextDepth];
                int locationCount = Mathf.Max(currLocationCount, nextLocationCount);
                int offset = (currLocationCount - locationCount) / 2;
                int start = -Mathf.Abs(offset);

                bool prevDiag = false;
                double diagProb = 0.25f;

                for (int i = start; i <= locationCount; i++)
                {
                    int ci = Mathf.Clamp(i, 0, currLocationCount - 1);
                    MapLocation loc = GetLocation(depth, ci);

                    int niVal = i + offset;
                    int ni = Mathf.Clamp(niVal, 0, nextLocationCount - 1);

                    if (ni != niVal)
                        prevDiag = true;

                    // Straight Link
                    MapLocation adj = GetLocation(nextDepth, ni);

                    if (!loc.IsAdjacent(adj))
                    {
                        loc.AddAdjacent(adj, "직선");

                        // Link to left
                        if (!prevDiag && ni > 0 && genRand.NextDouble() < diagProb)
                        {
                            MapLocation adjL = GetLocation(nextDepth, ni - 1);
                            loc.AddAdjacent(adjL, "왼쪽");
                        }

                        // Link to right
                        prevDiag = false;

                        if (i <= niVal && ni < (nextLocationCount - 1) && genRand.NextDouble() < diagProb)
                        {
                            MapLocation adjR = GetLocation(nextDepth, ni + 1);
                            loc.AddAdjacent(adjR, "오른쪽");
                            prevDiag = true;
                        }
                    }

                    // Debug.Log($"loc.adjacency.Count: {loc.adjacency.Count}, loc.directions.Count: {loc.directions.Count}");

                    // for (int j = 0; j < loc.directions.Count; j++)
                    //     Debug.Log($"Depth {depth} locId: {loc.ID} Index {ci} : {loc.directions[j].Item2} -> {GetLocation(loc.directions[j].Item1).ID}");
                }
            }

            SaveMapData();
        }

        public MapLocation GetLocation(int locationId)
        {
            if (locations.ContainsKey(locationId))
                return locations[locationId];
            return null;
        }

        public MapLocation GetLocation(int d, int i)
        {
            int id = MapLocation.GetID(d, i);
            return GetLocation(id);
        }

        private void SaveMapData()
        {
            AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

            PlayerPrefsUtility.SetInt("ActId", actId);
            PlayerPrefsUtility.SetInt("AreaId", areaData.areaID);
            PlayerPrefsUtility.SetInt("Floor", floor);
            PlayerPrefsUtility.SetInt("MaxFloor", maxFloor);

            // Save depthWidth
            string depthWidthJson = JsonConvert.SerializeObject(depthWidths, Formatting.None);
            PlayerPrefsUtility.SetString("DepthWidths", depthWidthJson);

            // Save locations
            string locationsJson = JsonConvert.SerializeObject(locations);
            PlayerPrefsUtility.SetString("Locations", locationsJson);

            PlayerPrefsUtility.Save();

            Debug.Log($"Saved floor Count: {depthWidths.Count}, locations: {locations.Count}");
        }

        private bool LoadMapData()
        {
            // Load depthWidth
            string depthWidthsJson = PlayerPrefsUtility.GetString("DepthWidths", null);

            if (string.IsNullOrEmpty(depthWidthsJson))
                return false;

            depthWidths = JsonConvert.DeserializeObject<Dictionary<int, int>>(depthWidthsJson);

            // Load locations
            string locationsJson = PlayerPrefsUtility.GetString("Locations", null);

            if (string.IsNullOrEmpty(locationsJson))
                return false;

            locations = JsonConvert.DeserializeObject<Dictionary<int, MapLocation>>(locationsJson);

            Debug.Log($"Loaded depthWidths: {depthWidths.Count}, locations: {locations.Count}");

            string seletedDepthIndiesJson = PlayerPrefsUtility.GetString("SelectedDepthIndies", null);

            if (!string.IsNullOrEmpty(seletedDepthIndiesJson))
                selectedDepthIndies = JsonConvert.DeserializeObject<Dictionary<int, int>>(seletedDepthIndiesJson);

            return true;
        }

        private LevelData GetRandomLevelData(List<LevelData> levelDatas)
        {
            float totalRatio = levelDatas.Sum(x => x.spawnRatio);
            float randomValue = Random.Range(0, totalRatio);
            float cumulativeRatio = 0;

            foreach (var levelData in levelDatas)
            {
                cumulativeRatio += levelData.spawnRatio;

                if (randomValue < cumulativeRatio)
                    return levelData;
            }

            // 기본적으로 첫 번째 levelData 반환 (안전장치)
            return levelDatas[0];
        }
    }
}