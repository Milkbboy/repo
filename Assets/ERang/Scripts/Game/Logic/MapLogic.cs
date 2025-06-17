using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ERang.Data;

namespace ERang
{
    public class MapLogic : MonoBehaviour
    {
        public static MapLogic Instance { get; private set; }

        public int actId;
        public int floor;
        public int selectLocationId;
        public int lastLocationId;
        public int levelId;
        public MapLocation selectLocation;

        private MapSystem mapSystem;
        private MapViewer mapViewer;

        private AudioSource audioSource;
        private AudioClip backSound;

        private Dictionary<int, int> selectedDepthIndies = new();
        /// <summary>
        /// 뽑힌 랜덤 이벤트 리스트. 뽑힐 확률 조정을 위해 저장
        /// eventId
        /// </summary>
        private List<int> randomEvents = new();

        void Awake()
        {
            Instance = this;

            mapSystem = GetComponent<MapSystem>();
            mapViewer = GetComponent<MapViewer>();

            actId = PlayerDataManager.GetValue(PlayerDataKeys.ActId);
            floor = PlayerDataManager.GetValue(PlayerDataKeys.Floor);
            lastLocationId = PlayerDataManager.GetValue(PlayerDataKeys.LastLocationId);

            string seletedDepthIndiesJson = PlayerDataManager.GetValue(PlayerDataKeys.SelectedDepthIndies);

            if (!string.IsNullOrEmpty(seletedDepthIndiesJson))
                selectedDepthIndies = JsonConvert.DeserializeObject<Dictionary<int, int>>(seletedDepthIndiesJson);

            // 맵 랜덤 이벤트
            string randomEventsJson = PlayerDataManager.GetValue(PlayerDataKeys.RandomEvents);

            if (!string.IsNullOrEmpty(randomEventsJson))
                randomEvents = JsonConvert.DeserializeObject<List<int>>(randomEventsJson);
        }

        void Start()
        {
            // AudioSource 컴포넌트를 추가하고 숨김니다.
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;

            backSound = Resources.Load<AudioClip>("Audio/8-bit-arcade-music");

            // 오디오를 재생합니다.
            if (backSound != null)
            {
                audioSource.volume = 0.2f;
                audioSource.clip = backSound;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("backSound 파일을 찾을 수 없습니다.");
            }

            if (!LoadMapData())
            {
                Debug.Log($"맵 데이터 로드 실패. 맵 생성. actId: {actId}");
                if (actId == 0)
                    actId = ActData.GetFirstActDataId();

                mapSystem.GenerateMap(actId);
            }

            mapViewer.DrawMap(floor, selectedDepthIndies);
        }

        public void ClickLocation(int locationId)
        {
            levelId = 0;

            int floor = locationId / 100;
            int floorIndex = locationId % 100;

            selectLocation = mapSystem.GetLocation(locationId);

            if (selectLocation == null)
            {
                Debug.LogError($"{Utils.MapLocationText(locationId)} 클릭 <color={Colors.Green}>{selectLocationId}</color> 데이터 없음");
                return;
            }

            selectLocationId = locationId;

            // 층과 선택한 위치 인덱스 저장
            selectedDepthIndies[floor] = floorIndex;

            // string selectedDepthIndiesJson = JsonConvert.SerializeObject(selectedDepthIndies, Formatting.None);
            // Dictionary<int, int> selectedDepthIndies2 = JsonConvert.DeserializeObject<Dictionary<int, int>>(selectedDepthIndiesJson);
            // Debug.Log($"selectedDepthIndies: {string.Join(", ", selectedDepthIndies)}, selectedDepthIndiesJson: {selectedDepthIndiesJson}, selectedDepthIndies2: {string.Join(", ", selectedDepthIndies2)}");

            Debug.Log($"{Utils.MapLocationText(locationId)} 클릭 <color={Colors.Red}>{selectLocationId}</color>({selectLocation.eventType}) 클릭. lastLocationId: {lastLocationId}");

            // 층 확인
            if (floor != this.floor)
            {
                Debug.LogError($"층이 다름. 현재 층: {this.floor}, 클릭 층: {floor}");
                return;
            }

            // 층 연결 확인
            MapLocation lastLocation = mapSystem.GetLocation(lastLocationId);

            if (lastLocation != null && !lastLocation.adjacency.Contains(selectLocationId))
            {
                Debug.LogError($"맵 위치 <color={Colors.Green}>{selectLocationId}</color> 는 마지막 위치 <color={Colors.Green}>{lastLocationId}</color> 와 연결되지 않음.");
                return;
            }

            // 선택 위치 표시
            mapViewer.SelectFloorIndex(floor, floorIndex);

            // 일반 배틀
            if (selectLocation.eventType == EventType.None)
            {
                AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

                levelId = LevelGroupData.GetRandomLevelId(areaData.levelGroupId);

                Debug.Log($"{Utils.FloorText(floor)} 일반 배틀 레벨 ID: {levelId}");
            }

            // 엘리트 배틀
            if (selectLocation.eventType == EventType.EliteBattle)
            {
                ActData actData = ActData.GetActData(actId);

                EventsData eventData = EventsData.FindEliteBattleEventsData(actData.eventIds);

                if (eventData == null)
                {
                    Debug.LogError($"{actId} 액트 {string.Join(", ", actData.eventIds)} 이벤트 중 엘리트 배틀 이벤트 데이터 없음");
                    return;
                }

                levelId = LevelGroupData.GetRandomLevelId(eventData.eliteBattleLevelGroupID);
            }

            // 보스 배틀
            if (selectLocation.eventType == EventType.BossBattle)
            {
                levelId = GetNormalBattleLevelId(floor);
            }

            // 랜덤 이벤트
            if (selectLocation.eventType == EventType.RandomEvent)
            {
                // 랜덤 이벤트 중 하나 뽑기
                RandomEventsData randomEventData = RandomEventsData.SelectRandomEvent(randomEvents);

                // 뽑힌 랜덤 이벤트 추가
                randomEvents.Add(randomEventData.randomEventsID);

                // 전투 이벤트인 경우 배틀 씬으로 전환
                if (randomEventData.eventType == RandomEventType.RandomBattleFix)
                {
                    levelId = LevelGroupData.GetRandomLevelId(randomEventData.levelGroupId);
                }
                else if (randomEventData.eventType == RandomEventType.RandomBattle)
                {
                    levelId = GetNormalBattleLevelId(floor);
                }
            }
        }

