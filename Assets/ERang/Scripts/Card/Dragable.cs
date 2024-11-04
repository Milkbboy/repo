using UnityEngine;

namespace ERang
{
    public class Dragable : MonoBehaviour
    {
        /// <summary>
        /// 드래그 상태를 외부에서 확인할 수 있는 프로퍼티
        /// </summary>
        public bool IsDragging => isDragging;

        public LayerMask slotLayerMask;

        private Vector3 offset;
        private Vector3 originalPosition;
        private bool isDragging = false;

        void Start()
        {
            // 초기 위치 저장
            originalPosition = transform.position;
        }

        void OnMouseDown()
        {
            // 마우스를 클릭한 위치와 카드의 위치 간의 오프셋을 계산
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            offset = transform.position - objPosition;
            isDragging = true;

            // 마우스를 클릭했을 때의 위치 저장
            originalPosition = transform.position;
        }

        void OnMouseDrag()
        {
            if (!isDragging)
                return;

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = objPosition + offset;
        }

        void OnMouseUp()
        {
            isDragging = false;

            // 가장 가까운 슬롯을 찾고, 슬롯 위에 있는지 확인
            if (TryGetNearestSlot(transform.position, out BoardSlot nearestSlot))
            {
                // 슬롯 위치로 이동
                transform.position = nearestSlot.transform.position;
                Debug.Log($"Nearest Slot: {nearestSlot.Index}");

                // 보드 슬롯 카드 생성하고 핸드 카드는 삭제
                return;
                // nearestSlot.SetCard(GetComponent<BaseCard>());
            }

            // 원래 위치로 돌아가기
            transform.position = originalPosition;
        }

        private bool TryGetNearestSlot(Vector3 position, out BoardSlot nearestSlot)
        {
            nearestSlot = null;
            float minDistance = float.MaxValue;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, slotLayerMask);

            foreach (RaycastHit hit in hits)
            {
                BoardSlot slot = hit.collider.GetComponent<BoardSlot>();
                if (slot != null && slot.IsOverlapCard)
                {
                    float distance = Vector3.Distance(position, slot.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestSlot = slot;
                    }
                }
            }

            Debug.Log($"position: {position}, hits: {hits.Length}, nearestSlot: {nearestSlot?.Index}");

            return nearestSlot != null;
        }

        private bool IsOverSlot(out Vector3 slotPosition)
        {
            slotPosition = Vector3.zero;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, slotLayerMask))
            {
                slotPosition = hit.transform.position;
                return true;
            }

            return false;
        }
    }
}