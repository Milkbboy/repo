using System.Collections;
using System.Collections.Generic;
using ERang.Data;
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
        public int hp;
        public int atk;
        public int Mana { get; private set; }
        public int MaxMana { get; private set; }
        public static Master Instance { get; private set; }
        // 모든 카드
        public List<Card> allCards = new List<Card>();
        // 덱에 있는 카드
        public List<Card> deckCards = new List<Card>();
        // 손에 있는 카드
        public List<Card> handCards = new List<Card>();
        // 보드에 있는 크리쳐 카드
        public List<Card> boardCreatureCards = new List<Card>();
        // 필드에 있는 건물 카드
        public List<Card> boardBuildingCards = new List<Card>();
        // 무덤에 있는 카드
        public List<Card> graveCards = new List<Card>();
        // 소멸한 카드
        public List<Card> extinctionCards = new List<Card>();

        public Master(int masterId)
        {
            Instance = this;

            MasterData masterData = MasterData.GetMasterData(masterId);

            this.masterId = masterData.master_Id;
            hp = masterData.hp;
            atk = masterData.atk;
            Mana = masterData.startMana;
            MaxMana = masterData.maxMana;

            // 마스터 카드 생성
            foreach (int cardId in masterData.startCardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);
                Card card = new Card(cardData);

                allCards.Add(card);
                deckCards.Add(card);
            }
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

        public void IncreaseMana(int value)
        {
            Mana += value;

            if (Mana > MaxMana)
            {
                Mana = MaxMana;
            }
        }

        public void DecreaseMana(int value)
        {
            Mana -= value;

            if (Mana < 0)
            {
                Mana = 0;
            }
        }

        public void HandCardToBoard(string cardUid, CardType cardType)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"HandCardToBoard: card is null({card.id})");
                return;
            }

            handCards.Remove(card);

            if (cardType == CardType.Creature)
            {
                boardCreatureCards.Add(card);

                Debug.Log($"HandCardToBoard: {cardUid}, HandCardCount: {handCards.Count}, boardCreatureCardCount: {boardCreatureCards.Count}");
            }
            else if (cardType == CardType.Building)
            {
                boardBuildingCards.Add(card);

                Debug.Log($"HandCardToBoard: {card.id}, HandCardCount: {handCards.Count}, boardBuildingCardCount: {boardBuildingCards.Count}");
            }
        }

        public void HandCardToGrave(string cardUid)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"HandCardToGrave: card is null({card.id})");
                return;
            }

            handCards.Remove(card);
            graveCards.Add(card);

            Debug.Log($"HandCardToGrave: {card.id}, HandCardCount: {handCards.Count}, GraveCardCount: {graveCards.Count}");
        }

        public void HandCardToExtinction(string cardUid)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"HandCardToExtinction: card is null({card.id})");
                return;
            }

            handCards.Remove(card);
            extinctionCards.Add(card);

            Debug.Log($"HandCardToExtinction: {card.id}, HandCardCount: {handCards.Count}, ExtinctionCardCount: {extinctionCards.Count}");
        }
    }
}