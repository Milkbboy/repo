using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    public class RewardCard : MonoBehaviour
    {
        public UnityAction<RewardCard> onClick;
        public UnityAction<RewardCard> onClickRight;

        private CardData card;

        private CardUI card_ui;

        void Awake()
        {
            card_ui = GetComponent<CardUI>();
            card_ui.onClick += OnClick;
            card_ui.onClickRight += OnClickRight;
        }

        void Update()
        {

        }

        public void Set(CardData card)
        {
            this.card = card;
            card_ui.SetCard(card, 1);
        }

        public void Hide()
        {
            card_ui.Hide();
        }

        public CardData GetCard()
        {
            return card;
        }

        public int GetLevel()
        {
            return 1;
        }

        public void OnClick(CardUI ucard)
        {
            onClick?.Invoke(this);
        }

        public void OnClickRight(CardUI ucard)
        {
            onClickRight?.Invoke(this);
        }
    }
}