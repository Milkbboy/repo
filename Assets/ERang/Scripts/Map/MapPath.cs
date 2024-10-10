using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class MapPath : MonoBehaviour
    {
        private MapLocation sourceLocation;
        private MapLocation destinationLocation;

        private LineRenderer line;

        void Awake()
        {
            line = GetComponent<LineRenderer>();
        }

        public void SetSource(MapLocation location)
        {
            sourceLocation = location;
        }

        public void SetDestination(MapLocation location)
        {
            destinationLocation = location;
        }

        public void SetPosition(int index, Vector3 position)
        {
            line.SetPosition(index, position);
        }

        public void SetColor(Color color)
        {
            line.material.color = color;
        }

        public void SetAlpha(float alpha)
        {
            if (!Mathf.Approximately(line.material.color.a, alpha))
                line.material.color = new Color(line.material.color.r, line.material.color.g, line.material.color.b, alpha);
        }
    }
}