using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    [CreateAssetMenu(fileName = "card", menuName = "ERang/CardData", order = 1)]
    public class CardData : ScriptableObject
    {
        public static List<CardEntity> card_list = new List<CardEntity>();
        public static Dictionary<string, CardEntity> card_dict = new Dictionary<string, CardEntity>();

        public static void Load(string path = "")
        {
            if (card_list.Count == 0)
            {
                CardItems cardItems = Resources.Load<CardItems>(path);

                if (cardItems != null)
                {
                    foreach (CardEntity card in cardItems.cards)
                    {
                        card_list.Add(card);
                        card_dict.Add(card.uid, card);

                        Debug.Log("Card loaded: " + card.uid);
                    }
                }
                else
                {
                    Debug.LogError("CardItems not found");
                }
            }
        }
    }
}