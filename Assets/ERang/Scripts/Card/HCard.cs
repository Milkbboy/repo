using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace ERang
{
    /// <summary>
    /// 핸드에 있는 카드를 표현하는 UI 컴포넌트
    /// </summary>
    public class HCard : CardView
    {
        public LayerMask slotLayerMask;
        public string LogText => Utils.CardLog(Card);

        private Dragable dragable;
        private string cardUid;

        protected override void Awake()
        {
            base.Awake();
            dragable = GetComponent<Dragable>();
        }

        protected override void OnMouseEnter()
        {
            if (HandDeck.Instance.DraggingCard != null)
                return;

            base.OnMouseEnter();
        }

        protected override void OnMouseExit()
        {
            if (HandDeck.Instance.DraggingCard != null)
                return;

            base.OnMouseExit();
        }

        void OnMouseDown()
        {
            HandDeck.Instance.SetDraggingCard(this);
        }

        void OnMouseUp()
        {
            HandDeck.Instance.SetDraggingCard(null);

            Debug.Log($"HCard. OnMouseUp - 1. {Card?.Uid} {LogText}, originalPosition: {originalPosition}");

            // 가장 가까운 슬롯을 찾고, 슬롯 위에 있는지 확인
            if (Card is MagicCard magicCard)
            {
                HandDeck.Instance.MagicCardUse(this);

                // IsSelectAttackType 속성이 현재 구현되어 있지 않아 임시로 주석 처리
                /*
                if (magicCard.IsSelectAttackType)
                    HandDeck.Instance.SetTargettingArraow(false);
                */
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

        /// <summary>
        /// 카드 데이터 설정 - BaseCard에서 GameCard로 변경
        /// </summary>
        public override void SetCard(GameCard card)
        {
            base.SetCard(card);
            cardUid = card.Uid;
        }

        public bool IsSelectAttackTypeCard()
        {
            return Card is MagicCard magicCard && magicCard.IsSelectAttackType;
        }

        public bool IsHandOnCard()
        {
            return Card is MagicCard magicCard && magicCard.IsHandOnCard;
        }

        public bool IsContainsSlotNum(int slotNum)
        {
            return Card is MagicCard magicCard && magicCard.TargetSlotNumbers.Contains(slotNum);
        }

        public void SetDrawPostion(Vector3 position)
        {
            transform.localPosition = position;
            originalPosition = transform.position;
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