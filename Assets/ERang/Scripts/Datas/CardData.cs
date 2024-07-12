using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ERang
{
    public class CardData : ScriptableObject
    {
        public string uid;
        public string cardName;
        public string desc;
        public int cost;
        public int atk;
        public int def;
        public int hp;

        public void Initialize(CardEntity cardEntity)
        {
            this.uid = cardEntity.uid;
            this.cardName = cardEntity.name;
            this.desc = cardEntity.desc;
            this.cost = cardEntity.cost;
            this.atk = cardEntity.atk;
            this.def = cardEntity.def;
            this.hp = cardEntity.hp;
        }

        public static List<CardData> card_list = new List<CardData>();
        public static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

        public static void Load(string path = "")
        {
            // 엑셀로 생성된 ScriptableObject 로드
            ExcelCard cardItems = Resources.Load<ExcelCard>(path);

            if (cardItems == null)
            {
                Debug.LogError("CardData not loaded");
                return;
            }

            foreach (var cardEntity in cardItems.items)
            {
                string assetPath = $"Assets/ERang/Resources/Cards/{cardEntity.name}.asset";
                CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

                if (cardData == null)
                {
                    // CardData 가 없으면 생성
                    cardData = CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(cardData, assetPath);
                }

                cardData.Initialize(cardEntity);

                card_list.Add(cardData);
                card_dict.Add(cardData.uid, cardData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("CardData loaded");
        }
    }
}