using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class GameOverPanel : UIPanel
    {
        public UIPanel score_panel;
        public Text title;
        public Text completed_txt;
        public Text xp_txt;
        public Text cards_txt;
        public Text total_txt;
        public Text btn_txt;

        public UIPanel unlock_panel;
        public RectTransform card_zone;
        public float card_offset = 180f;
        public CardUI[] cards;
        public BoxUI[] items;

        private Vector2 start_pos;
        private int unlocked = 0;

        private static GameOverPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            start_pos = card_zone.anchoredPosition;
        }

        public void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();
            UserData udata = Authenticator.Get().UserData;

            title.text = world.completed ? "Victory" : "Game Over";
            completed_txt.text = world.GetCompleteScore().ToString();
            xp_txt.text = world.GetXpScore().ToString();
            cards_txt.text = world.GetCardsScore().ToString();
            total_txt.text = world.GetTotalScore().ToString();

            foreach (CardUI ui in cards)
                ui.Hide();
            foreach (BoxUI ui in items)
                ui.Hide();

            if (udata != null)
            {
                unlocked = 0;
                foreach (string id in udata.GetJustUnlockedCards())
                {
                    CardData card = CardData.Get(id);
                    if (card.IsCard() && unlocked < cards.Length)
                    {
                        cards[unlocked].SetCard(card, 1);
                        unlocked++;
                    }
                }

                foreach (string id in udata.GetJustUnlockedCards())
                {
                    CardData item = CardData.Get(id);
                    if (item.IsItem() && unlocked < cards.Length)
                    {
                        items[unlocked].SetItem(item);
                        unlocked++;
                    }
                }
            }

            float offset = Mathf.Max(cards.Length - unlocked, 0) * card_offset;
            card_zone.anchoredPosition = start_pos + offset * Vector2.right;
            btn_txt.text = unlocked > 0 ? "Continue" : "QUIT";
            score_panel.Show(true);
            unlock_panel.Hide(true);
        }

        public void GoToUnlocked()
        {
            unlock_panel.Show();
            score_panel.Hide();
            btn_txt.text = "QUIT";
        }

        public void OnClickQuit()
        {
            if (unlocked > 0 && !unlock_panel.IsVisible())
            {
                GoToUnlocked();
            }
            else
            {
                GameClient.Get().Disconnect();
                SceneNav.GoToMenu();
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static GameOverPanel Get()
        {
            return instance;
        }
    }
}
