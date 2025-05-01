using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace ERang
{
    // HandCard
    public class HCard : MonoBehaviour
    {
        public string CardUid => cardUid;
        public BaseCard Card => card;

        public LayerMask slotLayerMask;
        public string LogText => Utils.CardLog(card);

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

        void Start()
        {
            originalPosition = transform.position;
        }

        void OnMouseEnter()
        {
            if (HandDeck.Instance.DraggingCard != null)
                return;

            if (card == null)
                return;

            cardUI.ShowDesc(card.Id);
        }

        void OnMouseExit()
        {
            if (HandDeck.Instance.DraggingCard != null)
                return;

            if (card == null)
                return;

            cardUI.ShowShortDesc(card.Id);
        }

        void OnMouseDown()
        {
            // Debug.Log($"HCard. OnMouseDown. {card?.Uid} {card?.LogText}");

            HandDeck.Instance.SetDraggingCard(this);
        }

        void OnMouseUp()
        {
            HandDeck.Instance.SetDraggingCard(null);

            Debug.Log($"HCard. OnMouseUp - 1. {card?.Uid} {card?.LogText}, originalPosition: {originalPosition}");

            // 가장 가까운 슬롯을 찾고, 슬롯 위에 있는지 확인
            if (card is MagicCard magicCard)
            {
                HandDeck.Instance.MagicCardUse(this);

                if (magicCard.IsSelectAttackType)
                    HandDeck.Instance.SetTargettingArraow(false);
            }
            else
            {
                if (TryGetNearestSlot(transform.position, out BSlot nearestSlot))
                {
                    // 슬롯 위치로 이동
                    Debug.Log($"HCard. Nearest Slot: {nearestSlot.Index}, card: {nearestSlot.Card}");

                    if (nearestSlot.Card == null && BattleLogic.Instance.HandCardUse(this, nearestSlot))
                        return;
                }
            }

            transform.DOMove(originalPosition, .1f);
            // Debug.Log($"HCard. OnMouseUp - 2. {card?.Uid} {card?.LogText}, originalPosition: {originalPosition}");
        }

        // Gizmos를 사용하여 Scene 뷰에서 구체를 그립니다.
        void OnDrawGizmos()
        {
            // 드래그 중일 때만 구체를 그립니다.
            if (dragable != null && dragable.IsDragging)
            {
                Gizmos.color = Color.red; // 구체의 색상을 설정합니다.

                // HCard의 Collider를 가져옴
                if (TryGetComponent<Collider>(out var hCardCollider))
                {
                    // Collider의 중심과 크기를 사용하여 Box를 그림
                    Vector3 boxCenter = hCardCollider.bounds.center;
                    Vector3 boxSize = hCardCollider.bounds.extents * 2; // extents는 반지름이므로 크기로 변환
                    Quaternion boxOrientation = hCardCollider.transform.rotation;

                    // Draw the box used in Physics.OverlapBox
                    Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxOrientation, boxSize);
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
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

        public bool IsSelectAttackTypeCard()
        {
            return card is MagicCard magicCard && magicCard.IsSelectAttackType;
        }

        public bool IsHandOnCard()
        {
            return card is MagicCard magicCard && magicCard.IsHandOnCard;
        }

        public bool IsContainsSlotNum(int slotNum)
        {
            return card is MagicCard magicCard && magicCard.TargetSlotNumbers.Contains(slotNum);
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

        public bool IsDragging()
        {
            return dragable.IsDragging;
        }

        private bool TryGetNearestSlot(Vector3 position, out BSlot nearestSlot)
        {
            nearestSlot = null;
            float minDistance = float.MaxValue;

            // HCard의 Collider를 가져옴
            if (!TryGetComponent<Collider>(out var hCardCollider))
            {
                Debug.LogError("HCard Collider is null");
                return false;
            }

            // HCard의 Collider와 겹치는 모든 콜라이더를 가져옴
            Collider[] hitColliders = Physics.OverlapBox(hCardCollider.bounds.center, hCardCollider.bounds.extents, hCardCollider.transform.rotation, slotLayerMask);

            // Debug.Log($"hitColliders.Length: {hitColliders.Length}");

            foreach (Collider hitCollider in hitColliders)
            {
                BSlot bSlot = hitCollider.GetComponent<BSlot>();

                if (bSlot == null || bSlot.IsOverlapCard == false)
                    continue;

                float distance = Vector3.Distance(position, bSlot.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSlot = bSlot;
                }
            }

            Debug.Log($"position: {position}, nearestSlot: {nearestSlot?.Index}");

            return nearestSlot != null;
        }
    }
}