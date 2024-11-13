using UnityEngine;

namespace ERang
{
    public class Dragable : MonoBehaviour
    {
        /// <summary>
        /// 드래그 상태를 외부에서 확인할 수 있는 프로퍼티
        /// </summary>
        public bool IsDragging => isDragging;

        private bool isDragging = false;
        private int originalSortingOrder;
        private Renderer[] renderers;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        void OnMouseDown()
        {
            isDragging = true;

            // 모든 렌더러의 sortingOrder를 높게 설정
            foreach (var renderer in renderers)
            {
                originalSortingOrder = renderer.sortingOrder;
                renderer.sortingOrder = 1000; // 높은 값으로 설정하여 맨 앞으로 이동
            }
        }

        void OnMouseDrag()
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = objPosition;
        }

        void OnMouseUp()
        {
            isDragging = false;

            // 모든 렌더러의 sortingOrder를 원래 값으로 복원
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = originalSortingOrder;
            }
        }
    }
}