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

        private AudioSource audioSource;
        private AudioClip clickSound;

        void Awake()
        {
            locations.Add(this);
            highlightCurrent.enabled = false;
            highlightNext.enabled = false;
        }

        void Start()
        {
            // AudioSource 컴포넌트를 추가하고 숨김니다.
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // 오디오 클립을 로드합니다.
            clickSound = Resources.Load<AudioClip>("Audio/MapClick");
        }

        public void OnMouseDown()
        {
            if (clickSound != null)
                audioSource.PlayOneShot(clickSound);
            else
                Debug.LogWarning("MapClick.mp3 파일을 찾을 수 없습니다.");

            MapLogic.Instance.ClickLocation(locationId);
        }

        private void OnDestroy()
        {
            locations.Remove(this);
        }

        public void SetIcon(EventType eventType)
        {
            string spritePath = eventType switch
            {
                EventType.Store => "Sprites/Shop",
                EventType.EliteBattle => "Sprites/Elite",
                EventType.RandomEvent => "Sprites/Random",
                EventType.BossBattle => "Sprites/Boss",
                _ => "Sprites/swords"
            };

            // Debug.Log($"spritePath: {spritePath}");

            Sprite sprite = Resources.Load<Sprite>(spritePath);

            if (sprite != null)
            {
                icon.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"Sprite not found at path: {spritePath}");
            }
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