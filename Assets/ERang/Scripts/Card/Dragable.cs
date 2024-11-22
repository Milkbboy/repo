using TMPro;
using UnityEngine;
using DG.Tweening;

namespace ERang
{
    public class Dragable : MonoBehaviour
    {
        /// <summary>
        /// 드래그 상태를 외부에서 확인할 수 있는 프로퍼티
        /// </summary>
        public bool IsDragging => isDragging;

        public float hoverHeight = 3f;
        public float animationDuration = 0.1f;
        public float scaleFactor = 1.1f;

        private bool isDragging = false;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        // 마우스 다운 시의 오프셋
        private Vector3 mouseOffset;

        private int originalSortingOrder;
        private int originalTextSortingOrder;

        private Renderer[] renderers;
        private TextMeshPro[] textMeshPros;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            textMeshPros = GetComponentsInChildren<TextMeshPro>(includeInactive: true);

            // TextMeshPro의 MeshRenderer를 제외한 Renderer 배열 생성
            renderers = System.Array.FindAll(renderers, r => !(r is MeshRenderer && r.GetComponent<TextMeshPro>() != null));

            // Debug.Log($"textMeshPros.Length: {textMeshPros.Length}");
        }

        void Start()
        {
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }

        void OnMouseDown()
        {
            isDragging = true;

            DeckSystem.Instance.SetDragginCard(GetComponent<HCard>());

            // 마우스 다운 시의 오프셋 계산
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mouseOffset = transform.position - objPosition;
        }

        void OnMouseDrag()
        {
            if (isDragging == false)
                return;

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = objPosition + mouseOffset;
        }

        void OnMouseUp()
        {
            isDragging = false;

            DeckSystem.Instance.SetDragginCard(null);

            transform.DOScale(originalScale, animationDuration);

            ResetSortingOrder();
        }

        void OnMouseEnter()
        {
            if (isDragging)
                return;

            transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
            transform.DOScale(originalScale * scaleFactor, animationDuration);

            // 모든 렌더러의 sortingOrder를 높게 설정
            foreach (var renderer in renderers)
            {
                originalSortingOrder = renderer.sortingOrder;
                renderer.sortingOrder = 1000; // 높은 값으로 설정하여 맨 앞으로 이동
            }

            // 모든 TextMeshPro의 sortingOrder를 높게 설정
            foreach (var textMeshPro in textMeshPros)
            {
                originalTextSortingOrder = textMeshPro.sortingOrder;
                textMeshPro.sortingOrder = 1001; // 높은 값으로 설정하여 맨 앞으로 이동
            }
        }

        void OnMouseExit()
        {
            if (isDragging)
                return;

            transform.DOMoveY(originalPosition.y, animationDuration);
            transform.DOScale(originalScale, animationDuration);

            ResetSortingOrder();
        }

        private void ResetSortingOrder()
        {
            // 모든 렌더러의 sortingOrder를 원래 값으로 복원
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = originalSortingOrder;
            }

            // 모든 TextMeshPro의 sortingOrder를 원래 값으로 복원
            foreach (var textMeshPro in textMeshPros)
            {
                textMeshPro.sortingOrder = originalTextSortingOrder;
            }
        }
    }
}