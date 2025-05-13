using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ERang.Data;

namespace ERang
{
    public class MapSystem : MonoBehaviour
    {
        public Dictionary<int, MapLocation> locations = new();
        public Dictionary<int, int> depthWidths = new();

        private System.Random random = new();

        public void ClickTest()
        {
        }

        public MapLocation GetLocation(int locationId)
        {
            if (locations.TryGetValue(locationId, out MapLocation location))
                return location;

            return null;
        }

        public void GenerateMap(int actId)
        {
            ActData actData = ActData.GetActData(actId);

            if (actData == null)
            {
                Debug.LogError($"GenerateMap actId {actId} 데이터 없음");
                return;
            }

            int floorStart = 1;
            int floorEnd = Random.Range(actData.mapSizeMin, actData.mapSizeMax);
            int floorCount = floorEnd - floorEnd + 1;

            Debug.Log($"맵 생성 actId: {actId}, floorStart: {floorStart}, floorEnd: {floorEnd} floorCount: {floorCount}, eventIds: {string.Join(", ", actData.eventIds)}");

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

            // 생성된 맵 너비 저장
            string depthWidthJson = JsonConvert.SerializeObject(depthWidths, Formatting.None);
            PlayerPrefsUtility.SetString("DepthWidths", depthWidthJson);

            // 생성된 맵 위치 저장
            string locationsJson = JsonConvert.SerializeObject(locations);
            PlayerPrefsUtility.SetString("Locations", locationsJson);
        }

        private MapLocation GetLocation(int d, int i)
        {
            int id = MapLocation.GetID(d, i);
            return GetLocation(id);
        }
    }
}