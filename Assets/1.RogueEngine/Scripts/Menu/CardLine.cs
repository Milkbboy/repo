using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    /// <summary>
    /// One line in the deckbuilder (can contain a card or a deck title)
    /// </summary>

    public class CardLine : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        public Image frame;
        public Image highlight;
        public Text title;
        public Text value;
        public Image level_icon;
        public Text level_txt;
        public IconValue cost;
        public UIPanel delete_btn;
        public AudioClip click_audio;
        public Material disabled_mat;
        public Material default_mat;
        public Sprite[] level_icons;

        public UnityAction<CardLine> onClick;
        public UnityAction<CardLine> onClickRight;
        public UnityAction<CardLine> onClickDelete;

        private ChampionCard ccard;
        private CardData card;
        private bool hidden = false;
        private bool hover = false;

        void Awake()
        {

        }

        void Update()
        {
            if (delete_btn != null)
            {
                bool visi = hover || GameTool.IsMobile();
                delete_btn.SetVisible(visi && !hidden);
            }
        }

        public void SetLine(ChampionCard card)
        {
            SetLine(card.CardData, card.level, 0);
            this.ccard = card;
        }

        public void SetLine(CardData card, int level, int quantity, bool invalid = false)
        {
            this.card = card;
            hidden = false;

            if (title != null)
                title.text = card.title;
            if (value != null)
                value.text = quantity.ToString();
            if (value != null)
                value.enabled = quantity > 1;
            if (cost != null)
                cost.value = card.GetMana(level);
            if (this.value != null)
                this.value.color = invalid ? Color.red : Color.white;
            if (level_txt != null)
                level_txt.text = "LVL " + level;
            if (level_txt != null)
                level_txt.enabled = level > 1;
            if (invalid)
                title.color = Color.gray;
            if (highlight != null)
                highlight.enabled = false;

            if (level_icon != null)
            {
                level_icon.sprite = GetLevelIcon(level);
                level_icon.enabled = level_icon.sprite != null;
            }

            if (image != null)
            {
                image.sprite = card.GetFullArt();
                image.enabled = true;
                image.material = invalid ? disabled_mat : default_mat;
            }

            gameObject.SetActive(true);
        }

        public void SetLine(string title)
        {
            this.card = null;
            hidden = false;

            if (this.title != null)
                this.title.text = title;
            if (this.title != null)
                this.title.color = Color.white;

            if (this.value != null)
                this.value.enabled = false;

            if (highlight != null)
                highlight.enabled = false;

            gameObject.SetActive(true);
        }

        public void SetSelected(bool selected)
        {
            if(highlight != null)
                highlight.enabled = selected;
        }

        public void Hide()
        {
            this.card = null;
            hidden = true;
            hover = false;

            if (title != null)
                title.text = "";
            if (this.title != null)
                this.title.color = Color.white;
            if (value != null)
                value.text = "";
            if (value != null)
                value.enabled = true;
            if (cost != null)
                cost.value = 0;
            if (image != null)
                image.enabled = false;
            if (frame != null)
                frame.enabled = false;
            if (delete_btn != null)
                delete_btn.SetVisible(false);

            gameObject.SetActive(false);
        }

        public Sprite GetLevelIcon(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, level_icons.Length - 1);
            if (level_icons.Length > 0)
                return level_icons[index];
            return null;
        }

        public CardData GetCard()
        {
            return card;
        }

        public ChampionCard GetChampionCard()
        {
            return ccard;
        }

        public bool IsHidden()
        {
            return hidden;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (hidden)
                return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onClick?.Invoke(this);
                AudioTool.Get().PlaySFX("ui", click_audio);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                onClickRight?.Invoke(this);
                AudioTool.Get().PlaySFX("ui", click_audio);
            }
        }

        public void OnClickDelete()
        {
            onClickDelete?.Invoke(this);
            AudioTool.Get().PlaySFX("ui", click_audio);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }
    }
}