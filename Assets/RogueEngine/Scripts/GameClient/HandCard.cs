using RogueEngine.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    /// <summary>
    /// Represents the visual aspect of a card in hand.
    /// Will take the data from Card.cs and display it
    /// </summary>

    public class HandCard : MonoBehaviour
    {
        public Image card_glow;
        public float move_speed = 10f;
        public float move_rotate_speed = 4f;
        public float move_max_rotate = 10f;

        [HideInInspector]
        public Vector2 deck_position;
        [HideInInspector]
        public float deck_angle;

        private string card_uid = "";

        private CardUI card_ui;
        private RectTransform hand_transform;
        private RectTransform card_transform;
        private Vector3 start_scale;
        private Vector3 current_rotate;
        private Vector3 target_rotate;
        private Vector3 prev_pos;

        private bool destroyed = false;
        private float focus_timer = 0f;

        private bool focus = false;
        private bool drag = false;
        private bool selected = false;

        private static List<HandCard> card_list = new List<HandCard>();

        void Awake()
        {
            card_list.Add(this);
            card_ui = GetComponent<CardUI>();
            card_transform = transform.GetComponent<RectTransform>();
            hand_transform = transform.parent.GetComponent<RectTransform>();
            start_scale = transform.localScale;
        }

        private void Start()
        {

        }

        private void OnDestroy()
        {
            card_list.Remove(this);
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            Card card = GetCard();
            Vector2 target_position = deck_position;
            Vector3 target_size = start_scale;
            focus_timer += Time.deltaTime;

            if (IsFocus())
            {
                target_position = deck_position + Vector2.up * 40f;
            }

            if (IsDrag())
            {
                target_position = GetTargetPosition();
                target_size = start_scale * 0.75f;
                Vector3 dir = card_transform.position - prev_pos;
                Vector3 addrot = new Vector3(dir.y * 90f, -dir.x * 90f, 0f);
                target_rotate += addrot * move_rotate_speed * Time.deltaTime;
                target_rotate = new Vector3(Mathf.Clamp(target_rotate.x, -move_max_rotate, move_max_rotate), Mathf.Clamp(target_rotate.y, -move_max_rotate, move_max_rotate), 0f);
                current_rotate = Vector3.Lerp(current_rotate, target_rotate, move_rotate_speed * Time.deltaTime);
            }
            else
            {
                target_rotate = new Vector3(0f, 0f, deck_angle);
                current_rotate = new Vector3(0f, 0f, deck_angle);
            }

            card_transform.anchoredPosition = Vector2.Lerp(card_transform.anchoredPosition, target_position, Time.deltaTime * move_speed);
            card_transform.localRotation = Quaternion.Slerp(card_transform.localRotation, Quaternion.Euler(current_rotate), Time.deltaTime * move_speed);
            card_transform.localScale = Vector3.Lerp(card_transform.localScale, target_size, 4f * Time.deltaTime);

            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter character = battle.GetCharacter(card.owner_uid);
            card_ui.SetCard(character, card);

            card_glow.enabled = IsFocus() || IsDrag();
            prev_pos = Vector3.Lerp(prev_pos, card_transform.position, 1f * Time.deltaTime);

            //Unselect
            if (!drag && selected && Input.GetMouseButtonDown(0))
                selected = false;
        }

        private Vector2 GetTargetPosition()
        {
            Card card = GetCard();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hand_transform, Input.mousePosition, Camera.main, out Vector2 tpos);
            if (card.CardData.IsRequireTarget())
            {
                tpos = deck_position + Vector2.up * 150f + Vector2.right * tpos.x / 10f;
            }
            return tpos;
        }

        public void SetCard(Card card)
        {
            this.card_uid = card.uid;

            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter character = battle.GetCharacter(card.owner_uid);
            card_ui.SetCard(character, card);
        }

        public void Kill()
        {
            if (!destroyed)
            {
                destroyed = true;
                Destroy(gameObject);
            }
        }

        public bool IsFocus()
        {
            if (GameTool.IsMobile())
                return selected && !drag;
            return focus && !drag && focus_timer > 0f;
        }

        public bool IsDrag()
        {
            return drag;
        }

        public Card GetCard()
        {
            Battle gdata = GameClient.Get().GetBattle();
            return gdata.GetCard(card_uid);
        }

        public CardData GetCardData()
        {
            Card card = GetCard();
            if (card != null)
                return CardData.Get(card.card_id);
            return null;
        }

        public string GetCardUID()
        {
            return card_uid;
        }

        public void OnMouseEnterCard()
        {
            if (BattleUI.IsUIOpened())
                return;

            focus = true;
        }

        public void OnMouseExitCard()
        {
            focus = false;
            focus_timer = -0.2f;
        }

        public void OnMouseDownCard()
        {
            if (BattleUI.IsOverUILayer("UI"))
                return;

            UnselectAll();
            drag = true;
            selected = true;
            PlayerControls.Get().UnselectAll();
            AudioTool.Get().PlaySFX("hand_card", AssetData.Get().hand_card_click_audio);
        }

        public void OnMouseUpCard()
        {
            Vector2 mpos = GameCamera.Get().MouseToPercent(Input.mousePosition);
            if (drag && mpos.y > 0.25f)
                TryPlayCard();
            else
                HandCardArea.Get().SortCards();
            drag = false;
        }

        public void TryPlayCard()
        {
            if (!GameClient.Get().IsYourTurn())
            {
                WarningText.ShowNotYourTurn();
                return;
            }

            BoardSlot bslot = BoardSlot.GetMouseRaycast();
            BoardCharacter bcharacter = BoardCharacter.GetMouseRaycast();
            Battle gdata = GameClient.Get().GetBattle();

            Card card = GetCard();
            BattleCharacter owner = gdata.GetCharacter(card.owner_uid);

            Slot slot = bslot != null ? bslot.GetSlot() : Slot.None;
            if (bcharacter != null && bslot == null)
                slot = bcharacter.GetCharacter().slot;

            if (!owner.CanPayMana(card))
            {
                WarningText.ShowNoMana();
                return;
            }

            if (gdata.CanPlayCard(card, slot, true))
            {
                PlayCard(slot);
            }
        }

        public void PlayCard(Slot target)
        {
            GameClient.Get().PlayCard(GetCard(), target);
            HandCardArea.Get().DelayRefresh(GetCard());
            Destroy(gameObject);
        }

        public CardData CardData { get { return GetCardData(); } }

        public static HandCard GetDrag()
        {
            foreach (HandCard card in card_list)
            {
                if (card.IsDrag())
                    return card;
            }
            return null;
        }

        public static HandCard GetFocus()
        {
            foreach (HandCard card in card_list)
            {
                if (card.IsFocus())
                    return card;
            }
            return null;
        }

        public static HandCard Get(string uid)
        {
            foreach (HandCard card in card_list)
            {
                if (card && card.GetCardUID() == uid)
                    return card;
            }
            return null;
        }

        public static void UnselectAll()
        {
            foreach (HandCard card in card_list)
                card.selected = false;
        }

        public static List<HandCard> GetAll()
        {
            return card_list;
        }
    }
}
