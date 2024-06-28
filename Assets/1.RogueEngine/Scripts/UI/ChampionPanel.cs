using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class ChampionPanel : MapPanel
    {
        public ChampionUI[] champions_ui;

        public Text name_txt;
        public Text level_txt;
        public Text xp_txt;
        public Text hp_txt;
        public Text speed_txt;
        public Text hand_txt;
        public Text energy_txt;
        public GameObject lvl_up_btn;
        public BoxUI[] items;

        [Header("Deck")]
        public ScrollRect scroll_rect;
        public RectTransform scroll_content;
        public CardGrid grid_content;
        public GameObject card_prefab;
        public float card_size = 0.5f;

        private string champion_uid;
        private ChampionData champion_data;
        private bool show_starter = false;

        private bool update_grid = false;
        private float update_grid_timer = 0f;

        private List<CardUI> cards_list = new List<CardUI>();

        private HashSet<string> shown = new HashSet<string>();

        private static ChampionPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (BoxUI slot in items)
                slot.onClick += OnClickItem;

            foreach (ChampionUI cui in champions_ui)
            {
                cui.onClick += OnClickChampion;
                cui.onClickLvlUp += OnClickChampionLvlUp;
            }

            for (int i = 0; i < grid_content.transform.childCount; i++)
                Destroy(grid_content.transform.GetChild(i).gameObject);
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

        public override void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();
            Champion champion = world.GetChampion(champion_uid);

            name_txt.text = champion.ChampionData.title;
            level_txt.text = "Level " + champion.level;
            xp_txt.text = champion.xp + " / " + GameplayData.Get().GetXpForNextLevel(champion.level) + " XP";

            bool can_lvl = champion.CanLevelUp();
            if (can_lvl != lvl_up_btn.activeSelf)
                lvl_up_btn.SetActive(can_lvl);

            foreach (ChampionUI champ_ui in champions_ui)
                champ_ui.Hide();

            int index = 0;
            foreach (Champion champ in world.champions)
            {
                if (index < champions_ui.Length)
                {
                    champions_ui[index].SetChampion(champ);
                    champions_ui[index].SetHighlight(champion.uid == champ.uid);
                    index++;
                }
            }

            foreach (BoxUI slot in items)
                slot.SetEmpty();

            index = 0;
            foreach (ChampionItem citem in champion.inventory)
            {
                if (index < items.Length && citem != null)
                {
                    items[index].SetItem(champion, citem);
                    index++;
                }
            }

            hp_txt.text = champion.GetHP() + " / " + champion.GetHPMax();
            speed_txt.text = champion.GetSpeed().ToString();
            hand_txt.text = champion.GetHand().ToString();
            energy_txt.text = champion.GetEnergy().ToString();

            RefreshDeck();
        }

        public void RefreshDeck()
        {
            World world = GameClient.Get().GetWorld();
            UserData udata = Authenticator.Get().UserData;

            foreach (CardUI card in cards_list)
                Destroy(card.gameObject);
            cards_list.Clear();
            shown.Clear();

            if (champion_uid != null)
            {
                Champion champion = world.GetChampion(champion_uid);
                List<ChampionCard> cards = champion.GetDeck();
                foreach (ChampionCard card in cards)
                {
                    SpawnCard(card);
                }
            }

            if (champion_data != null)
            {
                foreach (CardData card in champion_data.start_cards)
                {
                    if (show_starter || !shown.Contains(card.id))
                    {
                        SpawnCard(card, 1);
                        shown.Add(card.id);
                    }
                }
                foreach (TeamData team in champion_data.reward_cards)
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

        private void OnClickCard(CardUI card)
        {
            if (CardZoomPanel.Get() != null)
            {
                CardData icard = card.GetCard();
                CardZoomPanel.Get().ShowCard(icard, card.GetLevel());
            }
        }

        private void OnClickChampion(ChampionUI champ_ui)
        {
            Champion champion = champ_ui.GetChampion();
            champion_uid = champion.uid;
            RefreshPanel();
        }

        private void OnClickChampionLvlUp(ChampionUI ui)
        {
            Champion champion = ui.GetChampion();
            if (champion != null && champion.CanLevelUp())
            {
                GameClient.Get().LevelUp(champion);
            }
        }

        public void ShowChampion(Champion champ)
        {
            if (champ != null)
            {
                champion_uid = champ.uid;
                champion_data = null;
                RefreshPanel();
                Show();
            }
        }

        public void ShowChampion(ChampionData champ)
        {
            if (champ != null)
            {
                champion_uid = null;
                champion_data = champ;
                RefreshPanel();
                Show();
            }
        }

        public void OnClickItem(BoxUI slot)
        {
            ChampionItem item = slot.GetItem();
            if (item != null)
            {
                CardData card = CardData.Get(item.card_id);
                CardZoomPanel.Get().ShowCard(card, 1);
            }
        }

        public void OnClickLevelUp()
        {
            World world = GameClient.Get().GetWorld();
            Champion champion = world.GetChampion(champion_uid);
            if (champion != null && champion.CanLevelUp())
            {
                GameClient.Get().LevelUp(champion);
            }
        }

        public static ChampionPanel Get()
        {
            return instance;
        }
    }
}
