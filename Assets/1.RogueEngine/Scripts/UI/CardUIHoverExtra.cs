using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RogueEngine.UI
{
    [RequireComponent(typeof(CardUI))]
    public class CardUIHoverExtra : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject hover_area;
        public Text hover_text;

        private CardUI card_ui;
        private bool focus;

        void Awake()
        {
            card_ui = GetComponent<CardUI>();
            hover_area.SetActive(false);
        }

        void Update()
        {
            CardData card = card_ui.GetCard();
            if (card != null)
            {
                if (focus != hover_area.activeSelf)
                    hover_area.SetActive(focus);
                if (focus)
                    hover_text.text = card.GetTips();
            }
        }

        public void Hide()
        {
            card_ui.Hide();
        }

        public CardData GetCardData()
        {
            return card_ui.GetCard();
        }

        public bool IsFocus()
        {
            return focus;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            focus = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            focus = false;
        }

        private void OnDisable()
        {
            focus = false;
        }

    }
}
