using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    [CreateAssetMenu(fileName = "card", menuName = "ERang/CardData", order = 1)]
    public class CardData : ScriptableObject
    {
        public string uid;

        [Header("Card Info")]
        public string cardName;
        public string description;

        [Header("Card Stats")]
        public int attack;
        public int hp;

        public static List<CardData> card_list = new List<CardData>();
        public static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

        public static void Load(string folder = "")
        {
            if (card_list.Count == 0)
            {
                card_list.AddRange(Resources.LoadAll<CardData>(folder));

                foreach (CardData card in card_list)
                {
                    card_dict.Add(card.uid, card);

                    Debug.Log("Card loaded: " + card.cardName);
                }
            }
        }
    }
}