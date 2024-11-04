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

        void OnMouseDown()
        {
            isDragging = true;
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
        }
    }
}