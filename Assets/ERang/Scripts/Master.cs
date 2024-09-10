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

        private int masterId;
        private int hp;
        private int maxHp;
        private int mana;
        private int maxMana;
        private int rechargeMana;
        private int atk;
        private int def;
        private int gold;
        public int MasterId { get { return masterId; } }
        public int Hp { get { return hp; } }
        public int MaxHp { get { return maxHp; } }
        public int Mana { get { return mana; } }
        public int MaxMana { get { return maxMana; } }
        public int RechargeMana { get { return rechargeMana; } }
        public int Atk { get { return atk; } }
        public int Def { get { return def; } }
        public int Gold { get { return gold; } }

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
            mana = 0;
            maxMana = masterData.maxMana;
            rechargeMana = masterData.rechargeMana;
            atk = masterData.atk;
            def = masterData.def;
            gold = 1000; // 임시로 1000 골드로 설정

            // 마스터 카드 생성
            foreach (int cardId in masterData.startCardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"CardData 테이블에 카드({cardId}) 없음");
                    continue;
                }

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

        public void chargeMana()
        {
            mana += rechargeMana;

            if (mana > MaxMana)
                mana = MaxMana;
        }

        public void resetMana()
        {
            mana = 0;
        }

        public void IncreaseMana(int value)
        {
            mana += value;
        }

        public void DecreaseMana(int value)
        {
            mana -= value;

            if (mana < 0)
                mana = 0;
        }

        public void AddGold(int gold)
        {
            this.gold += gold;
        }

        public void HandCardToBoard(string cardUid)
        {
            Card card = GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드 카드덱에 카드({card.id}) 없음");
                return;
            }

            // 핸드 카드에서 카드 제거
            handCards.Remove(card);

            // 보드에 카드 추가
            switch (card.type)
            {
                case CardType.Creature: boardCreatureCards.Add(card); break;
                case CardType.Building: boardBuildingCards.Add(card); break;
            }
        }

        /// <summary>
        /// 턴 종료 핸드 카드 제거
        /// </summary>
        public void RemoveTurnEndHandCard(string cardUid)
        {
            Card card = handCards.Find(card => card.uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {card.id} 카드 없음 - RemoveHandCard");
                return;
            }

            handCards.Remove(card);
            graveCards.Add(card);
        }

        /// <summary>
        /// 사용한 핸드 카드 제거
        /// </summary>
        public void RemoveUsedHandCard(string cardUid)
        {
            Card card = handCards.Find(card => card.uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {card.id} 카드 없음 - RemoveHandUseCard");
                return;
            }

            handCards.Remove(card);

            if (card.isExtinction)
                extinctionCards.Add(card);
            else
                graveCards.Add(card);
        }

        /// <summary>
        /// 크리쳐 카드에서 먼저 찾고 없으면 건물 카드에서 찾기
        /// </summary>
        public void RemoveBoardCard(string cardUid)
        {
            Card card = boardCreatureCards.Find(card => card.uid == cardUid) ?? boardBuildingCards.Find(card => card.uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"보드 슬롯에 {cardUid} 카드 없음 - RemoveBoardCard");
                return;
            }

            switch (card.type)
            {
                case CardType.Creature:
                    boardCreatureCards.Remove(card);
                    break;
                case CardType.Building:
                    boardBuildingCards.Remove(card);
                    break;
                default:
                    Debug.LogError($"보드 슬롯에 {card.type} 카드 타입 없음 - RemoveBoardCard");
                    break;
            }

            if (card.isExtinction)
                extinctionCards.Add(card);
            else
                graveCards.Add(card);
        }
    }
}