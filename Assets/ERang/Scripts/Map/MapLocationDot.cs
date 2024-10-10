using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class MapLocationDot : MonoBehaviour
    {
        public string mapId;
        public int locationId;
        public int index;

        [Header("UI")]
        public SpriteRenderer circle;
        public SpriteRenderer highlightCurrent;
        public SpriteRenderer highlightNext;
        public SpriteRenderer icon;

        private static List<MapLocationDot> locations = new();

        void Awake()
        {
            locations.Add(this);
            highlightCurrent.enabled = false;
            highlightNext.enabled = false;
            // icon.enabled = false;
        }

        public void OnMouseDown()
        {
            Map.Instance.ClickLocation(locationId);
        }

        private void OnDestroy()
        {
            locations.Remove(this);
        }

        public void SetHightlight(int floor)
        {
            bool enabled = (locationId / 100) == floor;
            highlightNext.enabled = enabled;
        }

        public void SetHightlight(bool enabled)
        {
            highlightNext.enabled = enabled;
        }

        public void SetCurrent(bool enabled)
        {
            highlightCurrent.enabled = enabled;
        }

        public void SetAlpha(float alpha)
        {
            circle.color = new Color(circle.color.r, circle.color.g, circle.color.b, alpha);
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, alpha);
        }
    }
}