using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class MonsterCardData
    {
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
                if (CardData.card_dict.ContainsKey(cardEntity.Card_Id))
                    continue;

                CardData cardData = new CardData();

                cardData.Initialize(cardEntity);

                CardData.card_list.Add(cardData);
                CardData.card_dict.Add(cardData.card_id, cardData);
            }
        }
    }
}
