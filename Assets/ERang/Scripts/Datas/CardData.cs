using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ERang
{
    public class CardData : ScriptableObject
    {
        public int card_id;
        public string cardNameDesc_id;
        public string cardDesc_id;
        public string cardType;
        public int creatureAI_id;
        public int costMana;
        public int hp;
        public int atk;
        public int def;
        public string ability_id;
        public string extinction;
        public string completeExtinction;

        public void Initialize(CardEntity cardEntity)
        {
            card_id = cardEntity.Card_id;
            cardNameDesc_id = cardEntity.CardNameDesc_id;
            cardDesc_id = cardEntity.CardDesc_id;
            cardType = cardEntity.CardType;
            creatureAI_id = cardEntity.CreatureAI_id;
            costMana = cardEntity.CostMana;
            hp = cardEntity.Hp;
            atk = cardEntity.Atk;
            def = cardEntity.Def;
            ability_id = cardEntity.Ability_id;
            extinction = cardEntity.Extinction;
            completeExtinction = cardEntity.CompleteExtinction;
        }

        public static List<CardData> card_list = new List<CardData>();
        public static Dictionary<int, CardData> card_dict = new Dictionary<int, CardData>();

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
                string assetPath = $"Assets/ERang/Resources/Cards/{cardEntity.Card_id}.asset";
                CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

                if (cardData == null)
                {
                    // CardData 가 없으면 생성
                    cardData = CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(cardData, assetPath);
                }

                cardData.Initialize(cardEntity);

                card_list.Add(cardData);
                card_dict.Add(cardData.card_id, cardData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("CardData loaded");
        }

        public static CardData GetCardData(int card_id)
        {
            return card_dict[card_id];
        }
    }
}