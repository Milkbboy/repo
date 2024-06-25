using System.Collections;
using System.Collections.Generic;
using RogueEngine.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RogueEngine.UI
{

    public class BoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Text title;
        public Text desc;
        public Image icon;
        public Text value;
        public Image selected;
        public Image frame;

        public UnityAction<BoxUI> onClick;
        public UnityAction<BoxUI> onClickRight;

        private ChampionData champion_data;
        private CharacterData character_data;
        private CardData card_data;
        private Champion champion;
        private Card card;
        private string uid;

        private bool is_hover = false;
        private bool clickable = true;

        private static List<BoxUI> slots = new List<BoxUI>();

        protected virtual void Awake()
        {
            slots.Add(this);
        }

        protected virtual void OnDestroy()
        {
            slots.Remove(this);
        }

        protected virtual void Start()
        {

        }

        public void SetCard(Card card)
        {
            this.character_data = null;
            this.champion_data = null;
            this.card = card;
            this.card_data = null;
            uid = card.uid;

            if (title != null)
                title.text = card.CardData.GetTitle();
            if (desc != null)
                desc.text = "";
            if (icon != null)
                icon.sprite = card.CardData.GetFullArt();
            if (icon != null)
                icon.enabled = icon.sprite != null;
            if (value != null)
                value.enabled = false;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetItem(Champion champion, ChampionItem item)
        {
            uid = item.card_id;
            this.champion = champion;
            this.card_data = CardData.Get(item.card_id);

            if (title != null)
                title.text = this.card_data.GetTitle();
            if (desc != null)
                desc.text = "";
            if (icon != null)
                icon.sprite = this.card_data.GetIconArt();
            if (icon != null)
                icon.enabled = icon.sprite != null;
            if (value != null)
                value.text = item.quantity.ToString();
            if (value != null)
                value.enabled = item.quantity > 1;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetItemCard(Card card, int quantity)
        {
            uid = card.uid;
            this.card = card;
            this.card_data = CardData.Get(card.card_id);

            if (title != null)
                title.text = this.card_data.GetTitle();
            if (desc != null)
                desc.text = "";
            if (icon != null)
                icon.sprite = this.card_data.GetIconArt();
            if (icon != null)
                icon.enabled = icon.sprite != null;
            if (value != null)
                value.text = quantity.ToString();
            if (value != null)
                value.enabled = quantity > 1;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetCard(CardData card)
        {
            this.character_data = null;
            this.champion_data = null;
            this.card = null;
            this.card_data = card;

            if (title != null)
                title.text = card.title;
            if (desc != null)
                desc.text = card.text;
            if (icon != null)
                icon.sprite = card.GetFullArt();
            if (icon != null)
                icon.enabled = icon.sprite != null;
            if (value != null)
                value.enabled = false;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetItem(CardData item)
        {
            this.character_data = null;
            this.champion_data = null;
            this.card = null;
            this.card_data = item;

            if (title != null)
                title.text = item.title;
            if (desc != null)
                desc.text = item.text;
            if (icon != null)
                icon.sprite = item.GetIconArt();
            if (icon != null)
                icon.enabled = icon.sprite != null;
            if (value != null)
                value.enabled = false;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetCharacter(CharacterData charact)
        {
            this.character_data = charact;
            this.champion_data = null;
            this.card = null;
            this.card_data = null;

            if (title != null)
                title.text = charact.title;
            if (desc != null)
                desc.text = charact.desc;
            if (icon != null)
                icon.sprite = charact.art_portrait;
            if (icon != null)
                icon.enabled = charact.art_portrait != null;
            if (value != null)
                value.enabled = false;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetCharacter(ChampionData charact)
        {
            this.character_data = null;
            this.champion_data = charact;
            this.card = null;
            this.card_data = null;

            if (title != null)
                title.text = charact.title;
            if (desc != null)
                desc.text = charact.desc;
            if (icon != null)
                icon.sprite = charact.art_portrait;
            if (icon != null)
                icon.enabled = charact.art_portrait != null;
            if (value != null)
                value.enabled = false;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetEmpty()
        {
            this.character_data = null;
            this.champion_data = null;
            this.card = null;
            this.card_data = null;
            this.uid = "";

            if (title != null)
                title.text = "";
            if (desc != null)
                desc.text = "";
            if (icon != null)
                icon.enabled = false;
            if (selected != null)
                selected.enabled = false;
            if (value != null)
                value.enabled = false;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetSelected(bool select)
        {
            if (selected != null)
                selected.enabled = select;
        }

        public void SetClickable(bool clickable)
        {
            this.clickable = clickable;
            if (frame != null)
                frame.color = clickable ? Color.white : Color.gray;
        }

        public void Show()
        {
            if(!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
        
        public CharacterData GetCharacterData()
        {
            return character_data;
        }

        public ChampionData GetChampionData()
        {
            return champion_data;
        }

        public CardData GetCardData()
        {
            return card_data;
        }

        public ChampionItem GetItem()
        {
            return champion?.GetItem(uid);
        }

        public Card GetCard()
        {
            return card;
        }

        public string GetUID()
        {
            return uid;
        }

        public bool IsHover()
        {
            return is_hover;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickable && eventData.button == PointerEventData.InputButton.Left)
            {
                onClick?.Invoke(this);
            }

            if (clickable && eventData.button == PointerEventData.InputButton.Right)
            {
                onClickRight?.Invoke(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            is_hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            is_hover = false;
        }

        void OnDisable()
        {
            is_hover = false;
        }

        public static BoxUI GetHover()
        {
            foreach (BoxUI line in slots)
            {
                if (line.is_hover)
                    return line;
            }
            return null;
        }

        public static BoxUI GetHoverCard()
        {
            foreach (BoxUI line in slots)
            {
                if (line.card != null && line.is_hover)
                    return line;
            }
            return null;
        }
    }
}