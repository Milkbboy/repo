using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ERang
{
    public class DeckData
    {
        // 카드 컬렉션들
        private readonly List<BaseCard> deckCards = new List<BaseCard>();
        private readonly List<BaseCard> handCards = new();
        private readonly List<BaseCard> graveCards = new();
        private readonly List<BaseCard> extinctionCards = new();
        private readonly List<BaseCard> buildingCards = new();

        // 외부 읽기 전용 프로퍼티들
        public int DeckCardCount => deckCards.Count;
        public int HandCardCount => handCards.Count;
        public int ExtinctionCardCount => extinctionCards.Count;
        public int GraveCardCount => graveCards.Count;

        public IReadOnlyList<BaseCard> DeckCards => deckCards.AsReadOnly();
        public IReadOnlyList<BaseCard> HandCards => handCards.AsReadOnly();
        public IReadOnlyList<BaseCard> GraveCards => graveCards.AsReadOnly();
        public IReadOnlyList<BaseCard> ExtinctionCards => extinctionCards.AsReadOnly();
        public IReadOnlyList<BaseCard> BuildingCards => buildingCards.AsReadOnly();

        // 외부에서 호출 가능한 조작 메서드들
        public void AddToDeck(BaseCard card) => deckCards.Add(card);
        public void AddToHand(BaseCard card) => handCards.Add(card);
        public void AddToGrave(BaseCard card) => graveCards.Add(card);
        public void AddToExtinction(BaseCard card) => extinctionCards.Add(card);
        public void AddToBuilding(BaseCard card) => buildingCards.Add(card);

        public void RemoveFromDeck(BaseCard card) => deckCards.Remove(card);
        public void RemoveFromHand(BaseCard card) => handCards.Remove(card);
        public void RemoveFromGrave(BaseCard card) => graveCards.Remove(card);

        public void ShuffleDeck()
        {
            System.Random random = new();

            for (int i = 0; i < deckCards.Count; ++i)
            {
                BaseCard temp = deckCards[i];
                int randomIdex = random.Next(i, deckCards.Count);
                deckCards[i] = deckCards[randomIdex];
                deckCards[randomIdex] = temp;
            }
        }

        // NextTurnSelect 카드를 덱에서 찾아 핸드로 우선 이동
        public List<BaseCard> ExtractNextTurnSelectCards()
        {
            // 이 코드는 
            // InvalidOperationException: Collection was modified; enumeration operation may not execute 오류 발생
            // 컬렉션을 열거하는 동안 해당 컬렉션이 수정될 때 발생합니다.
            // foreach 로 deckCards 열거하면서 deckCards.Remove(card)로 카드를 제거해서 발생하는 오류
            // foreach (BaseCard card in deckCards)
            // {
            //     nextTurnCards.Add(card);
            //     deckCards.Remove(card);
            // }
            var nextTurnCards = new List<BaseCard>();

            for (int i = deckCards.Count - 1; i >= 0; --i)
            {
                BaseCard card = deckCards[i];
                if ((card.Traits & CardTraits.NextTurnSelect) == CardTraits.NextTurnSelect)
                {
                    nextTurnCards.Add(card);
                    deckCards.RemoveAt(i);
                }
            }

            return nextTurnCards;
        }

        public void MoveGraveToDeck()
        {
            foreach (BaseCard graveCard in graveCards)
            {
                graveCards.Remove(graveCard);
                deckCards.Add(graveCard);
            }
        }

        public void Clear()
        {
            deckCards.Clear();
            handCards.Clear();
            graveCards.Clear();
            extinctionCards.Clear();
            buildingCards.Clear();
        }

        public BaseCard FindHandCard(string cardUid)
        {
            return handCards.Find(card => card.Uid == cardUid);
        }

        public bool ContainsHandCard(string cardUid)
        {
            return handCards.Any(card => card.Uid == cardUid);
        }

        public BaseCard DrawToCard()
        {
            if (deckCards.Count == 0) return null;

            BaseCard card = deckCards[0];
            deckCards.RemoveAt(0);

            return card;
        }
    }
}
