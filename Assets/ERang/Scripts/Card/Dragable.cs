using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace ERang
{
    /// <summary>
    /// 카드의 드래그 동작과 호버 효과를 담당하는 컴포넌트
    /// 마우스 이벤트는 HCard에서 처리하고, 이 클래스는 순수하게 드래그/호버 로직만 담당
    /// </summary>
    public class Dragable : MonoBehaviour
    {
        // 드래그 거리 체크 결과를 이벤트로 전달
        public Action<float> OnDragDistanceChanged;

        /// <summary>
        /// 드래그 상태를 외부에서 확인할 수 있는 프로퍼티
        /// </summary>
        public bool IsDragging => isDragging;
        public Vector3 OriginalPosition => originalPosition;

        [Header("호버 효과 설정")]
        public float hoverHeight = 1f;
        public float animationDuration = 0.1f;
        public float scaleFactor = 1.5f;

        [Header("드래그 설정")]
        public float dragThreshold = 1f;

        private bool isDragging = false;
        private bool isCentered = false;
        private bool isHovering = false;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Vector3 mouseOffset;
        private float initialYPosition;

        // 렌더링 관련
        private int originalSortingOrder;
        private int originalTextSortingOrder;
        private Renderer[] renderers;
        private TextMeshPro[] textMeshPros;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            textMeshPros = GetComponentsInChildren<TextMeshPro>(includeInactive: true);

            // TextMeshPro의 MeshRenderer를 제외한 Renderer 배열 생성
            renderers = Array.FindAll(renderers, r => !(r is MeshRenderer && r.GetComponent<TextMeshPro>() != null));
        }

        void Start()
        {
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }

        /// <summary>
        /// HCard에서 호출: 드래그 시작
        /// </summary>
        public void StartDrag()
        {
            isDragging = true;
            initialYPosition = transform.position.y;

            // 마우스 다운 시의 오프셋 계산
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mouseOffset = transform.position - objPosition;

            Debug.Log($"Dragable.StartDrag: {transform.name}");
        }

        /// <summary>
        /// HCard에서 호출: 드래그 업데이트
        /// </summary>
        public void UpdateDrag()
        {
            if (!isDragging)
            {
                Debug.Log("UpdateDrag: not dragging");
                return;
            }

            if (isCentered)
            {
                Debug.Log("UpdateDrag: already centered");
                return;
            }

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = objPosition + mouseOffset;

            // 드래그 거리 체크
            float dragDistance = Mathf.Abs(transform.position.y - initialYPosition);
            OnDragDistanceChanged?.Invoke(dragDistance);
        }

        /// <summary>
        /// 카드를 중앙으로 이동 (타겟 선택 카드용)
        /// </summary>
        public void MoveCardToCenter()
        {
            isCentered = true;
            transform.DOMove(new Vector3(0, initialYPosition, originalPosition.z), animationDuration);
        }

        /// <summary>
        /// HCard에서 호출: 드래그 종료
        /// </summary>
        public void EndDrag()
        {
            isDragging = false;
            isCentered = false;

            // 🔧 타겟팅 화살표 끄는 로직 제거 (MagicCardUse에서 처리)
            // 타겟 선택이 유지되어야 MagicCardUse에서 사용할 수 있음

            // 크기 복원
            transform?.DOScale(originalScale, animationDuration);

            // 렌더링 순서 복원
            ResetSortingOrder();

            Debug.Log($"Dragable.EndDrag: {transform.name}");
        }

        /// <summary>
        /// HCard에서 호출: 호버 효과 시작
        /// </summary>
        public void StartHover()
        {
            if (isDragging || isHovering)
                return;

            isHovering = true;

            // 위치 및 크기 변경
            transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
            transform.DOScale(originalScale * scaleFactor, animationDuration);

            // 렌더링 순서 변경
            SetHighSortingOrder();

            Debug.Log($"Dragable.StartHover: {transform.name}");
        }

        /// <summary>
        /// HCard에서 호출: 호버 효과 종료
        /// </summary>
        public void EndHover()
        {
            if (isDragging || !isHovering)
                return;

            isHovering = false;

            // 원래 위치 및 크기로 복원
            transform.DOMoveY(originalPosition.y, animationDuration);
            transform.DOScale(originalScale, animationDuration);

            // 렌더링 순서 복원
            ResetSortingOrder();

            Debug.Log($"Dragable.EndHover: {transform.name}");
        }

        /// <summary>
        /// 원래 위치로 이동
        /// </summary>
        public void MoveToOriginalPosition()
        {
            transform.DOMove(originalPosition, animationDuration);
        }

        /// <summary>
        /// 높은 렌더링 순서 설정 (호버 시)
        /// </summary>
        private void SetHighSortingOrder()
        {
            foreach (var renderer in renderers)
            {
                originalSortingOrder = renderer.sortingOrder;
                renderer.sortingOrder = 1000;
            }

            foreach (var textMeshPro in textMeshPros)
            {
                originalTextSortingOrder = textMeshPro.sortingOrder;
                textMeshPro.sortingOrder = 1001;
            }
        }

        /// <summary>
        /// 렌더링 순서 복원
        /// </summary>
        private void ResetSortingOrder()
        {
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = originalSortingOrder;
            }

            foreach (var textMeshPro in textMeshPros)
            {
                textMeshPro.sortingOrder = originalTextSortingOrder;
            }
        }
    }
}