using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    public class MapLocationDot : MonoBehaviour
    {
        public string map_id;
        public int loc_id;

        [Header("UI")]
        public SpriteRenderer circle;
        public SpriteRenderer hightlight_current;
        public SpriteRenderer highlight_next;
        public SpriteRenderer icon;

        private static List<MapLocationDot> locations = new List<MapLocationDot>();

        void Awake()
        {
            locations.Add(this);
            hightlight_current.enabled = false;
            highlight_next.enabled = false;
            icon.enabled = false;
        }

        private void OnDestroy()
        {
            locations.Remove(this);
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            World world = GameClient.Get().GetWorld();
            Map map = GetMap();

            MapLocation current = world.GetCurrentLocation();
            MapLocation location = GetLocation();
            hightlight_current.enabled = current != null && current.ID == location.ID;
            highlight_next.enabled = world.CanMoveTo(location);

            icon.sprite = GetIcon();
            icon.enabled = icon.sprite != null;

            if (map.depth_width[location.depth] == 1)
                transform.localScale = Vector3.one * 1.25f;

            if (current != null)
            {
                bool valid = location.depth >= current.depth;
                if (location.depth == current.depth)
                    valid = location.index == current.index;

                SetAlpha(valid ? 1f : 0.5f);
            }
        }

        public void SetAlpha(float alpha)
        {
            circle.color = new Color(circle.color.r, circle.color.g, circle.color.b, alpha);
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, alpha);
        }

        public void OnMouseDown()
        {
            if (MapUI.Get().IsPanelOpen())
                return;

            World world = GameClient.Get().GetWorld();
            MapLocation location = GetLocation();
            if (location != null && world.CanMoveTo(location))
            {
                GameClient.Get().MapMove(location);
            }
        }

        public Sprite GetIcon()
        {
            MapLocation loc = GetLocation();
            EventData evt = loc?.GetEvent();
            if (evt != null)
            {
                return evt.icon;
            }
            return null;
        }

        public Map GetMap()
        {
            World world = GameClient.Get().GetWorld();
            return world.GetMap(map_id);
        }

        public MapLocation GetLocation()
        {
            World world = GameClient.Get().GetWorld();
            return world.GetLocation(map_id, loc_id);
        }

        public static MapLocationDot Get(int location_id)
        {
            foreach (MapLocationDot loc in locations)
            {
                if (loc.loc_id == location_id)
                    return loc;
            }
            return null;
        }

        public static List<MapLocationDot> GetAll()
        {
            return locations;
        }
    }

}
