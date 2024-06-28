using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class ShopPanel : MapPanel
    {
        public ChampionUI[] champions_ui;
        public BoxUI[] item_slots;
        public CardUI[] cards_ui;

        [Header("Info Area")]
        public UIPanel info_panel;
        public Text info_title;
        public Text info_desc;
        public BoxUI info_item;
        public CardUI info_card;
        public Text info_quantity;
        public Text info_price;
        public Button buy_button;

        private string champion_uid;
        private CardData selected_item;
        private CardData selected_card;
        private BoxUI selected_slot;

        private static ShopPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (ChampionUI cui in champions_ui)
                cui.onClick += OnClickChampion;

            foreach (BoxUI slot in item_slots)
                slot.onClick += OnClickSlot;

            foreach (CardUI ui in cards_ui)
                ui.onClick += OnClickCard;
        }

        public override void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();

            if (champion_uid == null)
                champion_uid = world.GetChampion(GameClient.Get().GetPlayerID()).uid;

            //Hide by default
            info_panel.Hide();
            foreach (BoxUI slot in item_slots)
                slot.Hide();

            foreach (CardUI ui in cards_ui)
                ui.Hide();

            Champion champion = world.GetChampion(champion_uid);
            foreach (ChampionUI champ_ui in champions_ui)
                champ_ui.Hide();

            //Display Champions
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

            //Display Items
            index = 0;
            foreach (string item_id in world.shop_items)
            {
                CardData item = CardData.Get(item_id);
                if (item != null && index < item_slots.Length)
                {
                    item_slots[index].SetItem(item);
                    item_slots[index].SetSelected(selected_slot == item_slots[index]);
                    index++;
                }
            }

            //Display Cards
            index = 0;
            foreach (string card_id in world.shop_cards)
            {
                CardData card = CardData.Get(card_id);
                if (card != null && index < cards_ui.Length)
                {
                    cards_ui[index].SetCard(card, 1);
                    index++;
                }
            }

            RefreshInfo();
        }

        private void RefreshInfo()
        {
            World world = GameClient.Get().GetWorld();
            Champion champion = world.GetChampion(champion_uid);

            if (selected_item != null)
            {
                int quantity = champion.CountItem(selected_item);
                info_title.text = selected_item.title;
                info_desc.text = selected_item.GetText();
                info_item.SetItem(selected_item);
                info_card.Hide();
                info_quantity.text = quantity.ToString();
                int cost = world.GetBuyCost(selected_item);
                info_price.text = cost.ToString();
                buy_button.interactable = world.shop_items.Contains(selected_item.id);
                info_panel.Show();
            }

            else if (selected_card != null)
            {
                info_title.text = selected_card.title;
                info_desc.text = selected_card.GetText();
                info_item.Hide();
                info_card.SetCard(selected_card, 1);
                info_quantity.text = champion.CountCard(selected_card).ToString();
                int cost = world.GetBuyCost(selected_card);
                info_price.text = cost.ToString();
                buy_button.interactable = world.shop_cards.Contains(selected_card.id);
                info_panel.Show();
            }
        }

        private void UnselectAll()
        {
            foreach (BoxUI slot in item_slots)
                slot.SetSelected(false);
        }

        public void OnClickSlot(BoxUI slot)
        {
            UnselectAll();
            slot.SetSelected(true);
            selected_slot = slot;
            selected_item = slot.GetCardData();
            selected_card = null;
            RefreshInfo();
        }

        public void OnClickCard(CardUI card)
        {
            UnselectAll();
            selected_card = card.GetCard();
            selected_slot = null;
            selected_item = null;
            RefreshInfo();
        }

        private void OnClickChampion(ChampionUI champ_ui)
        {
            Champion champion = champ_ui.GetChampion();
            this.champion_uid = champion.uid;
            RefreshPanel();
        }

        public void OnClickBuy()
        {
            World world = GameClient.Get().GetWorld();
            Champion champion = world.GetChampion(champion_uid);

            if (selected_card != null)
                GameClient.Get().MapBuyCard(champion, selected_card);
            else if (selected_item != null)
                GameClient.Get().MapBuyItem(champion, selected_item);
        }

        public void OnClickQuit()
        {
            GameClient.Get().MapEventContinue();
        }

        public override void Show(bool instant = false)
        {
            selected_slot = null;
            selected_item = null;
            base.Show(instant);
        }

        public override bool ShouldShow()
        {
            World world = GameClient.Get().GetWorld();
            int player_id = GameClient.Get().GetPlayerID();
            return world.state == WorldState.Shop && !world.AreAllActionsCompleted(player_id);
        }

        public override bool IsAutomatic()
        {
            return true;
        }

        public static ShopPanel Get()
        {
            return instance;
        }
    }
}
