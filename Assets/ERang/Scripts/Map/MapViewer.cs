using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class MapViewer : MonoBehaviour
    {
        public GameObject mapDotPrefab;
        public GameObject mapPathPrefab;

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
        private float maxScroll = 20f;
        private float tScroll = 0f;
        private Vector3 moveVect;
        private Vector3 dragStart;

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

        public void DrawMap(int floor)
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
    }
}