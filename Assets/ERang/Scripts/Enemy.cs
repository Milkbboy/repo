using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ERang.Data;

namespace ERang
{
    public class Enemy
    {
        public int enemyId;
        public int hp;
        public int atk;
        public static Enemy Instance { get; private set; }

        // 모든 카드
        public List<Card> monsterCards = new List<Card>();

        public Enemy(int enemyId)
        {
            Instance = this;

            MasterData enemyData = MasterData.GetMasterData(enemyId);

            this.enemyId = enemyData.master_Id;
            hp = enemyData.hp;
            atk = enemyData.atk;

            // 적 카드 생성
            foreach (int cardId in enemyData.startCardIds)
            {
                CardData cardData = MonsterCardData.GetCardData(cardId);

                if (cardData.cardType == CardType.Monster)
                {
                    Card card = new Card(cardData);
                    monsterCards.Add(card);
                }
            }
        }

        public List<Card> GetMonsterCards()
        {
            return monsterCards;
        }

        public Card GetMonsterCard(string cardUid)
        {
            foreach (Card card in monsterCards)
            {
                if (card.uid == cardUid)
                {
                    return card;
                }
            }

            return null;
        }

        public void RemoveMonsterCard(string cardUid)
        {
            Card card = GetMonsterCard(cardUid);
            monsterCards.Remove(card);
        }
    }
}