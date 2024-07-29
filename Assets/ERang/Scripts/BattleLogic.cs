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
            master = new Master();

            // 처음 시작이면 플레이어 startDeck 설정
            if (turn == 0)
            {
                MasterData masterData = MasterData.GetMasterData(masterId);

                foreach (int cardId in masterData.startCardIds)
                {
                    CardData cardData = CardData.GetCardData(cardId);
                    Card card = new Card(cardData);
                    master.allCards.Add(card);
                    master.deckCards.Add(card);
                }

                // deck 카드 섞기
                ShuffleDeck(master.deckCards);

                // draw staring card
                DrawCard(handCardCount);
            }

            // StartTurn();
        }

        // Update is called once per frame
        void Update()
        {

        }

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

        void DrawCard(int cardCount)
        {
            for (int i = 0; i < cardCount; ++i)
            {
                if (master.deckCards.Count == 0)
                {
                    // 덱에 카드가 없으면 무덤에 있는 카드를 덱으로 옮김
                    master.deckCards.AddRange(master.graveCards);
                    master.graveCards.Clear();
                    // 덱 카드 섞기
                    ShuffleDeck(master.deckCards);
                }

                Card card = master.deckCards[0];

                // 덱에서 카드를 뽑아 손에 추가
                master.deckCards.RemoveAt(0);
                master.handCards.Add(card);

                HandDeck.Instance.SpawnNewCard(card);
            }

            HandDeck.Instance.DrawCards();
        }

        public Master GetMaster()
        {
            return master;
        }
    }
}