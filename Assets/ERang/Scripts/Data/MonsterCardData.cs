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
                string assetPath = $"Assets/ERang/Resources/Cards/Monster/{cardEntity.Card_Id}.asset";
                CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

                if (cardData == null)
                {
                    // CardData 가 없으면 생성
                    cardData = CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(cardData, assetPath);
                }
                else
                {
                    // CardData 가 있으면 초기화
                    cardData.abilityIds.Clear();
                }

                cardData.Initialize(cardEntity);

                card_list.Add(cardData);
                card_dict.Add(cardData.card_id, cardData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static CardData GetCardData(int card_id)
        {
            return card_dict.TryGetValue(card_id, out CardData cardData) ? cardData : null;
        }
    }
}
