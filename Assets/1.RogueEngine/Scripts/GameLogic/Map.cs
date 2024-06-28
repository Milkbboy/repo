using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Contains data for a generated map

    [System.Serializable]
    public class Map
    {
        public string map_id;
        public int seed;

        public Dictionary<int, MapLocation> locations = new Dictionary<int, MapLocation>();
        public Dictionary<int, int> depth_width = new Dictionary<int, int>();

        public Map(MapData mdata, int seed) { map_id = mdata.id; this.seed = seed; }

        public MapData MapData { get { return MapData.Get(map_id); } }

        public virtual void GenerateMap(World world)
        {
            MapData mdata = MapData.Get(map_id);
            System.Random seed_rand = new System.Random(seed);                          //Use a separate rand to generate seeds, so they stay consistant across versioning if generation logic get changed
            System.Random gen_rand = new System.Random(seed + map_id.GetHashCode());    //Add arbitrary value so the two rand aren't generating the same numbers

            for (int d = 1; d <= mdata.depth; d++)
            {
                int nb_index = gen_rand.Next(mdata.width_min, mdata.width_max + 1);
                int fixed_width = mdata.GetFixedWidth(d);
                if (fixed_width > 0)
                    nb_index = fixed_width;

                depth_width[d] = nb_index;

                for (int i = 0; i < nb_index; i++)
                {
                    EventData evt = mdata.GetLocationEvent(d, i, gen_rand);
                    MapLocation loc = new MapLocation(map_id, seed_rand.Next(), d, i, evt);
                    locations.Add(loc.ID, loc);
                }
            }

            //Adjacency
            for (int d = 1; d <= mdata.depth - 1; d++)
            {
                int depth = d;
                int depth_next = d + 1;
                int nb_index_cur = depth_width[depth];                  //Number of locations in current row
                int nb_index_next = depth_width[depth_next];           //Number of locations in next row
                int nb_index = Mathf.Max(nb_index_cur, nb_index_next); //max number of locations in both rows
                int offset = (nb_index_next - nb_index_cur) / 2;        //Offset in index between current row and next one
                int start = -Mathf.Abs(offset);                      //Start looping in negative number if offset

                bool prev_diag = false; //Prevent overlapping lines
                double diag_prob = mdata.fork_probability;

                for (int i = start; i < nb_index; i++)
                {
                    int ci = Mathf.Clamp(i, 0, nb_index_cur - 1);
                    MapLocation loc = GetLocation(depth, ci);

                    int ni_val = i + offset;
                    int ni = Mathf.Clamp(ni_val, 0, nb_index_next - 1);
                    if (ni != ni_val)
                        prev_diag = true;

                    //Straight Link
                    MapLocation adj = GetLocation(depth_next, ni);
                    if (!loc.IsAdjacent(adj))
                    {
                        loc.AddAdjacent(adj);

                        //Link to left
                        if (!prev_diag && ni > 0 && gen_rand.NextDouble() < diag_prob)
                        {
                            MapLocation adjL = GetLocation(depth_next, ni - 1);
                            loc.AddAdjacent(adjL);
                        }

                        //Link to right
                        prev_diag = false;
                        if (i <= ni_val && ni < (nb_index_next - 1) && gen_rand.NextDouble() < diag_prob)
                        {
                            MapLocation adjR = GetLocation(depth_next, ni + 1);
                            loc.AddAdjacent(adjR);
                            prev_diag = true;
                        }
                    }
                }
            }
        }

        public MapLocation GetLocation(int loc_id)
        {
            if (locations.ContainsKey(loc_id))
                return locations[loc_id];
            return null;
        }

        public MapLocation GetLocation(int d, int i)
        {
            int id = MapLocation.GetID(d, i);
            return GetLocation(id);
        }

        public void AddExplored(int loc_id)
        {
            MapLocation loc = GetLocation(loc_id);
            if (loc != null)
                loc.explored = true;
        }

        public bool IsExplored(int loc_id)
        {
            MapLocation loc = GetLocation(loc_id);
            if (loc != null)
                return loc.explored;
            return false;
        }


    }

}