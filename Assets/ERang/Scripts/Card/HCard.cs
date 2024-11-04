using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    // HandCard
    public class HCard : MonoBehaviour
    {
        public string CardUid => cardUid;

        private Dragable dragable;
        private MouseOver mouseOver;
        private BaseCard card;
        private CardUI cardUI;
        private string cardUid;

        private Vector3 originalPosition;

        void Awake()
        {
            dragable = GetComponent<Dragable>();
            mouseOver = GetComponent<MouseOver>();
            cardUI = GetComponent<CardUI>();
        }

        // Update is called once per frame
        void Update()
        {
            if (dragable == null || mouseOver == null)
                return;

            mouseOver.IsDragging = dragable.IsDragging;
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
    }
}