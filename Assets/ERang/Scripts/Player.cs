using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        public int AllCardCount => allCards.Count;
        public List<BaseCard> AllCards => allCards;

        [SerializeField] private List<BaseCard> allCards = new List<BaseCard>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("Player 생성됨");
            }
            else if (Instance != this)
            {
                Debug.Log("Player 파괴됨");
                Destroy(gameObject);
            }
        }

        public void AddCard(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);

            if (cardData == null)
            {
                Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음 - AddCard");
                return;
            }

            BaseCard card = Utils.MakeCard(cardData);

            // 카드 타입별로 생성
            allCards.Add(card);
        }
    }
}