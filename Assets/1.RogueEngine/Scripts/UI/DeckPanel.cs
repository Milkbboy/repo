using RogueEngine.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueEngine.UI
{


    public class DeckPanel : MapPanel
    {
        public Text title;
        public BoxUI champion_ui;
        public ScrollRect scroll_rect;
        public RectTransform scroll_content;
        public CardGrid grid_content;
        public GameObject card_prefab;
        public float card_size = 0.6f;

        private Champion champion;
        private ChampionData champ_data;
        private bool show_starter;
        private bool force_show = false;

        private bool update_grid = false;
        private float update_grid_timer = 0f;

        private List<CardUI> cards_list = new List<CardUI>();

        private HashSet<string> shown = new HashSet<string>();

        private static DeckPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            for (int i = 0; i < grid_content.transform.childCount; i++)
                Destroy(grid_content.transform.GetChild(i).gameObject);
        }

        public override void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();
            UserData udata = Authenticator.Get().UserData;

            if (!force_show)
            {
                champion = world.GetNextActionChampion(GameClient.Get().GetPlayerID());
                if(world.state == WorldState.Trash)
                    title.text = "Trash a Card";
            }

            foreach (CardUI card in cards_list)
                Destroy(card.gameObject);
            cards_list.Clear();
            shown.Clear();

            if (champion != null)
            {
                champion_ui.SetCharacter(champion.ChampionData);

                List <ChampionCard> cards = champion.GetDeck();
                foreach (ChampionCard card in cards)
                {
                    SpawnCard(card);
                }
            }

            if (champ_data != null)
            {
                champion_ui.SetCharacter(champ_data);
            
                foreach (CardData card in champ_data.start_cards)
                {
                    if (show_starter || !shown.Contains(card.id))
                    {
                        SpawnCard(card, 1);
                        shown.Add(card.id);
                    }
                }
                foreach (TeamData team in champ_data.reward_cards)
                {
                    List<CardData> cards = CardData.GetRewardCards(udata, team, null);
                    foreach (CardData card in cards)
                    {
                        if (!show_starter && !shown.Contains(card.id))
                        {
                            SpawnCard(card, 1);
                            shown.Add(card.id);
                        }
                    }
                }
            }

            update_grid = true;
            scroll_rect.verticalNormalizedPosition = 1f;
        }

        private void SpawnCard(ChampionCard card)
        {
            GameObject nCard = Instantiate(card_prefab, grid_content.transform);
            nCard.transform.localScale = Vector3.one * card_size;
            CardUI card_ui = nCard.GetComponent<CardUI>();
            card_ui.SetCard(card);
            card_ui.onClick += OnClickCard;
            //card_ui.onClickRight += OnClickCardRight;
            cards_list.Add(card_ui);
        }

        private void SpawnCard(CardData icard, int level)
        {
            GameObject nCard = Instantiate(card_prefab, grid_content.transform);
            nCard.transform.localScale = Vector3.one * card_size;
            CardUI card_ui = nCard.GetComponent<CardUI>();
            card_ui.SetCard(icard, level);
            card_ui.onClick += OnClickCard;
            //card_ui.onClickRight += OnClickCardRight;
            cards_list.Add(card_ui);
        }

        private void LateUpdate()
        {
            //Resize grid
            update_grid_timer += Time.deltaTime;
            if (update_grid && update_grid_timer > 0.2f)
            {
                grid_content.GetColumnAndRow(out int rows, out int cols);
                if (cols > 0)
                {
                    float row_height = grid_content.GetGrid().cellSize.y + grid_content.GetGrid().spacing.y;
                    float height = rows * row_height;
                    scroll_content.sizeDelta = new Vector2(scroll_content.sizeDelta.x, height + 100);
                    update_grid = false;
                }
            }
        }

        private void OnClickCard(CardUI card)
        {
            World world = GameClient.Get().GetWorld();
            if (world != null && world.state == WorldState.Trash)
            {
                ChampionCard ccard = champion.GetCard(card.GetCard());
                GameClient.Get().MapTrashCard(champion, ccard);
            }
            else if (CardZoomPanel.Get() != null)
            {
                CardData icard = card.GetCard();
                CardZoomPanel.Get().ShowCard(icard, 1);
            }
        }

        public void Show(Champion champion)
        {
            this.champion = champion;
            show_starter = false;
            title.text = "Deck";
            Show();
            RefreshPanel();
        }

        public void Show(ChampionData champion)
        {
            champ_data = champion;
            show_starter = false;
            title.text = "Champion Cards";
            Show();
            RefreshPanel();
        }

        public void Show(CardData item)
        {
            champ_data = null;
            show_starter = false;
            force_show = true;
            Show();
            RefreshPanel();
        }

        public void ShowStarter(ChampionData champion)
        {
            champ_data = champion;
            show_starter = true;
            force_show = true;
            title.text = "Starter Deck";
            Show();
            RefreshPanel();
        }

        public void OnClickClose()
        {
            World world = GameClient.Get().GetWorld();
            if (world != null && world.state == WorldState.Trash)
            {
                GameClient.Get().MapEventContinue();
            }

            Hide();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            force_show = false;
        }

        public override bool ShouldShow()
        {
            World world = GameClient.Get().GetWorld();
            Champion champ = world.GetNextActionChampion(GameClient.Get().GetPlayerID());
            bool is_trash = world.state == WorldState.Trash && champ != null && !champ.action_completed;
            return force_show || is_trash;
        }

        public override bool IsAutomatic()
        {
            return true;
        }

        public static DeckPanel Get()
        {
            return instance;
        }
    }
}
