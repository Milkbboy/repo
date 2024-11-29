using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class DeckSystem : MonoBehaviour
    {
        public static DeckSystem Instance { get; private set; }

        public HandDeck handDeck;
        public Transform gravePosition;

        public int DeckCardCount => deckCards.Count;
        public int HandCardCount => handCards.Count;
        public int ExtinctionCardCount => extinctionCards.Count;
        public int GraveCardCount => graveCards.Count;

        public List<BaseCard> DeckCards => deckCards;
        public List<BaseCard> HandCards => handCards;
        public List<BaseCard> GraveCards => graveCards;
        public List<BaseCard> ExtinctionCards => extinctionCards;
        public List<BaseCard> BuildingCards => buildingCards;

        private readonly int maxHandCardCount = 5;
        private readonly System.Random random = new();

        private readonly List<BaseCard> creatureCards = new(); // 마스터 크리쳐 카드
        private readonly List<BaseCard> buildingCards = new(); // 건물 카드

        private readonly List<BaseCard> allCards = new();
        private readonly List<BaseCard> deckCards = new();
        private readonly List<BaseCard> handCards = new();
        private readonly List<BaseCard> graveCards = new();
        private readonly List<BaseCard> extinctionCards = new();

        private DeckUI deckUI;

        void Awake()
        {
            Instance = this;

            deckUI = GetComponent<DeckUI>();
        }

        public BaseCard FindHandCard(string cardUid)
        {
            return handCards.Find(card => card.Uid == cardUid);
        }

        /// <summary>
        /// 마스터 덱 카드 생성
        /// </summary>
        public void CreateMasterCards(List<int> cardIds)
        {
            foreach (int cardId in cardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음");
                    continue;
                }

                BaseCard card = Utils.MakeCard(cardData);

                // 카드 타입별로 생성
                allCards.Add(card);
                deckCards.Add(card);
            }

            deckUI.SetDeckCardCount(DeckCardCount);
        }

        /// <summary>
        /// 핸드 카드 생성
        /// </summary>
        public IEnumerator MakeHandCards()
        {
            // 덱 카드 섞기
            ShuffleCards(deckCards);

            int spawnCount = maxHandCardCount - HandCardCount;

            // Debug.Log($"DrawHandDeckCard. masterHandCardCount: {HandCardCount}, spawnCount: {spawnCount}");

            for (int i = 0; i < spawnCount; ++i)
            {
                // 덱에 카드가 없으면 무덤 카드를 덱으로 옮김
                if (DeckCards.Count == 0)
                {
                    deckCards.AddRange(graveCards);
                    graveCards.Clear();

                    // 덱 카드 섞기
                    ShuffleCards(deckCards);
                }

                if (deckCards.Count > 0)
                {
                    BaseCard card = deckCards[0];

                    // 덱에서 카드를 뽑아 손에 추가
                    deckCards.RemoveAt(0);
                    handCards.Add(card);
                }
            }

            yield return StartCoroutine(DrawHandDeck());

            deckUI.SetDeckCardCount(DeckCardCount);
        }

        /// <summary>
        /// 핸드 카드를 보드로 이동
        /// </summary>
        public void HandCardToBoard(BaseCard card)
        {
            if (card == null)
            {
                Debug.LogError($"핸드덱에 {card.Id} 카드 없음");
                return;
            }

            RemoveHandCard(card);

            switch (card)
            {
                case CreatureCard creatureCard:
                    creatureCards.Add(creatureCard);
                    break;

                case BuildingCard buildingCard:
                    buildingCards.Add(buildingCard);
                    break;
            }

            deckUI.SetDeckCardCount(DeckCardCount);
        }

        /// <summary>
        /// 턴 종료 핸드 카드 제거
        /// - 핸드 카드는 무덤으로만 이동
        /// </summary>
        public void RemoveTurnEndHandCard()
        {
            // 이 코드는 
            // InvalidOperationException: Collection was modified; enumeration operation may not execute 오류 발생
            // 컬렉션을 열거하는 동안 해당 컬렉션이 수정될 때 발생합니다.
            // foreach 로 handCards 열거하면서 handCards.Remove(card)로 핸드 카드 제거해서 발생하는 오류
            // foreach (Card card in handCards)
            // {
            //     handCards.Remove(card);
            //     graveCards.Add(card);
            // }

            for (int i = handCards.Count - 1; i >= 0; i--)
            {
                BaseCard card = handCards[i];

                handCards.RemoveAt(i);
                graveCards.Add(card);
            }

            handDeck.TurnEndRemoveHandCard(gravePosition);

            deckUI.SetDeckCardCount(DeckCardCount);
            deckUI.SetGraveCardCount(GraveCardCount);
        }

        /// <summary>
        /// 사용한 핸드 카드 제거
        /// </summary>
        public void RemoveUsedHandCard(string cardUid)
        {
            BaseCard card = handCards.Find(card => card.Uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {card.Id} 카드 없음");
                return;
            }

            handCards.Remove(card);
            handDeck.RemoveHandCard(cardUid);

            if (card.IsExtinction)
                extinctionCards.Add(card);
            else
                graveCards.Add(card);

            deckUI.SetDeckCardCount(DeckCardCount);
            deckUI.SetGraveCardCount(GraveCardCount);
            deckUI.SetExtinctionCardCount(ExtinctionCardCount);
        }

        public void UpdateHandCardUI()
        {
            handDeck.UpdateHandCardUI();
        }

        /// <summary>
        /// 카드 랜덤 섞기
        /// </summary>
        /// <param name="cards">카드 리스트</param>
        private void ShuffleCards(List<BaseCard> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                BaseCard temp = cards[i];
                int randomIdex = random.Next(i, cards.Count);
                cards[i] = cards[randomIdex];
                cards[randomIdex] = temp;
            }
        }

        private void RemoveHandCard(BaseCard card)
        {
            handCards.Remove(card);
            handDeck.RemoveHandCard(card.Uid);
        }

        private IEnumerator DrawHandDeck()
        {
            for (int i = 0; i < handCards.Count; ++i)
            {
                BaseCard card = handCards[i];

                yield return handDeck.SpawnHandCard(card);
            }
        }

        private void UpdateDeckCardCountUI()
        {
            deckUI.SetDeckCardCount(DeckCardCount);
            deckUI.SetGraveCardCount(GraveCardCount);
            deckUI.SetExtinctionCardCount(ExtinctionCardCount);
        }
    }
}