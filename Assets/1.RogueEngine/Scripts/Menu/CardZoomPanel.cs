using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    /// <summary>
    /// When clicking on a card in menu, a box will appear with additional game info
    /// You can also buy cards in this panel
    /// </summary>

    public class CardZoomPanel : UIPanel
    {
        public CardUI card_ui;
        public Text title;
        public Text desc;

        public LevelTab[] level_tabs;

        private CardData card;
        private int level;

        private static CardZoomPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (LevelTab tab in level_tabs)
                tab.onClick += OnClickLevel;

            TabButton.onClickAny += OnClickTab;
        }

        private void OnDestroy()
        {
            TabButton.onClickAny -= OnClickTab;
        }

        protected override void Update()
        {
            base.Update();

        }

        public void ShowCard(CardData card, int level)
        {
            this.card = card;
            this.level = level;

            card_ui.SetCard(card, level);
            title.text = card.GetTitle();

            string desc = card.GetTips();
            string cdesc = card.GetText(level);
            if(!string.IsNullOrWhiteSpace(cdesc))
                this.desc.text = cdesc + "\n\n" + "<size=16>" + desc + "</size>";
            else
                this.desc.text = "<size=16>" + desc + "</size>";

            foreach (LevelTab tab in level_tabs)
            {
                tab.SetSelected(tab.level == level);
                tab.gameObject.SetActive(tab.level <= card.level_max);
            }

            Show();
        }

        public void RefreshCard()
        {
            ShowCard(card, level);
        }

        public void OnClickLevel(int level)
        {
            this.level = level;
            RefreshCard();
        }

        private void OnClickTab(TabButton btn)
        {
            if (btn.group == "menu")
                Hide();
        }

        public CardData GetCard()
        {
            return card;
        }

        public string GetCardId()
        {
            return card.id;
        }

        public static CardZoomPanel Get()
        {
            return instance;
        }
    }
}