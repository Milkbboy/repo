// 수정된 HCard.cs - 모든 마우스 이벤트 통합 관리
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

namespace ERang
{
    public class HCard : MonoBehaviour
    {
        public string CardUid => cardUid;
        public BaseCard Card => card;
        public LayerMask slotLayerMask;

        private Dragable dragable;

        private BaseCard card;
        private CardUI cardUI;
        private string cardUid;
        private GameObject gradeCommon;
        private GameObject gradeRare;
        private GameObject gradeLegend;

        private Vector3 originalPosition;
        private bool hasTriggeredTargetSelection = false;

        void Awake()
        {
            dragable = GetComponent<Dragable>();
            cardUI = GetComponent<CardUI>();

            dragable.OnDragDistanceChanged += OnDragDistanceChanged;

            gradeCommon = transform.Find("Grade_01_Common")?.gameObject;
            gradeRare = transform.Find("Grade_02_Rare")?.gameObject;
            gradeLegend = transform.Find("Grade_03_Legend")?.gameObject;
        }

        void OnDestroy()
        {
            dragable.OnDragDistanceChanged -= OnDragDistanceChanged;
        }

        void Start()
        {
            originalPosition = transform.position;
        }

        private void OnDragDistanceChanged(float dragDistance)
        {
            // 임계값 도달 시 한 번만 실행
            if (dragDistance >= dragable.dragThreshold && !hasTriggeredTargetSelection)
            {
                HandleTargetSelection();
                hasTriggeredTargetSelection = true;
            }
        }

        private void HandleTargetSelection()
        {
            if (card is MagicCard magicCard && magicCard.IsSelectAttackType)
            {
                Debug.Log($"Target select card detected! Moving to center");
                dragable.MoveCardToCenter();
                HandDeck.Instance.SetTargettingArraow(true);
            }
        }

        void OnMouseEnter()
        {
            if (HandDeck.Instance.DraggingCard != null)
                return;

            if (card == null)
                return;

            cardUI.ShowDesc(card.Id);

            // Dragable의 호버 효과 호출
            dragable.StartHover();
        }

        void OnMouseExit()
        {
            if (HandDeck.Instance.DraggingCard != null)
                return;

            if (card == null)
                return;

            cardUI.ShowShortDesc(card.Id);

            // Dragable의 호버 효과 종료
            dragable.EndHover();
        }

        void OnMouseDown()
        {
            hasTriggeredTargetSelection = false;

            // 핸드 온 카드 드래깅 안되게 처리
            if (IsHandOnCard())
                return;

            Debug.Log($"HCard. OnMouseDown. {card?.ToCardLogInfo()}");

            // 1. 드래깅 카드 설정 (다른 카드 호버 방지용)
            HandDeck.Instance.SetDraggingCard(this);

            // 2. Dragable 컴포넌트에 드래그 시작 알림
            dragable.StartDrag();
        }

        void OnMouseDrag()
        {
            // Debug.Log($"HCard.OnMouseDrag called for {card?.LogText}");
            // Dragable 컴포넌트에 드래그 업데이트 전달
            dragable.UpdateDrag();
        }

        void OnMouseUp()
        {
            Debug.Log($"HCard. OnMouseUp. {card?.ToCardLogInfo()}");

            // 1. Dragable 컴포넌트에 드래그 종료 알림
            dragable.EndDrag();

            // 2. 🔧 카드 사용 로직을 먼저 처리 (draggingCard가 null이 되기 전에!)
            HandleCardUsage();

            // 3. 🔧 카드 사용 완료 후에 드래깅 카드 해제
            HandDeck.Instance.SetDraggingCard(null);
        }

        // 마우스 이벤트와 분리된 카드 사용 로직
        private void HandleCardUsage()
        {
            if (card is MagicCard magicCard)
            {
                // 🔧 타겟팅 화살표는 MagicCardUse 내부에서 처리하도록 수정
                HandDeck.Instance.MagicCardUse(this);

                // 🔧 MagicCardUse 완료 후, 혹시 화살표가 아직 켜져 있다면 끄기
                // (타겟 선택 실패했을 수도 있으니)
                if (magicCard.IsSelectAttackType)
                {
                    HandDeck.Instance.SetTargettingArraow(false);
                }
            }
            else
            {
                // 일반 카드 (크리쳐, 빌딩 등) 사용
                if (TryGetNearestSlot(transform.position, out BSlot nearestSlot))
                {
                    Debug.Log($"HCard. Nearest Slot: {nearestSlot.Index}, card: {nearestSlot.Card}");

                    if (nearestSlot.Card == null && BattleLogic.Instance.HandCardUse(this, nearestSlot))
                        return;
                }
            }

            // 원래 위치로 복귀
            transform.DOMove(originalPosition, .1f);
        }

        // 기존 메서드들 완전 구현
        public void SetCard(BaseCard card)
        {
            cardUid = card.Uid;
            this.card = card;

            if (cardUI != null)
                cardUI.SetCard(card);
            else
                Debug.LogError("CardUI is null");

            // 카드 등급 설정
            GameObject cardGrade = card.CardGrade switch
            {
                CardGrade.Common => gradeCommon,
                CardGrade.Rare => gradeRare,
                CardGrade.Legendary => gradeLegend,
                _ => null
            };

            if (cardGrade)
                cardGrade.SetActive(true);
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

        void OnDrawGizmos()
        {
            // 드래그 중일 때만 구체를 그립니다.
            if (dragable != null && dragable.IsDragging)
            {
                Gizmos.color = Color.red;

                // HCard의 Collider를 가져옴
                if (TryGetComponent<Collider>(out var hCardCollider))
                {
                    // Collider의 중심과 크기를 사용하여 Box를 그림
                    Vector3 boxCenter = hCardCollider.bounds.center;
                    Vector3 boxSize = hCardCollider.bounds.extents * 2;
                    Quaternion boxOrientation = hCardCollider.transform.rotation;

                    // Draw the box used in Physics.OverlapBox
                    Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxOrientation, boxSize);
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
            }
        }
    }
}