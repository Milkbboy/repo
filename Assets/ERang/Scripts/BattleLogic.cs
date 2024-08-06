using System;
using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }
        public HandDeck handDeck;

        public int turn = 0;
        public int masterId = 1001;
        public int handCardCount = 5;

        private Master master;
        private System.Random random = new System.Random();

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            InitMaster(masterId);

            TurnStart();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void TurnStart()
        {
            // deck 카드 섞기
            ShuffleDeck(master.deckCards);

            // draw staring card
            StartCoroutine(DrawHandDeckCard(handCardCount));

            Actions.OnTurnChanged?.Invoke(turn);
        }

        public void TurnEnd()
        {
            turn += 1;

            // 핸드 데크에 있는 카드를 모두 무덤으로 이동
            if (master.handCards.Count > 0)
            {
                master.graveCards.AddRange(master.handCards);

                for (int i = 0; i < master.handCards.Count; ++i)
                {
                    HandDeck.Instance.RemoveCard(master.handCards[i].uid);
                }

                master.handCards.Clear();
            }

            Actions.OnTurnChanged?.Invoke(turn);

            TurnStart();
        }

        void InitMaster(int masterId)
        {
            MasterData masterData = MasterData.GetMasterData(masterId);

            this.master = new Master(masterId);

            foreach (int cardId in masterData.startCardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);
                Card card = new Card(cardData);

                master.allCards.Add(card);
                master.deckCards.Add(card);
            }

            Actions.OnDeckCountChange?.Invoke();
        }

        public Master GetMaster()
        {
            return master;
        }

        // 카드 섞기
        void ShuffleDeck(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                Card temp = cards[i];
                int randomIdex = random.Next(i, cards.Count);
                cards[i] = cards[randomIdex];
                cards[randomIdex] = temp;
            }
        }

        // 카드 그리기
        IEnumerator DrawHandDeckCard(int cardCount)
        {
            int spawnCount = cardCount - master.handCards.Count;

            Debug.Log($"DrawHandDeckCard. masterHandCardCount: {master.handCards.Count}, spawnCount: {spawnCount}");

            for (int i = 0; i < spawnCount; ++i)
            {
                if (master.deckCards.Count == 0)
                {
                    // 덱에 카드가 없으면 무덤에 있는 카드를 덱으로 옮김
                    master.deckCards.AddRange(master.graveCards);
                    master.graveCards.Clear();

                    // 덱 카드 섞기
                    ShuffleDeck(master.deckCards);
                }

                if (master.deckCards.Count > 0)
                {
                    Card card = master.deckCards[0];

                    // 덱에서 카드를 뽑아 손에 추가
                    master.deckCards.RemoveAt(0);
                    master.handCards.Add(card);

                    HandDeck.Instance.SpawnNewCard(card);
                    HandDeck.Instance.DrawCards();
                }

                yield return new WaitForSeconds(.2f);
            }

            Actions.OnDeckCountChange?.Invoke();
            Actions.OnGraveDeckCountChanged?.Invoke();
        }
    }
}