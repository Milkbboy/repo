using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class RewardPanel : MapPanel
    {
        public Image portrait;
        public Text reward_gold;
        public Text reward_xp;
        public Image reward_gold_img;
        public Image reward_xp_img;

        public Button new_card_btn;
        public UIPanel new_card_panel;

        public Button new_item_btn;
        public UIPanel new_item_panel;

        public RewardCard[] reward_cards;
        public BoxUI[] reward_items;

        private Champion champion;
        private CardData selected_card = null;
        private CardData selected_item = null;

        private static RewardPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (RewardCard card in reward_cards)
                card.onClick += OnClickCard;
            foreach (RewardCard card in reward_cards)
                card.onClickRight += OnClickCardRight;

            foreach (BoxUI card in reward_items)
                card.onClick += OnClickItem;
            foreach (BoxUI card in reward_items)
                card.onClickRight += OnClickItemRight;
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();
            champion = world.GetNextActionChampion(GameClient.Get().GetPlayerID());
            if (champion == null || champion.reward == null)
                return;

            portrait.sprite = champion.ChampionData.art_full;
            reward_gold.text = "+" + champion.reward.gold.ToString();
            reward_xp.text = "+" + champion.reward.xp.ToString();
            reward_gold.enabled = champion.reward.gold > 0;
            reward_xp.enabled = champion.reward.xp > 0;
            reward_gold_img.enabled = champion.reward.gold > 0;
            reward_xp_img.enabled = champion.reward.xp > 0;

            bool has_cards_choice = champion.reward.cards != null && champion.reward.cards.Count > 0;
            bool has_items_choice = champion.reward.items != null && champion.reward.items.Count > 0;
            new_card_btn.gameObject.SetActive(has_cards_choice || selected_card != null);
            new_item_btn.gameObject.SetActive(has_items_choice || selected_item != null);
            new_card_btn.interactable = selected_card == null;
            new_item_btn.interactable = selected_item == null;
            new_card_btn.GetComponentInChildren<Text>().text = selected_card ? selected_card.title : "New Card";
            new_item_btn.GetComponentInChildren<Text>().text = selected_item ? selected_item.title : "New Item";

            new_card_panel.Hide();
            new_item_panel.Hide();

            foreach (RewardCard card in reward_cards)
                card.Hide();

            int index = 0;
            foreach (string card_id in champion.reward.cards)
            {
                CardData card = CardData.Get(card_id);
                if (card != null && index < reward_cards.Length)
                {
                    reward_cards[index].Set(card);
                    index++;
                }
            }

            foreach (BoxUI card in reward_items)
                card.Hide();

            index = 0;
            foreach (string item_id in champion.reward.items)
            {
                CardData card = CardData.Get(item_id);
                if (card != null && index < reward_items.Length)
                {
                    reward_items[index].SetItem(card);
                    index++;
                }
            }
        }

        public void OnClickNewCard()
        {
            new_card_panel.Show();
        }

        public void OnClickNewItem()
        {
            new_item_panel.Show();
        }

        private void OnClickCard(RewardCard rcard)
        {
            selected_card = rcard.GetCard();
            GameClient.Get().MapSelectCardReward(champion, selected_card);
        }

        private void OnClickCardRight(RewardCard card)
        {
            CardZoomPanel.Get().ShowCard(card.GetCard(), card.GetLevel());
        }

        private void OnClickItem(BoxUI box)
        {
            selected_item = box.GetCardData();
            GameClient.Get().MapSelectItemReward(champion, selected_item);
        }

        private void OnClickItemRight(BoxUI box)
        {
            CardZoomPanel.Get().ShowCard(box.GetCardData(), 1);
        }

        public void OnClickSkip()
        {
            GameClient.Get().MapEventContinue(champion);
            Hide();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            selected_card = null;
            selected_item = null;
        }

        public override bool ShouldShow()
        {
            World world = GameClient.Get().GetWorld();
            Champion champ = world.GetNextActionChampion(GameClient.Get().GetPlayerID());
            bool is_reward = world.state == WorldState.Reward && champ != null && champ.reward != null && !champ.action_completed;
            bool is_level = world.state == WorldState.LevelUp && champ != null && champ.reward != null && world.event_champion == champ.uid;
            return is_reward || is_level;
        }

        public override bool IsAutomatic()
        {
            return true;
        }

        public static RewardPanel Get()
        {
            return instance;
        }
    }
}