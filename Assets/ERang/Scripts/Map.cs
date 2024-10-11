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

        public List<(int, string)> directions = new();
        public List<int> adjacency = new();

        public int ID { get { return GetID(depth, index); } }

        public MapLocation(int seed, int depth, int index)
        {
            this.seed = seed;
            this.depth = depth;
            this.index = index;
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
        public Dictionary<int, int> depthWidth = new();
        // 층에서 고른 인덱스 (표시용)
        public Dictionary<int, int> depthIndies = new();

        private int actId;
        private int areaId;
        private int floor;
        private int lastLocationId;
        private System.Random random = new();

        private MapViewer viewer;
        private int withMin;
        private int withMax;

        void Start()
        {
            Instance = this;

            actId = PlayerPrefsUtility.GetInt("ActId", 0);
            areaId = PlayerPrefsUtility.GetInt("AreaId", 0);
            floor = PlayerPrefsUtility.GetInt("Floor", 1);
            lastLocationId = PlayerPrefsUtility.GetInt("LastLocationId", 0);

            viewer = GetComponent<MapViewer>();
            withMin = 3;
            withMax = 5;

            AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

            LoadMapData();

            Debug.Log($"LoadMap Success. floor: {floor}, areaId: {areaId}, areaData.areaID: {areaData.areaID}, locations.Count: {locations.Count}");

            if (floor != 2 && (areaId != areaData.areaID || locations.Count == 0))
            {
                // 맵 새로 생성되면 처음 부터 시작
                lastLocationId = 0;
                GenerateMap();
            }

            foreach (var depthEntry in depthIndies)
            {
                int depth = depthEntry.Key;
                int index = depthEntry.Value;

                // Debug.Log($"Selected Depth: {depth}, Index: {index}");
            }

            viewer.DrawMap(floor, depthIndies);
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

            Debug.Log($"Click Location: {locationId}, lastLocationId: {lastLocationId}, floor: {floor}, floorIndex: {floorIndex}");

            if (locations.TryGetValue(lastLocationId, out MapLocation currentLoc))
            {
                if (!currentLoc.adjacency.Contains(locationId))
                {
                    Debug.LogError($"Not adjacent location: {locationId}");
                    return;
                }
            }

            viewer.SelectFloorIndex(floor, floorIndex);
            depthIndies[floor] = floorIndex;

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
            string depthIndiesJson = JsonConvert.SerializeObject(depthIndies);
            PlayerPrefsUtility.SetString("depthIndies", depthIndiesJson);

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

            string cardIdsString = $"등장 카드들 {string.Join(", ", cardDataList.Select(x => $"{x.Item1}: {x.Item2}({x.Item3})").ToList())}";

            Debug.Log($"Level ID: {randomLevelData.levelId}. {cardIdsString}");
        }

        public void GenerateMap()
        {
            ActData actData = actId == 0 ? ActData.GetActDatas().First() : ActData.GetActData(actId);
            AreaData areaData = areaId == 0 ? AreaData.areaDatas.First() : AreaData.GetAreaDataFromFloor(floor);

            int floorStart = areaData.floorStart;
            int floorEnd = areaData.floorMax;
            int floorCount = areaData.floorCount;

            // 1층 데이터면 2층꺼 AreaData로 변경
            if (floorStart == 1)
            {
                floorCount = AreaData.areaDatas[0].floorCount + AreaData.areaDatas[1].floorCount;
                floorEnd = AreaData.areaDatas[1].floorMax;
            }

            Debug.Log($"GenerateMap areaID: {areaData.areaID}, floorStart: {floorStart}, floorEnd: {floorEnd} floorCount: {floorCount}");

            int seed = random.Next(int.MinValue, int.MaxValue);

            System.Random seedRand = new(seed);
            System.Random genRand = new(seed + "gen".GetHashCode());

            for (int d = floorStart; d <= floorEnd; d++)
            {
                int locationCount = genRand.Next(withMin, withMax + 1);

                depthWidth[d] = locationCount;

                Debug.Log($"Depth {d} : {depthWidth[d]} locations: {locationCount}");

                for (int i = 0; i < locationCount; i++)
                {
                    MapLocation loc = new(seedRand.Next(), d, i);
                    locations.Add(loc.ID, loc);
                }
            }

            for (int d = floorStart; d <= floorEnd - 1; d++)
            {
                int depth = d;
                int nextDepth = d + 1;
                int currLocationCount = depthWidth[depth];
                int nextLocationCount = depthWidth[nextDepth];
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

            // Save depthWidth
            string depthWidthJson = JsonConvert.SerializeObject(depthWidth);
            PlayerPrefs.SetString("depthWidth", depthWidthJson);

            // Save locations
            string locationsJson = JsonConvert.SerializeObject(locations);
            PlayerPrefs.SetString("locations", locationsJson);

            PlayerPrefs.Save();

            Debug.Log($"Saved floor Count: {depthWidth.Count}, locations: {locations.Count}");
        }

        private bool LoadMapData()
        {
            // Load depthWidth
            string depthWidthJson = PlayerPrefs.GetString("depthWidth", null);

            if (string.IsNullOrEmpty(depthWidthJson))
                return false;

            depthWidth = JsonConvert.DeserializeObject<Dictionary<int, int>>(depthWidthJson);

            // Load locations
            string locationsJson = PlayerPrefs.GetString("locations", null);

            if (string.IsNullOrEmpty(locationsJson))
                return false;

            locations = JsonConvert.DeserializeObject<Dictionary<int, MapLocation>>(locationsJson);

            Debug.Log($"Loaded depthWidth: {depthWidth.Count}, locations: {locations.Count}");

            string depthIndiesJson = PlayerPrefs.GetString("depthIndies", null);

            if (!string.IsNullOrEmpty(depthIndiesJson))
                depthIndies = JsonConvert.DeserializeObject<Dictionary<int, int>>(depthIndiesJson);

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