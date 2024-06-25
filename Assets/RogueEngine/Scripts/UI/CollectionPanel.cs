using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    /// <summary>
    /// CollectionPanel is the panel where players can see all the cards they own
    /// Also the panel where they can use the deckbuilder
    /// </summary>

    public class CollectionPanel : UIPanel
    {
        [Header("Cards")]
        public ScrollRect scroll_rect;
        public RectTransform scroll_content;
        public CardGrid grid_content;
        public GameObject card_prefab;

        [Header("Left Side")]
        public IconButton[] team_filters;

        public Toggle toggle_power;
        public Toggle toggle_attack;
        public Toggle toggle_skill;
        public Toggle toggle_common;
        public Toggle toggle_rare;

        public Dropdown sort_dropdown;
        public InputField search;

        public Material color_ui;
        public Material grayscale_ui;

        private TeamData filter_team = null;
        private int filter_dropdown = 0;
        private string filter_search = "";

        private List<CardUI> card_list = new List<CardUI>();
        private List<CardUI> all_list = new List<CardUI>();

        private bool spawned = false;
        private bool update_grid = false;
        private float update_grid_timer = 0f;

        private static CollectionPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            //Delete grid content
            for (int i = 0; i < grid_content.transform.childCount; i++)
                Destroy(grid_content.transform.GetChild(i).gameObject);

            foreach (IconButton button in team_filters)
                button.onClick += OnClickTeam;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

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

        private void SpawnCards()
        {
            spawned = true;
            foreach (CardUI card in all_list)
                Destroy(card.gameObject);
            all_list.Clear();

            foreach (CardData card in CardData.GetAll())
            {
                GameObject nCard = Instantiate(card_prefab, grid_content.transform);
                CardUI dCard = nCard.GetComponentInChildren<CardUI>();
                dCard.SetCard(card, 1);
                dCard.onClick += OnClickCard;
                dCard.onClickRight += OnClickCardRight;
                all_list.Add(dCard);
                nCard.SetActive(false);
            }
        }

        //----- Refresh UI --------

        private void RefreshAll()
        {
            RefreshFilters();
            RefreshCards();
        }

        private void RefreshFilters()
        {
            search.text = "";
            filter_team = null;
            filter_dropdown = 0;
            filter_search = "";
            sort_dropdown.value = 0;

            foreach (IconButton button in team_filters)
                button.Deactivate();

            if (team_filters.Length > 0)
            {
                team_filters[0].Activate();
                filter_team = TeamData.Get(team_filters[0].value);
            }
        }

        public void RefreshCards()
        {
            if (!spawned)
                SpawnCards();

            card_list.Clear();

            List<CardData> all_cards = new List<CardData>();
            List<CardData> shown_cards = new List<CardData>();

            foreach (CardData icard in CardData.GetAll())
            {
                all_cards.Add(icard);
            }

            if (filter_dropdown == 0) //Name
            {
                all_cards.Sort((CardData a, CardData b) =>
                {
                    if (IsAvailable(a) == IsAvailable(b))
                        return a.title.CompareTo(b.title);
                    else
                        return IsAvailable(b).CompareTo(IsAvailable(a));
                });
            }

            if (filter_dropdown == 1) //Cost
            {
                all_cards.Sort((CardData a, CardData b) =>
                {
                    if (IsAvailable(a) == IsAvailable(b))
                        return b.mana == a.mana ? a.title.CompareTo(b.title) : a.mana.CompareTo(b.mana);
                    else
                        return IsAvailable(b).CompareTo(IsAvailable(a));
                });
            }

            foreach (CardData card in all_cards)
            {
                if (card.availability != CardAvailability.Unlisted)
                {
                    if (filter_team == null || filter_team == card.team)
                    {
                        RarityData rarity = card.rarity;
                        CardType type = card.card_type;

                        bool is_attack = card.HasTrait("attack");
                        bool is_skill = card.HasTrait("skill");
                        bool is_power = type == CardType.Power;
                        bool is_common = card.rarity.id == "common";
                        bool is_rare = card.rarity.id == "rare";

                        bool type_check = (is_attack && toggle_attack.isOn)
                            || (is_skill && toggle_skill.isOn)
                            || (is_power && toggle_power.isOn)
                            || (!toggle_attack.isOn && !toggle_skill.isOn && !toggle_power.isOn);

                        bool rarity_check = (is_common && toggle_common.isOn)
                            || (is_rare && toggle_rare.isOn)
                            || (!toggle_common.isOn && !toggle_rare.isOn);

                        string search = filter_search.ToLower();
                        bool search_check = string.IsNullOrWhiteSpace(search)
                            || card.id.Contains(search)
                            || card.title.ToLower().Contains(search)
                            || card.GetText().ToLower().Contains(search);

                        if (type_check && rarity_check && search_check)
                        {
                            shown_cards.Add(card);
                        }
                    }
                }
            }

            int index = 0;
            foreach (CardData qcard in shown_cards)
            {
                if (index < all_list.Count)
                {
                    CardUI dcard = all_list[index];
                    bool unlocked = IsAvailable(qcard);
                    dcard.SetCard(qcard, 1);
                    dcard.SetMaterial(unlocked ? color_ui : grayscale_ui);
                    card_list.Add(dcard);
                    if (!dcard.transform.parent.gameObject.activeSelf)
                        dcard.transform.parent.gameObject.SetActive(true);
                    index++;
                }
            }

            for (int i = index; i < all_list.Count; i++)
                all_list[i].transform.parent.gameObject.SetActive(false);

            update_grid = true;
            update_grid_timer = 0f;
            scroll_rect.verticalNormalizedPosition = 1f;
        }

        public bool IsAvailable(CardData card)
        {
            UserData udata = Authenticator.Get().GetUserData();
            return card.IsAvailable(udata);
        }

        //---- Left Panel Filters Clicks -----------

        public void OnClickTeam(IconButton button)
        {
            filter_team = null;
            if (button.IsActive())
            {
                filter_team = TeamData.Get(button.value);
            }
            RefreshCards();
        }

        public void OnChangeToggle()
        {
            RefreshCards();
        }

        public void OnChangeDropdown()
        {
            filter_dropdown = sort_dropdown.value;
            RefreshCards();
        }

        public void OnChangeSearch()
        {
            filter_search = search.text;
            RefreshCards();
        }

        //---- Card grid clicks ----------

        public void OnClickCard(CardUI card)
        {
            CardZoomPanel.Get().ShowCard(card.GetCard(), 1);

        }

        public void OnClickCardRight(CardUI card)
        {
            CardZoomPanel.Get().ShowCard(card.GetCard(), 1);
        }

        //-----

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshAll();
        }

        public static CollectionPanel Get()
        {
            return instance;
        }
    }
}