        public void ClickStart()
        {
            SaveMapData();

            NextScene(levelId == 0 ? "Event" : "Battle");

            PlayerDataManager.SetValue(PlayerDataKeys.LastScene, "Map");
        }

        public void NextScene(string sceneName)
        {
            GameObject nextSceneObject = GameObject.Find("SceneManager");

            if (nextSceneObject.TryGetComponent<NextScene>(out NextScene nextScene))
                nextScene.Play(sceneName);
        }

        public MapLocation GetLocation(int locationId)
        {
            return mapSystem.GetLocation(locationId);
        }

        public MapLocation GetLocation(int d, int i)
        {
            int id = MapLocation.GetID(d, i);
            return GetLocation(id);
        }

        private bool LoadMapData()
        {
            // floor 로드
            floor = PlayerDataManager.GetValue(PlayerDataKeys.Floor);

            // 선택한 위치 인덱스 리스트 로드
            // selectedDepthIndies: [3, 0], [4, 0], selectedDepthIndiesJson: {"3":0,"4":0}, selectedDepthIndies2: [3, 0], [4, 0]
            string selectedDepthIndiesJson = PlayerDataManager.GetValue(PlayerDataKeys.SelectedDepthIndies);
            selectedDepthIndies = JsonConvert.DeserializeObject<Dictionary<int, int>>(selectedDepthIndiesJson);

            Debug.Log($"selectedDepthIndies: {string.Join(", ", selectedDepthIndies)}, selectedDepthIndiesJson: {selectedDepthIndiesJson}");

            // Load depthWidth
            string depthWidthsJson = PlayerDataManager.GetValue(PlayerDataKeys.DepthWidths);

            if (string.IsNullOrEmpty(depthWidthsJson))
                return false;

            mapSystem.depthWidths = JsonConvert.DeserializeObject<Dictionary<int, int>>(depthWidthsJson);

            // Load locations
            string locationsJson = PlayerDataManager.GetValue(PlayerDataKeys.Locations);

            if (string.IsNullOrEmpty(locationsJson))
                return false;

            // 맵 위치
            mapSystem.locations = JsonConvert.DeserializeObject<Dictionary<int, MapLocation>>(locationsJson);

            Debug.Log($"맵 로드 floor: {floor}, depthWidths: {mapSystem.depthWidths.Count}, locations: {mapSystem.locations.Count}");

            return true;
        }

        private void SaveMapData()
        {
            // 선택한 위치
            string selectLocationJson = JsonConvert.SerializeObject(selectLocation, Formatting.None);
            // 선택한 위치 인덱스 리스트
            string selectedDepthIndiesJson = JsonConvert.SerializeObject(selectedDepthIndies, Formatting.None);
            // 뽑힌 랜덤 이벤트
            string randomEventsJson = JsonConvert.SerializeObject(randomEvents, Formatting.None);

            PlayerDataManager.SetValues(
                (PlayerDataKeys.ActId, actId),
                (PlayerDataKeys.Floor, floor),
                (PlayerDataKeys.LevelId, levelId),
                (PlayerDataKeys.LastLocationId, selectLocationId),
                (PlayerDataKeys.SelectLocation, selectLocationJson),
                (PlayerDataKeys.SelectedDepthIndies, selectedDepthIndiesJson),
                (PlayerDataKeys.RandomEvents, randomEventsJson)
            );

            Debug.Log($"맵 저장 {actId} 액트 {floor} 층, lastLocationId: {selectLocationId}");
        }

        /// <summary>
        /// 층에 맞는 일반 배틀 랜덤 레벨 아이디 얻기
        /// </summary>
        /// <param name="floor">층</param>
        /// <returns></returns>
        private int GetNormalBattleLevelId(int floor)
        {
            AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

            return LevelGroupData.GetRandomLevelId(areaData.levelGroupId);
        }
    }
}