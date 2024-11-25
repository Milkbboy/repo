using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    // HandCard
    public class HCard : MonoBehaviour
    {
        public string CardUid => cardUid;
        public BaseCard Card => card;

        public LayerMask slotLayerMask;
        public float detectionRadius = 1.0f; // 감지 반경

        private Dragable dragable;

        private BaseCard card;
        private CardUI cardUI;
        private string cardUid;

        private Vector3 originalPosition;

        void Awake()
        {
            dragable = GetComponent<Dragable>();
            cardUI = GetComponent<CardUI>();
        }

        void OnMouseEnter()
        { 
            cardUI.ShowDesc(card.Id);
        }

        void OnMouseExit()
        {
            cardUI.ShowShortDesc(card.Id);
        }

        void OnMouseUp()
        {
            // 가장 가까운 슬롯을 찾고, 슬롯 위에 있는지 확인
            if (TryGetNearestSlot(transform.position, out BSlot nearestSlot))
            {
                // 슬롯 위치로 이동
                Debug.Log($"Nearest Slot: {nearestSlot.Index}");

                if (BattleLogic.Instance.HandCardUse(this, nearestSlot))
                    return;
            }

            transform.position = originalPosition;
        }

        // Gizmos를 사용하여 Scene 뷰에서 구체를 그립니다.
        void OnDrawGizmos()
        {
            // 드래그 중일 때만 구체를 그립니다.
            if (dragable != null && dragable.IsDragging)
            {
                Gizmos.color = Color.red; // 구체의 색상을 설정합니다.
                Gizmos.DrawWireSphere(transform.position, detectionRadius); // 구체를 그립니다.
            }
        }

        public void SetCard(BaseCard card)
        {
            cardUid = card.Uid;
            this.card = card;

            // Debug.Log($"cardId: {card.Id} CardType: {card.CardType}, class type: {card.GetType()}");

            if (cardUI != null)
                cardUI.SetCard(card);
            else
                Debug.LogError("CardUI is null");
        }

        public void SetDrawPostion(Vector3 position)
        {
            transform.localPosition = position;
            originalPosition = transform.position;
        }

        public void GoBackPosition()
        {
            transform.position = originalPosition;
        }

        public void DiscardAnimation(Transform discardPos)
        {
            DiscardAnimation discardAnimation = GetComponent<DiscardAnimation>();
            discardAnimation.PlaySequence(discardPos);
        }

        public void UpdateCardUI()
        {
            cardUI.SetCard(card);
        }

        private bool TryGetNearestSlot(Vector3 position, out BSlot nearestSlot)
        {
            nearestSlot = null;
            float minDistance = float.MaxValue;

            // 감지 반경 내의 모든 콜라이더를 가져옴
            Collider[] hitColliders = Physics.OverlapSphere(position, detectionRadius, slotLayerMask);

            foreach (Collider hitCollider in hitColliders)
            {
                BSlot bSlot = hitCollider.GetComponent<BSlot>();

                if (bSlot == null || bSlot.IsOverlapCard == false || bSlot.Card == null)
                    continue;

                float distance = Vector3.Distance(position, bSlot.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSlot = bSlot;
                }
            }

            // Debug.Log($"position: {position}, nearestSlot: {nearestSlot?.Index}");

            return nearestSlot != null;
        }
    }
}