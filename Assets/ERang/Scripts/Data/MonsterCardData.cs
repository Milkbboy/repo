using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    public class MonsterCardData : ScriptableObject
    {
        public static List<CardData> card_list = new List<CardData>();
        public static Dictionary<int, CardData> card_dict = new Dictionary<int, CardData>();

        public static void Load(string path = "")
        {
            // 엑셀로 생성된 ScriptableObject 로드
            MonsterCardDataTable cardDataTable = Resources.Load<MonsterCardDataTable>(path);

            if (cardDataTable == null)
            {
                Debug.LogError("CardDataTable not found");
                return;
            }

            foreach (var cardEntity in cardDataTable.items)
            {
                if (card_dict.ContainsKey(cardEntity.Card_Id))
                    continue;

                CardData cardData = new CardData();

                cardData.Initialize(cardEntity);

                card_list.Add(cardData);
                card_dict.Add(cardData.card_id, cardData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 카드 id, name을 반환
        /// </summary>
        /// <returns></returns>
        public static List<(int, string)> GetCardIdNames()
        {
            List<(int, string)> cardIds = new();

            foreach (var card in card_list)
            {
                cardIds.Add((card.card_id, card.nameDesc));
            }

            return cardIds;
        }

        public static CardData GetCardData(int card_id)
        {
            return card_dict.TryGetValue(card_id, out CardData cardData) ? cardData : null;
        }
    }
}
