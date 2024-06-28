using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "MapData", menuName = "TcgEngine/MapData", order = 1)]
    public class MapData : ScriptableObject
    {
        public string id;
        public string title;

        [Header("Gameplay")]
        public string map_scene;
        public string battle_scene;

        [Header("Path Generation")]
        public int depth = 12;                  //How many locations the player need to travel through to finish the map, or number of rows
        public int width_min = 4;               //How many choices of path per row minimum
        public int width_max = 6;               //How many choices of path per row maximum
        public float fork_probability = 0.25f;  //Probability of locations forking to a different index, between 0.0 and 1.0

        [Header("Random Locations")]
        public EventData[] events;     //Probabilities of each location type

        [Header("Fixed Locations")]
        public FixedWidthData[] fixed_widths;         //Fixed width will always have the same number of locations in that row depth
        public FixedLocationData[] fixed_events;   //Fixed locations will always have the same type at specified depth and index

        public static List<MapData> map_list = new List<MapData>();

        public EventData GetLocationEvent(int depth, int index, System.Random rand)
        {
            FixedLocationData floc = GetFixedLocation(depth, index);
            if (floc != null)
                return floc.evt;

            List<EventData> valid_events = new List<EventData>();
            foreach (EventData evt in events)
            {
                if (depth >= evt.depth_min && depth <= evt.depth_max)
                    valid_events.Add(evt);
            }

            if (valid_events.Count > 0)
                return valid_events[rand.Next(0, valid_events.Count)];
            return null;
        }

        public FixedLocationData GetFixedLocation(int depth, int index)
        {
            foreach (FixedLocationData loc in fixed_events)
            {
                if (loc.depth == depth && index >= loc.index_min && index <= loc.index_max)
                    return loc;
            }
            return null;
        }

        public int GetFixedWidth(int depth)
        {
            foreach (FixedWidthData fw in fixed_widths)
            {
                if (fw.depth == depth)
                    return fw.width;
            }
            return 0;
        }

        public static void Load(string folder = "")
        {
            if (map_list.Count == 0)
                map_list.AddRange(Resources.LoadAll<MapData>(folder));
        }

        public static MapData Get(string id)
        {
            foreach (MapData map in GetAll())
            {
                if (map.id == id)
                    return map;
            }
            return null;
        }

        public static List<MapData> GetAll()
        {
            return map_list;
        }
    }

    [System.Serializable]
    public class FixedWidthData
    {
        public int depth;
        public int width;
    }

    [System.Serializable]
    public class FixedLocationData
    {
        public int depth;
        public int index_min;
        public int index_max;
        public EventData evt;
    }
}