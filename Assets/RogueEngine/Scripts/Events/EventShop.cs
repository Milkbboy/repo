using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Shop", order = 10)]
    public class EventShop : EventData
    {
        [Header("Shop")]
        public float buy_mult = 1f;
        public float sell_mult = 1f;
        public CardData[] cards;
        public CardData[] items;

        [Header("Randoms")]
        public int cards_random_amount = 0;  //Random cards are taken from the pool of valid cards for the champion
        public int items_random_amount = 0;
        public CardData[] items_random;

        public static List<EventShop> shop_list = new List<EventShop>();

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            Player player = logic.WorldData.GetPlayer(champion.player_id);

            logic.WorldData.state = WorldState.Shop;
            logic.WorldData.ResetActionCompleted();
            logic.WorldData.shop_buy_ratio = buy_mult + champion.ongoing_buy_factor;
            logic.WorldData.shop_sell_ratio = sell_mult + champion.ongoing_sell_factor;

            logic.WorldData.shop_items.Clear();
            foreach (CardData item in items)
            {
                if(item.IsAvailable(player.udata))
                    logic.WorldData.shop_items.Add(item.id);
            }

            logic.WorldData.shop_cards.Clear();
            foreach (CardData card in cards)
            {
                if (card.IsAvailable(player.udata))
                    logic.WorldData.shop_cards.Add(card.id);
            }

            List<CardData> unlock_cards = champion.ChampionData.GetRewardCards(player, null);
            if (unlock_cards.Count > 0)
            {
                int max = Mathf.Min(unlock_cards.Count, cards_random_amount);
                List<CardData> valid_cards = new List<CardData>(unlock_cards);
                List<CardData> shop_cards = logic.GetRandomCardsByProbability(valid_cards, max, champion.Hash + 5);
                foreach (CardData card in shop_cards)
                {
                    logic.WorldData.shop_cards.Add(card.id);
                }
            }

            if (items_random.Length > 0)
            {
                int max = Mathf.Min(items_random.Length, items_random_amount);
                List<CardData> valid_items = CardData.GetRewardItems(player.udata, items_random, null);
                List <CardData> shop_items = logic.GetRandomCardsByProbability(valid_items, max, champion.Hash + 7);
                foreach(CardData item in shop_items)
                {
                    logic.WorldData.shop_items.Add(item.id);
                }
            }
        }

        public static new EventShop Get(string id)
        {
            foreach (EventData evt in GetAll())
            {
                if (evt.id == id && evt is EventShop)
                    return evt as EventShop;
            }
            return null;
        }
    }
}
