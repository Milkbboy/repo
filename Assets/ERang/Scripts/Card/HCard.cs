using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ERang.Data;

namespace ERang
{
    // HandCard
    public class HCard : MonoBehaviour
    {
        public string CardUid => cardUid;
        public BaseCard Card => card;
        public bool IsHandOnCard => isHandOnCard;
        public HashSet<int> TargetSlotNumbers => targetSlotNumbers;

        public LayerMask slotLayerMask;
        public float detectionRadius = 1.0f; // 감지 반경

        private Dragable dragable;
        private bool isHandOnCard = false;

        private BaseCard card;
        private CardUI cardUI;
        private string cardUid;
        /// <summary>
        /// 핸드 카드 공격 타입
        /// </summary>
        private List<AiDataAttackType> aiDataAttackTypes = new();
        /// <summary>
        /// 핸드 카드 공격 타입이 Select 이면 선택 가능한 슬롯 번호
        /// </summary>
        private HashSet<int> targetSlotNumbers = new();

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
            // Debug.Log($"HCard. OnMouseEnter. {card?.Uid} {card?.LogText}");

            if (HandDeck.Instance.DraggingCard != null)
                return;

            if (card == null)
                return;

            cardUI.ShowDesc(card.Id);
        }

        void OnMouseExit()
        {
            // Debug.Log($"HCard. OnMouseExit. {card?.Uid} {card?.LogText}");

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
            if (card is MagicCard)
            {
                HandDeck.Instance.MagicCardUse(this);

                HandDeck.Instance.SetTargettingArraow(false);
            }
            else
            {
                if (TryGetNearestSlot(transform.position, out BSlot nearestSlot))
                {
                    // 슬롯 위치로 이동
                    Debug.Log($"HCard. Nearest Slot: {nearestSlot.Index}");

                    if (BattleLogic.Instance.HandCardUse(this, nearestSlot))
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

            // 마법 카드인 경우 공격 타입 및 타겟 슬롯 번호 설정
            if (card is not MagicCard)
                return;

            AiGroupData aiGroupData = AiGroupData.GetAiGroupData(card.AiGroupId);

            if (aiGroupData == null)
            {
                Debug.LogError($"HCard.SetCard 함수. AiGroupData is null. AiGroupId: {card.AiGroupId}");
                return;
            }

            foreach (List<int> aiDataIds in aiGroupData.ai_Groups)
            {
                foreach (int aiDataId in aiDataIds)
                {
                    AiData aiData = AiData.GetAiData(aiDataId);

                    if (aiData == null)
                    {
                        Debug.LogError($"HCard.SetCard 함수. AiData is null. AiDataId: {aiDataId}");
                        continue;
                    }

                    aiDataAttackTypes.Add(aiData.attackType);

                    if (aiData.attackType == AiDataAttackType.SelectEnemy || aiData.attackType == AiDataAttackType.SelectEnemyCreature)
                    {
                        foreach (var slotNumber in Constants.EnemySlotNumbers)
                        {
                            targetSlotNumbers.Add(slotNumber);
                        }
                    }

                    if (aiData.attackType == AiDataAttackType.SelectFriendly || aiData.attackType == AiDataAttackType.SelectFriendlyCreature)
                    {
                        foreach (var slotNumber in Constants.MySlotNumbers)
                        {
                            targetSlotNumbers.Add(slotNumber);
                        }
                    }

                    // 핸드 온 카드 설정
                    foreach (int abilityId in aiData.ability_Ids)
                    {
                        AbilityData ability = AbilityData.GetAbilityData(abilityId);

                        if (ability == null)
                        {
                            Debug.LogWarning($"HCard.SetCard 함수. AbilityData({abilityId}) {Utils.RedText("테이블 데이터 없음")}");
                            continue;
                        }

                        isHandOnCard = ability.workType == AbilityWorkType.OnHand;
                    }
                }
            }

            if (aiDataAttackTypes.Count > 0)
                Debug.Log($"핸드 카드 AttackTypes: {string.Join(", ", aiDataAttackTypes)}, targetSlotNumbers: {string.Join(", ", targetSlotNumbers)}");
            else
                Debug.Log($"핸드 카드 AttackTypes: 없음");
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