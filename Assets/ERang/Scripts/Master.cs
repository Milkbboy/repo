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
        public static Master Instance { get; private set; }

        public int masterId;
        public int hp;
        public int maxHp;
        public int mana;
        public int maxMana;
        public int atk;
        public int def;
        public int Hp { get { return hp; } }
        public int MaxHp { get { return maxHp; } }
        public int Mana { get { return mana; } }
        public int MaxMana { get { return maxMana; } }
        public int Atk { get { return atk; } }
        public int Def { get { return def; } }

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
            maxHp = hp = masterData.hp;
            mana = masterData.startMana;
            maxMana = masterData.maxMana;
            atk = masterData.atk;
            def = masterData.def;

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
            return handCards.Find(card => card.uid == cardUid);
        }

        public void IncreaseMana(int value)
        {
            mana += value;

            if (mana > MaxMana)
            {
                mana = MaxMana;
            }
        }

        public void DecreaseMana(int value)
        {
            mana -= value;

            if (mana < 0)
            {
                mana = 0;
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
            }
            else if (cardType == CardType.Building)
            {
                boardBuildingCards.Add(card);
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

            // Debug.Log($"HandCardToGrave: {card.id}, HandCardCount: {handCards.Count}, GraveCardCount: {graveCards.Count}");
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

            // Debug.Log($"HandCardToExtinction: {card.id}, HandCardCount: {handCards.Count}, ExtinctionCardCount: {extinctionCards.Count}");
        }

        public Card GetBoardCreatureCard(string cardUid)
        {
            return boardCreatureCards.Find(card => card.uid == cardUid);
        }

        public List<Card> GetCreatureCards()
        {
            return boardCreatureCards;
        }

        /// <summary>
        /// 보드 크리쳐 카드를 소멸로 이동
        /// </summary>
        /// <param name="cardUid"></param>
        public void BoardCreatureCardToExtinction(string cardUid)
        {
            Card card = GetBoardCreatureCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"BoardSlotCardToGrave: card is null({card.id})");
                return;
            }

            boardCreatureCards.Remove(card);
            extinctionCards.Add(card);

            // Debug.Log($"BoardSlotCardToGrave: {card.id}, BoardCreatureCardCount: {boardCreatureCards.Count}, GraveCardCount: {graveCards.Count}");
        }
    }
}