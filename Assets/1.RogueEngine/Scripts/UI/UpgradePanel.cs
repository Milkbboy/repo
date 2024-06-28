using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class UpgradePanel : MapPanel
    {
        public ChampionUI[] champions_ui;

        public ScrollRect scroll_view;
        public RectTransform content_rect;
        public GridLayoutGroup content_grid;
        public GameObject card_line_prefab;

        public GameObject unselect_area;
        public GameObject upgrade_area;
        public GameObject max_area;

        public Text free_text;
        public CardUI card_current;
        public CardUI card_next;
        public CardUI card_max;
        public Text text_current;
        public Text text_current2;
        public Text text_next;

        public Text gold_cost;
        public Image gold_icon;

        private string champion_uid;
        private string selected_card_uid;

        private List<CardLine> card_lines = new List<CardLine>();

        private static UpgradePanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (ChampionUI cui in champions_ui)
                cui.onClick += OnClickChampion;

        }

        public override void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();

            if (champion_uid == null)
                champion_uid = world.GetChampion(GameClient.Get().GetPlayerID()).uid;

            Champion champion = world.GetChampion(champion_uid);

            free_text.text = champion.free_upgrades.ToString() + " free";
            free_text.enabled = champion.free_upgrades > 0;

            foreach (CardLine line in card_lines)
                Destroy(line.gameObject);
            card_lines.Clear();

            foreach (ChampionUI champ_ui in champions_ui)
                champ_ui.Hide();

            int index = 0;
            foreach (Champion champ in world.champions)
            {
                if (champ.player_id == GameClient.Get().GetPlayerID())
                {
                    if (index < champions_ui.Length)
                    {
                        champions_ui[index].SetChampion(champ);
                        champions_ui[index].SetHighlight(champion.uid == champ.uid);
                        index++;
                    }
                }
            }

            ChampionCard selected_card = champion.GetCard(selected_card_uid);
            foreach (ChampionCard card in champion.cards)
            {
                CardLine line = CreateLine(card);

                if (selected_card != null && card.uid == selected_card.uid)
                    line.SetSelected(true);
            }

            float height = content_grid.cellSize.y + content_grid.spacing.y;
            content_rect.sizeDelta = new Vector2(content_rect.sizeDelta.x, champion.cards.Count * height + 200f);

            RefreshUpgradeArea();
        }

        private void RefreshUpgradeArea()
        {
            World world = GameClient.Get().GetWorld();
            Champion champion = world.GetChampion(champion_uid);
            Player player = world.GetPlayer(champion.player_id);
            ChampionCard selected_card = champion.GetCard(selected_card_uid);
            if (selected_card != null)
            {
                bool max_level = selected_card.level >= selected_card.CardData.level_max;
                int cost = world.GetUpgradeCost(selected_card.CardData, selected_card.level + 1);
                upgrade_area.SetActive(!max_level);
                max_area.SetActive(max_level);
                unselect_area.SetActive(false);

                card_current.SetCard(selected_card.CardData, selected_card.level);
                card_next.SetCard(selected_card.CardData, selected_card.level + 1);
                card_max.SetCard(selected_card.CardData, selected_card.level);
                text_current.text = "LEVEL " + selected_card.level.ToString();
                text_current2.text = "LEVEL " + selected_card.level.ToString();
                text_next.text = "LEVEL " + (selected_card.level + 1).ToString();
                gold_cost.text = cost.ToString();
                gold_cost.color = cost > player.gold ? Color.red : new Color(1f, 0.8f, 0.15f);
                gold_cost.enabled = champion.free_upgrades == 0;
                gold_icon.enabled = champion.free_upgrades == 0;

            }
            else
            {
                unselect_area.SetActive(true);
                upgrade_area.SetActive(false);
                max_area.SetActive(false);
            }
        }

        private CardLine CreateLine(ChampionCard card)
        {
            GameObject cobj = Instantiate(card_line_prefab, content_grid.transform);
            CardLine line = cobj.GetComponent<CardLine>();
            line.SetLine(card);
            line.onClick += OnClickCard;
            card_lines.Add(line);
            return line;
        }

        private void OnClickChampion(ChampionUI champ_ui)
        {
            Champion champion = champ_ui.GetChampion();
            champion_uid = champion.uid;
            RefreshPanel();
        }

        public void OnClickCard(CardLine dline)
        {
            ChampionCard ccard = dline.GetChampionCard();
            selected_card_uid = ccard.uid;

            foreach (CardLine line in card_lines)
            {
                line.SetSelected(line.GetChampionCard().uid == selected_card_uid);
            }

            RefreshUpgradeArea();
        }

        public void OnClickUpgrade()
        {
            World world = GameClient.Get().GetWorld();
            Champion champion = world.GetChampion(champion_uid);
            ChampionCard selected_card = champion.GetCard(selected_card_uid);
            if (selected_card != null)
            {
                GameClient.Get().MapUpgradeCard(champion, selected_card);
            }
        }

        public void OnClickQuit()
        {
            GameClient.Get().MapEventContinue();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            selected_card_uid = null;
        }

        public override bool ShouldShow()
        {
            World world = GameClient.Get().GetWorld();
            int player_id = GameClient.Get().GetPlayerID();
            return world.state == WorldState.Upgrade && !world.AreAllActionsCompleted(player_id);
        }

        public override bool IsAutomatic()
        {
            return true;
        }

        public static UpgradePanel Get()
        {
            return instance;
        }
    }
}
