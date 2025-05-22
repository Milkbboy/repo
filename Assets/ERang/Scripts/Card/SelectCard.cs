using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace ERang
{
    public class SelectCard : MonoBehaviour
    {
        public GameCard Card => card;

        public bool isScaleFixed = false;
        public UnityAction<SelectCard> OnClick;

        public float hoverHeight = 0.3f;
        public float animationDuration = 0.1f;
        public float scaleFactor = 1.2f;

        private GameCard card;
        private CardUI cardUI;

        private Vector3 originalPosition;
        private Vector3 originalScale;

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

            // isScaleFixed = !isScaleFixed;
        }

        public void SetCard(GameCard card)
        {
            this.card = card;
            cardUI.SetCard(card);
        }

        public void SetScaleFixed(bool isScaleFixed)
        {
            this.isScaleFixed = isScaleFixed;

            OnMouseExit();
        }

        public IEnumerator DiscardAnimation(Transform position)
        {
            var discardAnimation = GetComponent<DiscardAnimation>();
            discardAnimation.PlaySequence(position, OnDiscardAnimationComplete());
            yield return new WaitUntil(() => discardAnimation.IsAnimationComplete);
        }

        private IEnumerator OnDiscardAnimationComplete()
        {
            // 애니메이션 완료 후 실행할 작업
            Debug.Log("DiscardAnimation Complete");
            yield return null;
        }

        public void SetDrawPostion(Vector3 position)
        {
            transform.localPosition = position;
            originalPosition = transform.position;
        }
    }
}