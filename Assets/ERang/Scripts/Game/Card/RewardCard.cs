using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace ERang
{
    public class RewardCard : MonoBehaviour
    {
        public BaseCard Card => card;

        public UnityAction<RewardCard> OnClick;

        public float hoverHeight = 0.5f;
        public float animationDuration = 0.1f;
        public float scaleFactor = 0.5f;

        private BaseCard card;
        private CardUI cardUI;

        private Vector3 originalPosition;
        private Vector3 originalScale;

        private bool isScaleFixed = false;

        void Awake()
        {
            cardUI = GetComponent<CardUI>();
        }

        void Start()
        {
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }

        void OnMouseEnter()
        {
            if (isScaleFixed)
                return;

            cardUI.ShowDesc(card.Id);

            transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
            transform.DOScale(originalScale * scaleFactor, animationDuration);
        }

        void OnMouseExit()
        {
            if (isScaleFixed)
                return;

            cardUI.ShowShortDesc(card.Id);

            transform.DOMoveY(originalPosition.y, animationDuration);
            transform.DOScale(originalScale, animationDuration);
        }

        void OnMouseDown()
        {
            OnClick?.Invoke(this);

            isScaleFixed = true;
        }

        public void SetCard(BaseCard card)
        {
            this.card = card;
            cardUI.SetCard(card);
        }

        public void SetScaleFixed(bool isScaleFixed)
        {
            this.isScaleFixed = isScaleFixed;

            OnMouseExit();
        }

        public void DiscardAnimation(Transform position)
        {
            GetComponent<DiscardAnimation>().PlaySequence(position);
        }
    }
}