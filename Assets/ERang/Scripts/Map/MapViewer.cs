using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ERang
{
    public class MapViewer : MonoBehaviour
    {
        public GameObject mapDotPrefab;
        public GameObject mapPathPrefab;
        public Canvas mapCanvas;
        public TextMeshProUGUI floorTextPrefab;

        [Header("Layout")]
        public Transform scrollTrans;
        public float rowSpacing = 1f;
        public float indexSpacing = 1f;
        public float rowOffset = 5f;

        [Header("Scrolling")]
        public float scrollAccel = 20f;
        public float scrollSpeedKey = 8f;
        public float scrollSpeedMouse = 1f;
        public float scrollSpeedWheel = 40f;
        public float scrollSpeedAuto = 4f;
        public float autoScrollOffset = -3f;

        private Map map;
        private bool isGenerated = false;

        private float camWidth;
        private float maxScroll = 100f;
        private float tScroll = 0f;
        private Vector3 moveVect;
        private Vector3 dragStart;
        private float moveSpeed = 1f;

        private MapLocationDot moveTarget = null;
        private Vector3 moveTargetOffset;

        private List<MapLocationDot> mapLocs = new();
        private List<MapPath> mapPaths = new();

        private Dictionary<int, List<MapLocationDot>> mapLocationDic = new();

        void Awake()
        {
            map = GetComponent<Map>();
        }

        void Start()
        {
            scrollTrans.localPosition = Vector3.zero;
            camWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;
        }

        void Update()
        {
            bool drag = Input.GetMouseButton(0);

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            moveVect = Vector3.zero;

            // Move mouse
            if (Input.GetMouseButtonDown(0))
                dragStart = worldPosition;

            if (drag)
            {
                float sspeed = scrollSpeedMouse;
                moveVect = (worldPosition - dragStart) * -sspeed;
                dragStart = worldPosition;
            }

            // Move keyboard
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveVect = Vector3.left * scrollSpeedKey * Time.deltaTime;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveVect = Vector3.right * scrollSpeedKey * Time.deltaTime;

            if (Input.mouseScrollDelta.magnitude > 0.1f)
                moveVect = Vector3.right * scrollSpeedWheel * Input.mouseScrollDelta.y * Time.deltaTime;

            if (moveTarget != null && maxScroll > 0.01f)
            {
                Vector3 dir = moveTarget.transform.localPosition + moveTargetOffset;
                dir.x = Mathf.Clamp(dir.x, 0f, maxScroll);
                moveVect = dir.normalized * scrollSpeedAuto * moveSpeed * Time.deltaTime;

                if (tScroll >= (dir.x - 0.1f))
                    moveTarget = null;
            }

            // Do scrolling
            tScroll += moveVect.x;
            tScroll = Mathf.Clamp(tScroll, 0f, maxScroll);
            scrollTrans.position = Vector3.Lerp(scrollTrans.position, transform.position + Vector3.left * tScroll, scrollAccel * Time.deltaTime);
        }

        public void DrawMap(int floor, Dictionary<int, int> depthIndies)
        {
            Debug.Log($"DrawMap {map.locations.Count} floor {floor}");

            foreach (KeyValuePair<int, MapLocation> pair in map.locations)
            {
                MapLocation loc = pair.Value;

                GameObject doto = Instantiate(mapDotPrefab, GetPosition(map, loc), Quaternion.identity);
                doto.transform.SetParent(scrollTrans);

                MapLocationDot dot = doto.GetComponent<MapLocationDot>();
                // dot.mapId = map.mapId;
                dot.locationId = loc.ID;
                dot.index = loc.index;
                dot.SetHightlight(floor);

                mapLocs.Add(dot);

                if (mapLocationDic.TryGetValue(loc.depth, out List<MapLocationDot> locList))
                {
                    locList.Add(dot);
                }
                else
                {
                    locList = new List<MapLocationDot>() { dot };

                    mapLocationDic.Add(loc.depth, locList);
                }

                foreach (int adj in loc.adjacency)
                {
                    MapLocation adjLoc = map.GetLocation(adj);
                    Vector3 apos = GetPosition(map, adjLoc);
                    GameObject lineObject = Instantiate(mapPathPrefab, scrollTrans);
                    MapPath line = lineObject.GetComponent<MapPath>();
                    line.SetSource(loc);
                    line.SetDestination(adjLoc);
                    line.SetPosition(0, doto.transform.position);
                    line.SetPosition(1, apos);

                    mapPaths.Add(line);
                }
            }

            foreach (var depthEntry in depthIndies)
            {
                int depth = depthEntry.Key;
                int index = depthEntry.Value;

                if (mapLocationDic.TryGetValue(depth, out List<MapLocationDot> locList))
                    locList[index].SetCurrent(true);
            }

            // if (depthIndies.TryGetValue(loc.ID, out int floorIndex))
            // {
            //     locList[floorIndex].SetCurrent(true);
            //     // Debug.Log($"DrawMap Selected floorIndex: {floorIndex}");
            // }

            // 층 텍스트 표시
            DisplayFloorTexts();
        }

        public void SelectFloorIndex(int floor, int floorIndex)
        {
            mapLocationDic.TryGetValue(floor, out List<MapLocationDot> locList);

            if (locList == null)
            {
                Debug.LogWarning($"SelectFloorIndex: floor {floor} not found");
                return;
            }

            foreach (MapLocationDot dot in locList)
            {
                // Debug.Log($"SelectFloorIndex: floor {floor} index {floorIndex} dot {dot.locationId} index {dot.index}");
                dot.SetCurrent(dot.index == floorIndex);
                dot.SetHightlight(dot.index != floorIndex);
            }
        }

        public void MoveTo(MapLocationDot point)
        {
            Vector3 teleport = point.transform.localPosition + Vector3.right * autoScrollOffset * 2f;
            tScroll = teleport.x;
            MoveToward(point);
        }

        public void MoveToward(MapLocationDot point, float speed = 2f)
        {
            moveTarget = point;
            moveTargetOffset = Vector3.right * -autoScrollOffset;
            moveSpeed = speed;
        }

        public Vector3 GetPosition(Map map, MapLocation location)
        {
            Vector3 edge = transform.position - new Vector3(rowOffset + rowSpacing, 0f, 0f);

            int locationIndex = map.depthWidth[location.depth];
            int halfIndex = locationIndex / 2;
            float offset = halfIndex * indexSpacing;

            if (locationIndex % 2 == 0)
                offset -= indexSpacing * .5f;

            Vector3 position = edge + new Vector3(location.depth * rowSpacing, location.index * indexSpacing - offset, 0f);

            return position;
        }

        private void DisplayFloorTexts()
        {
            // 층의 위치를 계산합니다.
            Dictionary<int, Vector3> floorPositions = new Dictionary<int, Vector3>();

            foreach (KeyValuePair<int, MapLocation> pair in map.locations)
            {
                MapLocation loc = pair.Value;

                if (!floorPositions.ContainsKey(loc.depth))
                {
                    floorPositions[loc.depth] = GetPosition(map, loc);
                }
            }

            // 층 텍스트를 생성하고 배치합니다.
            foreach (KeyValuePair<int, Vector3> pair in floorPositions)
            {
                int floorNumber = pair.Key;
                Vector3 worldPosition = pair.Value;

                // 월드 좌표를 스크린 좌표로 변환합니다.
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);

                // 스크린 좌표를 캔버스 좌표로 변환합니다.
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mapCanvas.transform as RectTransform, screenPosition, Camera.main, out Vector2 canvasPosition);

                TextMeshProUGUI floorText = Instantiate(floorTextPrefab);
                floorText.text = $"{floorNumber}F";

                floorText.transform.SetParent(mapCanvas.transform, false);
                floorText.rectTransform.anchoredPosition = new Vector2(canvasPosition.x, -150); // 위치 조정

                // Debug.Log($"Floor {floorNumber} position: {canvasPosition}");
            }
        }
    }
}