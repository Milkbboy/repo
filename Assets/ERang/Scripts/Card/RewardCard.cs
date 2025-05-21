using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace ERang
{
    /// <summary>
    /// 보상 카드를 표현하는 UI 컴포넌트
    /// </summary>
    public class RewardCard : CardView
    {
        public UnityAction<RewardCard> OnClick;

        public float hoverHeight = 0.5f;
        public float animationDuration = 0.1f;
        public float scaleFactor = 0.5f;

        private bool isScaleFixed = false;

        protected override void OnMouseEnter()
        {
            if (isScaleFixed)
                return;

            base.OnMouseEnter();

            transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
            transform.DOScale(originalScale * scaleFactor, animationDuration);
        }

        protected override void OnMouseExit()
        {
            if (isScaleFixed)
                return;

            base.OnMouseExit();

            transform.DOMoveY(originalPosition.y, animationDuration);
            transform.DOScale(originalScale, animationDuration);
        }

        void OnMouseDown()
        {
            OnClick?.Invoke(this);
            isScaleFixed = true;
        }

        /// <summary>
        /// 고정 크기 설정 (선택 시 크기 고정)
        /// </summary>
        public void SetScaleFixed(bool isScaleFixed)
        {
            this.isScaleFixed = isScaleFixed;

            if (!isScaleFixed)
                OnMouseExit();
        }
    }
}