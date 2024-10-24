using System.Collections.Generic;

namespace ERang
{
    [System.Serializable]
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
}