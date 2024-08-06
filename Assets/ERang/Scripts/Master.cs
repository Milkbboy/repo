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
        public int mana;
        public int maxMana;
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

        public Master(MasterData masterData)
        {
            Instance = this;
            this.masterId = masterData.master_Id;
            this.hp = masterData.hp;
            this.atk = masterData.atk;
            this.mana = masterData.startMana;
            this.maxMana = masterData.maxMana;
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
            mana += value;

            if (mana > maxMana)
            {
                mana = maxMana;
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

        public int GetMana()
        {
            return mana;
        }

        public int GetMaxMana()
        {
            return maxMana;
        }

        public bool CanUseCard(string cardUid)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"CanUseCard: card is null({card.id})");
                return false;
            }

            if (mana < card.costMana)
            {
                Debug.LogError($"CanUseCard: mana is not enough({card.id}, {mana} < {card.costMana})");
                return false;
            }

            return true;
        }

        public void HandCardToBoard(string cardUid, CardType cardType)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"HandCardToBoard: card is null({cardUid})");
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

                Debug.Log($"HandCardToBoard: {cardUid}, HandCardCount: {handCards.Count}, boardBuildingCardCount: {boardBuildingCards.Count}");
            }
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

            Debug.Log($"HandCardToGrave: {cardUid}, HandCardCount: {handCards.Count}, GraveCardCount: {graveCards.Count}");

            Actions.OnGraveDeckCountChanged?.Invoke();
        }
    }
}