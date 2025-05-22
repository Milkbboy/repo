using UnityEngine;
using UnityEngine.Events;

namespace ERang
{
    /// <summary>
    /// 모든 카드 UI 컴포넌트의 기본 클래스
    /// </summary>
    public abstract class CardView : MonoBehaviour
    {
        // 카드 데이터 참조
        public GameCard Card { get; protected set; }
        
        // 카드 UI 컴포넌트
        protected CardUI cardUI;
        
        // 원래 위치 및 크기
        protected Vector3 originalPosition;
        protected Vector3 originalScale;
        
        protected virtual void Awake()
        {
            cardUI = GetComponent<CardUI>();
        }
        
        protected virtual void Start()
        {
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }
        
        /// <summary>
        /// 카드 데이터 설정
        /// </summary>
        public virtual void SetCard(GameCard card)
        {
            this.Card = card;
            
            if (cardUI != null)
                cardUI.SetCard(card);
            else
                Debug.LogError("CardUI is null");
        }
        
        /// <summary>
        /// 카드 UI 업데이트
        /// </summary>
        public virtual void UpdateCardUI()
        {
            if (cardUI != null && Card != null)
                cardUI.SetCard(Card);
        }
        
        /// <summary>
        /// 원래 위치로 돌아가기
        /// </summary>
        public virtual void GoBackPosition()
        {
            transform.position = originalPosition;
        }
        
        /// <summary>
        /// 카드 폐기 애니메이션 실행
        /// </summary>
        public virtual void DiscardAnimation(Transform discardPos)
        {
            DiscardAnimation discardAnimation = GetComponent<DiscardAnimation>();
            if (discardAnimation != null)
                discardAnimation.PlaySequence(discardPos);
        }
        
        /// <summary>
        /// 마우스 엔터 시 상세 설명 표시
        /// </summary>
        protected virtual void OnMouseEnter()
        {
            if (Card != null)
                cardUI.ShowDesc(Card.Id);
        }
        
        /// <summary>
        /// 마우스 이탈 시 간략 설명 표시
        /// </summary>
        protected virtual void OnMouseExit()
        {
            if (Card != null)
                cardUI.ShowShortDesc(Card.Id);
        }
    }
}