using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine.Client
{

    public class MapViewer : MonoBehaviour
    {
        public GameObject map_dot_prefab;
        public GameObject map_line_prefab;

        [Header("Layout")]
        public Transform scroll_trans;
        public float row_spacing = 1f;
        public float index_spacing = 1f;
        public float row_offset = 5f;

        [Header("Scrolling")]
        public float scroll_accel = 20f;
        public float scroll_speed_key = 8f;
        public float scroll_speed_mouse = 1f;
        public float scroll_speed_wheel = 40f;
        public float scroll_speed_auto = 4f;
        public float auto_scroll_offset = -3f;

        private string map_id;
        private bool is_generated = false;

        private float cam_width;
        private float max_scroll = 20f;
        private float tscroll = 0f;
        private Vector3 move_vect;
        private Vector3 drag_start;
        private float move_speed = 1f;

        private MapLocationDot move_target = null;
        private Vector3 move_target_offset;

        private List<MapLocationDot> map_locs = new List<MapLocationDot>();
        private List<MapPath> map_paths = new List<MapPath>();

        private static MapViewer instance;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            scroll_trans.localPosition = Vector3.zero;
            cam_width = Camera.main.orthographicSize * 2f * Camera.main.aspect;
        }

        public void GenerateMap()
        {
            is_generated = true;

            World world = GameClient.Get().GetWorld();
            Map map = world.GetMap(world.map_id);
            map_id = world.map_id;
            max_scroll = map.MapData.depth * row_spacing - cam_width + row_offset;
            max_scroll = Mathf.Max(max_scroll, 0f);

            foreach (KeyValuePair<int, MapLocation> pair in map.locations)
            {
                MapLocation loc = pair.Value;
                GameObject doto = Instantiate(map_dot_prefab, GetPosition(map, loc), Quaternion.identity);
                doto.transform.SetParent(scroll_trans);
                MapLocationDot dot = doto.GetComponent<MapLocationDot>();
                dot.map_id = map.map_id;
                dot.loc_id = loc.ID;
                map_locs.Add(dot);

                foreach (int adj in loc.adjacency)
                {
                    MapLocation adj_loc = map.GetLocation(adj);
                    Vector3 apos = GetPosition(map, adj_loc);
                    GameObject lineo = Instantiate(map_line_prefab, scroll_trans);
                    MapPath line = lineo.GetComponent<MapPath>();
                    line.SetSource(loc);
                    line.SetDest(adj_loc);
                    line.SetPosition(0, doto.transform.position);
                    line.SetPosition(1, apos);
                    map_paths.Add(line);
                }
            }

            MapLocationDot current = MapLocationDot.Get(world.map_location_id);
            if (current != null)
                MoveTo(current);
        }
        
        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            if (!is_generated)
                GenerateMap();

            bool drag = Input.GetMouseButton(0);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            move_vect = Vector3.zero;

            //Move mouse
            if (Input.GetMouseButtonDown(0))
                drag_start = worldPosition;

            if (drag)
            {
                float sspeed = scroll_speed_mouse;
                move_vect = (worldPosition - drag_start) * -sspeed;
                drag_start = worldPosition;
            }

            //Move keyboard
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                move_vect = Vector3.left * scroll_speed_key * Time.deltaTime;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                move_vect = Vector3.right * scroll_speed_key * Time.deltaTime;

            if (Input.mouseScrollDelta.magnitude > 0.1f)
                move_vect = Vector3.right * scroll_speed_wheel * Input.mouseScrollDelta.y * Time.deltaTime;

            //Auto move
            //if (move_vect.magnitude > 0.01f)
            //   move_target = null;

            if (move_target != null && max_scroll > 0.01f)
            {
                Vector3 dir = move_target.transform.localPosition + move_target_offset;
                dir.x = Mathf.Clamp(dir.x, 0f, max_scroll);
                move_vect = dir.normalized * scroll_speed_auto * move_speed * Time.deltaTime;
                if (tscroll >= (dir.x - 0.1f))
                    move_target = null;
            }

            //Do scrolling
            tscroll += move_vect.x;
            tscroll = Mathf.Clamp(tscroll, 0f, max_scroll);
            scroll_trans.position = Vector3.Lerp(scroll_trans.position, transform.position + Vector3.left * tscroll, scroll_accel * Time.deltaTime);
        }

        public void MoveTo(MapLocationDot point)
        {
            Vector3 teleport = point.transform.localPosition + Vector3.right * auto_scroll_offset * 2f;
            tscroll = teleport.x;
            MoveToward(point);
        }

        public void MoveToward(MapLocationDot point, float speed = 2f)
        {
            move_target = point;
            move_target_offset = Vector3.right * -auto_scroll_offset;
            move_speed = speed;
        }

        public Vector3 GetPosition(Map map, MapLocation loc)
        {
            Vector3 edge = transform.position - new Vector3(row_offset + row_spacing, 0f, 0f);
            int nb_index = map.depth_width[loc.depth];
            int half_index = nb_index / 2;
            float offset = half_index * index_spacing;
            if (nb_index % 2 == 0)
                offset -= index_spacing * 0.5f;

            Vector3 pos = edge + new Vector3(loc.depth * row_spacing, loc.index * index_spacing - offset, 0f);

            //Add Random offset
            System.Random rand = new System.Random(loc.seed);
            float ri = (float) rand.NextDouble() * index_spacing * 0.25f;
            float rd = (float) rand.NextDouble() * row_spacing * 0.25f;
            float a = (float) rand.NextDouble() * 360f * Mathf.Deg2Rad;
            pos += new Vector3(Mathf.Cos(a) * rd, Mathf.Sin(a) * ri, 0f);

            return pos;
        }

        public string GetMapID()
        {
            return map_id;
        }

        public bool HasChanged()
        {
            World world = GameClient.Get().GetWorld();
            return is_generated && map_id != world.map_id;
        }

        public static MapViewer Get()
        {
            return instance;
        }
    }
}
