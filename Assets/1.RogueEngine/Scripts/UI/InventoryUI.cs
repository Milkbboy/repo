using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    //Inventory to play items during battles

    public class InventoryUI : MonoBehaviour
    {
        public BoxUI[] items;

        public UIPanel info_panel;
        public Text info_title;
        public Text info_text;

        private float timer = 0f;

        private static InventoryUI instance;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            foreach (BoxUI box in items)
                box.onClick += OnClickItem;

            foreach (BoxUI box in items)
                box.Hide();
        }

        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                timer = 0f;
                Refresh();
            }

            BoxUI hovered = GetHover();
            info_panel.SetVisible(hovered != null);
            if (hovered != null)
            {
                Card item = hovered.GetCard();
                info_title.text = item.CardData.GetTitle();
                info_text.text = item.CardData.GetText(1, Mathf.RoundToInt(info_text.fontSize * 1.5f));
            }
        }

        private void Refresh()
        {
            World world = GameClient.Get().GetWorld();
            BattleCharacter active = world.battle.GetActiveCharacter();
            if (active == null)
                return;

            Champion champion = world.GetChampion(active.uid);
            HashSet<string> added = new HashSet<string>();

            int index = 0;
            foreach (Card card in active.cards_item)
            {
                ChampionItem item = champion.GetItem(card.card_id);
                if (index < items.Length && !added.Contains(card.card_id))
                {
                    added.Add(card.card_id);
                    items[index].SetItemCard(card, item.quantity);
                    items[index].SetClickable(item.CardData.item_type == ItemType.ItemConsumable);

                    index++;
                }
            }

            for (int i = index; i < items.Length; i++)
                items[i].Hide();
        }

        private void OnClickItem(BoxUI ui)
        {
            World world = GameClient.Get().GetWorld();
            BattleCharacter active = world.battle.GetActiveCharacter();
            Card item = active?.GetItem(ui.GetUID());
            if (item != null && item.CardData.item_type == ItemType.ItemConsumable)
            {
                GameClient.Get().UseItem(active, item);
                active.cards_item.Remove(item);
                Refresh();
            }
        }

        public BoxUI GetHover()
        {
            foreach (BoxUI line in items)
            {
                if (line.IsHover())
                    return line;
            }
            return null;
        }

        public static InventoryUI Get()
        {
            return instance;
        }
    }
}
