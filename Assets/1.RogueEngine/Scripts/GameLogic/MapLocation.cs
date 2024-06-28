using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    [System.Serializable]
    public class MapLocation
    {
        public string map_id;
        public int seed;

        public int depth;
        public int index;

        public string evt_id;

        public bool explored;

        public List<int> adjacency = new List<int>();

        public int ID { get { return GetID(depth, index); } }

        public MapLocation(string map_id, int seed, int depth, int index, EventData evt)
        {
            this.map_id = map_id;
            this.seed = seed;
            this.depth = depth;
            this.index = index;
            this.evt_id = evt != null ? evt.id : "";
        }

        public void AddAdjacent(MapLocation loc)
        {
            if (loc != null && !adjacency.Contains(loc.ID))
                adjacency.Add(loc.ID);
        }

        public bool IsAdjacent(MapLocation loc)
        {
            return adjacency.Contains(loc.ID);
        }

        public EventData GetEvent()
        {
            return EventData.Get(evt_id);
        }

        public static int GetID(int d, int i)
        {
            return d *100 + i;
        }
    }
}
