using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RogueEngine.UI
{
    /// <summary>
    /// Scripts to display all stats of a card, 
    /// is used by other script that display cards like BoardCard, and HandCard, CollectionCard..
    /// </summary>

    public class CardUI : MonoBehaviour, IPointerClickHandler
    {
        public Image card_image;
        public Image team_icon;
        public Image rarity_icon;
        public Image level_icon;
        public Image cost_icon;
        public Text cost;

        public Text card_title;
        public Text card_lvl;
        public Text card_text;
        public Text card_traits;

        public TraitUI[] stats;

        public Sprite[] level_icons;

        public UnityAction<CardUI> onClick;
        public UnityAction<CardUI> onClickRight;

        private CardData card;
        private string uid;
        private int level;

        void Awake()
        {

        }

        public void SetCard(BattleCharacter caster, Card card)
        {
            if (card == null)
                return;

            uid = card.uid;
            SetCard(card.CardData, card.level);

            if (cost != null)
                cost.text = card.GetMana().ToString();

            if (card_text != null)
                card_text.text = card.CardData.GetText(caster, card);

            foreach (TraitUI stat in stats)
                stat.SetCard(card);
        }

        public void SetCard(ChampionCard card)
        {
            uid = card.uid;
            SetCard(card.CardData, card.level);
        }

        public void SetCard(CardData card, int level)
        {
            if (card == null)
                return;

            this.card = card;
            this.level = level;

            if(card_image != null)
                card_image.sprite = card.GetFullArt();
            if (card_title != null)
                card_title.text = card.GetTitle().ToUpper();
            if (card_text != null)
                card_text.text = card.GetText(level);
            if (card_traits != null)
                card_traits.text = card.GetTraitText();
            if (card_lvl != null)
                card_lvl.text = "LVL " + level.ToString();
            if (card_lvl != null)
                card_lvl.enabled = level > 1;

            if (cost != null)
                cost.text = card.GetMana(level).ToString();

            if (team_icon != null)
            {
                team_icon.sprite = card.team.icon;
                team_icon.enabled = team_icon.sprite != null;
            }

            if (rarity_icon != null)
            {
                rarity_icon.sprite = card.rarity.icon;
                rarity_icon.enabled = rarity_icon.sprite != null;
            }

            if (level_icon != null)
            {
                level_icon.sprite = GetLevelIcon(level);
                level_icon.enabled = level_icon.sprite != null;
            }

            foreach (TraitUI stat in stats)
                stat.SetCard(card);

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetMaterial(Material mat)
        {
            if (card_image != null)
                card_image.material = mat;
            if (team_icon != null)
                team_icon.material = mat;
            if (rarity_icon != null)
                rarity_icon.material = mat;
            if (cost_icon != null)
                cost_icon.material = mat;
        }

        public void SetOpacity(float opacity)
        {
            if (card_image != null)
                card_image.color = new Color(card_image.color.r, card_image.color.g, card_image.color.b, opacity);
            if (team_icon != null)
                team_icon.color = new Color(team_icon.color.r, team_icon.color.g, team_icon.color.b, opacity);
            if (rarity_icon != null)
                rarity_icon.color = new Color(rarity_icon.color.r, rarity_icon.color.g, rarity_icon.color.b, opacity);
            if (cost_icon != null)
                cost_icon.color = new Color(cost_icon.color.r, cost_icon.color.g, cost_icon.color.b, opacity);
            if (cost != null)
                cost.color = new Color(cost.color.r, cost.color.g, cost.color.b, opacity);
            if (card_title != null)
                card_title.color = new Color(card_title.color.r, card_title.color.g, card_title.color.b, opacity);
            if (card_text != null)
                card_text.color = new Color(card_text.color.r, card_text.color.g, card_text.color.b, opacity);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        public Sprite GetLevelIcon(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, level_icons.Length-1);
            if (level_icons.Length > 0)
                return level_icons[index];
            return null;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (onClick != null)
                    onClick.Invoke(this);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (onClickRight != null)
                    onClickRight.Invoke(this);
            }
        }

        public CardData GetCard()
        {
            return card;
        }

        public int GetLevel()
        {
            return level;
        }

        public string GetUID()
        {
            return uid;
        }
    }
}
