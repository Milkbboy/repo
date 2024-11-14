using TMPro;
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
        private float originalZ;
        private float dragOffsetZ = 0.3f; // 카메라 쪽으로 이동할 거리
        private Vector3 originalScale;
        private float scaleFactor = 2f; // 드래그 중인 오브젝트의 크기 배율
        private int originalSortingOrder;
        private int originalTextSortingOrder;
        private Vector3 mouseOffset; // 마우스 다운 시의 오프셋

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

        void OnMouseDown()
        {
            isDragging = true;
            originalZ = transform.position.z; // 원래 z 좌표 저장
            originalScale = transform.localScale; // 원래 크기 저장

            // 마우스 다운 시의 오프셋 계산
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mouseOffset = transform.position - objPosition;

            // 크기를 키움
            transform.localScale = originalScale * scaleFactor;

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

        void OnMouseDrag()
        {
            if (isDragging == false)
                return;

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            // 드래그 중일 때 z 좌표를 카메라 쪽으로 약간 이동
            objPosition.z = originalZ - dragOffsetZ; // 원래 z 좌표에서 약간 이동

            transform.position = objPosition + mouseOffset;
        }

        void OnMouseUp()
        {
            isDragging = false;

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

            // 드래그가 끝나면 원래 z 좌표로 복원
            Vector3 position = transform.position;
            position.z = originalZ;
            transform.position = position;

            // 크기를 원래 크기로 복원
            transform.localScale = originalScale;
        }
    }
}