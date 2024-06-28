using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using RogueEngine;

namespace RogueEngine.UI
{
    /// <summary>
    /// The UI for card selector, appears when an ability with CardSelector target is triggered
    /// </summary>

    public class CardSelector : SelectorPanel
    {
        public GameObject card_prefab;

        public RectTransform content;
        public Text title;
        public Text subtitle;

        public Button select_button;
        public Text select_button_text;
        public float card_spacing = 100f;

        private AbilityData iability;

        private List<Card> card_list = new List<Card>();
        private List<CardSelectorCard> selector_list = new List<CardSelectorCard>();

        private Vector2 mouse_start;
        private int mouse_start_index;
        private int selection_index = 0;
        private bool drag = false;
        private float mouse_scroll = 0f;
        private float timer = 0f;
        private bool force_show = false;

        private static CardSelector instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            Hide();
        }

        protected override void Update()
        {
            base.Update();

            if (!GameClient.Get().IsReady())
                return;

            timer += Time.deltaTime;

            //Drag cards
            Vector2 mouse_pos = GetMouseRectPosition();
            Vector2 move = mouse_pos - mouse_start;
            if (drag && move.magnitude > 0.1f)
            {
                selection_index = mouse_start_index - Mathf.RoundToInt(move.x / card_spacing);
                selection_index = Mathf.Clamp(selection_index, 0, selector_list.Count - 1);
            }

            //Mouse scroll
            mouse_scroll += -Input.mouseScrollDelta.y;
            if (mouse_scroll > 0.5f)
            {
                OnClickNext();
                mouse_scroll -= 1f;
            }
            else if (mouse_scroll < -0.5f)
            {
                OnClickPrev();
                mouse_scroll += 1f;
            }

            //Refresh cards
            foreach (CardSelectorCard card in selector_list)
            {
                bool is_selected = card.GetIndex() == selection_index;
                Vector3 pos = GetCardPosition(card);
                Vector3 scale = is_selected ? Vector3.one : Vector3.one / 2f;
                card.SetTargetPos(pos);
                card.SetTargetScale(scale);
            }

            //Close on right click if not a selection
            if (iability == null && Input.GetMouseButtonDown(1) && timer > 1f)
                Hide();

            Battle battle = GameClient.Get().GetBattle();
            if (battle != null && iability != null && battle.selector == SelectorType.None)
                Hide(); //Ability was selected already, close panel
        }

        public void RefreshPanel()
        {
            foreach (CardSelectorCard card in selector_list)
                Destroy(card.gameObject);
            selector_list.Clear();
            drag = false;
            mouse_scroll = 0f;

            select_button_text.text = (iability != null) ? "Select" : "OK";
            select_button.gameObject.SetActive(iability != null);

            int index = 0;
            foreach (Card card in card_list)
            {
                CardData icard = CardData.Get(card.card_id);
                if (icard != null)
                {
                    GameObject obj = Instantiate(card_prefab, content.transform);

                    RectTransform rect = obj.GetComponent<RectTransform>();
                    CardSelectorCard selector_card = obj.GetComponent<CardSelectorCard>();
                    selector_card.SetCard(card);
                    selector_card.SetIndex(index);

                    Vector3 pos = GetCardPosition(selector_card);
                    Vector3 scale = (index == selection_index ? 1 : 0.5f) * Vector3.one;
                    selector_card.SetTargetPos(pos);
                    selector_card.SetTargetScale(scale);
                    rect.anchoredPosition = pos;
                    selector_list.Add(selector_card);

                    index++;
                }
            }
        }

        //Show ability
        public override void Show(AbilityData iability, BattleCharacter caster, Card card)
        {
            Battle battle = GameClient.Get().GetBattle();
            this.card_list = iability.GetCardTargets(battle, caster, card);
            this.iability = iability;
            title.text = iability.title;
            subtitle.text = iability.desc;
            selection_index = 0;
            force_show = false;
            timer = 0f;
            Show();
        }

        //Show deck/discard
        public void Show(List<Card> card_list, string title, bool sort = false)
        {
            this.card_list.Clear();
            this.card_list.AddRange(card_list);
            this.iability = null;
            this.title.text = title;
            subtitle.text = "";
            selection_index = 0;
            force_show = true;
            timer = 0f;

            if (sort)
            {
                this.card_list.Sort((Card a, Card b) => { return a.CardData.title.CompareTo(b.CardData.title); }); //Reorder to not show the deck order
            }

            Show();
        }

        public void OnClickOK()
        {
            Battle battle = GameClient.Get().GetBattle();
            if (iability != null && battle.selector == SelectorType.SelectorCard)
            {
                CardSelectorCard selector_card = null;
                if (selection_index >= 0 && selection_index < selector_list.Count)
                    selector_card = selector_list[selection_index];

                if (selector_card != null)
                {
                    Card selected_card = selector_card.GetCard();
                    Card card = battle.GetCard(battle.selector_card_uid);
                    BattleCharacter character = battle.GetCharacter(battle.selector_caster_uid);
                    if (selected_card != null && iability.AreTargetConditionsMet(battle, character, card, selected_card))
                    {
                        GameClient.Get().SelectCard(selected_card);
                        Hide();
                    }
                }
            }
            else
            {
                Hide();
            }
        }

        public void OnClickMouseDown()
        {
            mouse_start = GetMouseRectPosition();
            mouse_start_index = selection_index;
            drag = true;
        }

        public void OnClickMouseUp()
        {
            drag = false;
        }

        public void OnClickCancel()
        {
            GameClient.Get().CancelSelection();
            Hide();
        }

        public void OnClickNext()
        {
            selection_index += 1;
            selection_index = Mathf.Clamp(selection_index, 0, selector_list.Count - 1);
        }

        public void OnClickPrev()
        {
            selection_index -= 1;
            selection_index = Mathf.Clamp(selection_index, 0, selector_list.Count - 1);
        }

        private Vector2 GetCardPosition(CardSelectorCard card)
        {
            int index_offset = card.GetIndex() - selection_index;
            Vector2 pos = new Vector2(index_offset * card_spacing, (index_offset != 0) ? 50f : 0f);
            float center_offset = (index_offset != 0) ? (Mathf.Sign(index_offset) * 140f) : 0;
            pos += Vector2.right * center_offset;
            return pos;
        }

        private Vector2 GetMouseRectPosition()
        {
            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out localpoint);
            return localpoint;
        }

        public bool IsAbility()
        {
            return IsVisible() && iability != null;
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            force_show = false;
        }

        public override bool ShouldShow()
        {
            Battle battle = GameClient.Get().GetBattle();
            int player_id = GameClient.Get().GetPlayerID();
            return force_show || (battle.selector == SelectorType.SelectorCard && battle.selector_player_id == player_id);
        }

        public static CardSelector Get()
        {
            return instance;
        }
    }
}