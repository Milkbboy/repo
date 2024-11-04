using UnityEngine;
using DG.Tweening;

namespace ERang
{
    public class MouseOver : MonoBehaviour
    {
        public bool IsDragging { set => isDragging = value; }

        public float hoverHeight = 0.2f;
        public float animationDuration = 0.2f;
        public float scaleFactor = 1.1f;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private bool isDragging;

        void Start()
        {
            isDragging = false;

            originalPosition = transform.position;
            originalScale = transform.localScale;
        }

        void OnMouseEnter()
        {
            if (isDragging)
                return;

            transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
            transform.DOScale(originalScale * scaleFactor, animationDuration);
        }

        void OnMouseExit()
        {
            if (isDragging)
                return;

            transform.DOMoveY(originalPosition.y, animationDuration);
            transform.DOScale(originalScale, animationDuration);
        }
    }
}