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
    public class CardUIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private CardUI card_ui;
        private Card card;
        private bool focus;

        private static List<CardUIHover> ui_list = new List<CardUIHover>();

        void Awake()
        {
            ui_list.Add(this);
            card_ui = GetComponent<CardUI>();
        }

        void OnDestroy()
        {
            ui_list.Remove(this);
        }

        public void SetCard(BattleCharacter owner, Card card, bool valid)
        {
            this.card = card;
            card_ui.SetCard(owner, card);
            card_ui.SetOpacity(valid ? 1f : 0.5f);
            card_ui.SetMaterial(valid ? AssetData.Get().color_ui : AssetData.Get().grayscale_ui);
        }

        public void Hide()
        {
            card_ui.Hide();
        }

        public Card GetCard()
        {
            return card;
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

        public static CardUIHover GetFocus()
        {
            foreach (CardUIHover ui in ui_list)
            {
                if (ui.IsFocus())
                    return ui;
            }
            return null;
        }

        public static List<CardUIHover> GetAll()
        {
            return ui_list;
        }
    }
}
