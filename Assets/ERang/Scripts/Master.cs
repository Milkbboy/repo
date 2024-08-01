using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /**
    * 마스터 클래스
    * - 모든 카드를 관리
    */
    public class Master
    {
        public int masterId;
        public static Master Instance { get; private set; }
        // 모든 카드
        public List<Card> allCards = new List<Card>();
        // 덱에 있는 카드
        public List<Card> deckCards = new List<Card>();
        // 손에 있는 카드
        public List<Card> handCards = new List<Card>();
        // 보드에 있는 크리쳐 카드
        public List<Card> boardCards = new List<Card>();
        // 필드에 있는 건물 카드
        public List<Card> fieldCards = new List<Card>();
        // 무덤에 있는 카드
        public List<Card> graveCards = new List<Card>();
        // 소멸한 카드
        public List<Card> extinctionCards = new List<Card>();

        public Master(int masterId)
        {
            Instance = this;
            this.masterId = masterId;
        }

        ~Master()
        {
        }

        public Card GetHandCard(string cardUid)
        {
            foreach (Card card in handCards)
            {
                if (card.uid == cardUid)
                {
                    return card;
                }
            }

            return null;
        }

        public void HandCardToBoard(string cardUid)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"HandCardToBoard: card is null({cardUid})");
                return;
            }

            handCards.Remove(card);
            boardCards.Add(card);

            Debug.Log($"HandCardToBoard: {cardUid}");
        }

        public void HandCardToGrave(string cardUid)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"HandCardToGrave: card is null({cardUid})");
                return;
            }

            handCards.Remove(card);
            graveCards.Add(card);

            Debug.Log($"HandCardToGrave: {cardUid}");
        }
    }
}