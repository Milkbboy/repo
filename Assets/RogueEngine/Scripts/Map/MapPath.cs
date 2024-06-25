using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine.Client
{

    public class MapPath : MonoBehaviour
    {

        private MapLocation source_loc;
        private MapLocation dest_loc;

        private LineRenderer line; 

        void Awake()
        {
            line = GetComponent<LineRenderer>();
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;
            if (source_loc == null || dest_loc == null)
                return;

            World world = GameClient.Get().GetWorld();
            MapLocation current = world.GetCurrentLocation();
            if (current == null)
                return;

            bool valid = source_loc.depth >= current.depth;
            if (source_loc.depth == current.depth)
                valid = source_loc.index == current.index;

            SetAlpha(valid ? 0.9f : 0.4f);
        }

        public void SetSource(MapLocation loc)
        {
            source_loc = loc;
        }

        public void SetDest(MapLocation loc)
        {
            dest_loc = loc;
        }

        public void SetPosition(int index, Vector3 pos)
        {
            line.SetPosition(index, pos);
        }

        public void SetColor(Color color)
        {
            line.material.color = color;
        }

        public void SetAlpha(float alpha)
        {
            if(!Mathf.Approximately(line.material.color.a, alpha))
                line.material.color = new Color(line.material.color.r, line.material.color.g, line.material.color.b, alpha);
        }
    }
}
