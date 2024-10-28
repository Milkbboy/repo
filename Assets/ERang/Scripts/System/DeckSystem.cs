using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class DeckSystem : MonoBehaviour
    {
        public int DeckCardCount => deckCards.Count;
        public int HandCardCount => handCards.Count;
        public int ExtinctionCardCount => extinctionCards.Count;
        public int GraveCardCount => graveCards.Count;

        public List<Card> DeckCards => deckCards;
        public List<Card> HandCards => handCards;
        public List<Card> GraveCards => graveCards;
        public List<Card> ExtinctionCards => extinctionCards;

        private readonly int maxHandCardCount = 5;
        private readonly System.Random random = new();

        private readonly List<Card> allCards = new();
        private readonly List<Card> deckCards = new(); // 뽑을 카드덱
        private readonly List<Card> handCards = new();
        private readonly List<Card> graveCards = new(); // 무덤 카드
        private readonly List<Card> extinctionCards = new(); // 소멸 카드
        // 이 녀석들은 보드에서 관리해 볼까?
        private readonly List<Card> creatureCards = new(); // 마스터 크리쳐 카드
        private readonly List<Card> buildingCards = new(); // 건물 카드

        private DeckUI deckUI;

        void Awake()
        {
            deckUI = GetComponent<DeckUI>();
        }

        public Card FindHandCard(string cardUid)
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
                    Debug.LogError($"CardData 테이블에 <color=red>{cardId}</color> 카드 없음");
                    continue;
                }

                // new 식 단순화(IDE0090). https://learn.microsoft.com/ko-kr/dotnet/fundamentals/code-analysis/style-rules/ide0090
                Card card = new(cardData);

                allCards.Add(card);
                deckCards.Add(card);
            }

            UpdateDeckCardCountUI();
        }

        /// <summary>
        /// 핸드 카드 생성
        /// </summary>
        /// <returns></returns>
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
                    Card card = deckCards[0];

                    // 덱에서 카드를 뽑아 손에 추가
                    deckCards.RemoveAt(0);
                    handCards.Add(card);
                }
            }

            yield return StartCoroutine(DrawHandDeck());
        }

        /// <summary>
        /// 핸드 카드 보드에 놓기
        /// </summary>
        public void HandCardToBoard(string cardUid)
        {
            Card card = handCards.Find(card => card.Uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드덱에 {card.Id} 카드 없음");
                return;
            }

            // 핸드 카드 제거
            handCards.Remove(card);
            deckUI.RemoveHandCard(cardUid);

            // 카드 타입별로 보드에 추가
            switch (card.Type)
            {
                case CardType.Creature: creatureCards.Add(card); break;
                case CardType.Building: buildingCards.Add(card); break;
            }

            UpdateDeckCardCountUI();
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
                Card card = handCards[i];

                handCards.RemoveAt(i);
                graveCards.Add(card);
            }

            deckUI.TurnEndRemoveHandCard();

            UpdateDeckCardCountUI();
        }

        /// <summary>
        /// 사용한 핸드 카드 제거
        /// </summary>
        public void RemoveUsedHandCard(string cardUid)
        {
            Card card = handCards.Find(card => card.Uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {card.Id} 카드 없음");
                return;
            }

            handCards.Remove(card);
            deckUI.RemoveHandCard(cardUid);

            RemoveCardProcess(card);
        }

        /// <summary>
        /// 보드 슬롯에 있는 카드 제거
        /// </summary>
        public void RemoveBoardCard(string cardUid)
        {
            // 크리쳐 먼저 찾고 없으면 건물 찾기
            Card card = creatureCards.Find(card => card.Uid == cardUid) ?? buildingCards.Find(card => card.Uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"보드 슬롯에 {cardUid} 카드 없음");
                return;
            }

            switch (card.Type)
            {
                case CardType.Creature:
                    creatureCards.Remove(card);
                    break;
                case CardType.Building:
                    buildingCards.Remove(card);
                    break;
                default:
                    Debug.LogError($"보드 슬롯에 {card.Id} 카드 {card.Type} 타입 없음");
                    break;
            }

            RemoveCardProcess(card);
        }

        /// <summary>
        /// 카드 랜덤 섞기
        /// </summary>
        /// <param name="cards">카드 리스트</param>
        private void ShuffleCards(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                Card temp = cards[i];
                int randomIdex = random.Next(i, cards.Count);
                cards[i] = cards[randomIdex];
                cards[randomIdex] = temp;
            }
        }

        /// <summary>
        /// 제거된 카드 설정
        /// </summary>
        private void RemoveCardProcess(Card card)
        {
            if (card.isExtinction)
                extinctionCards.Add(card);
            else
                graveCards.Add(card);

            UpdateDeckCardCountUI();
        }

        IEnumerator DrawHandDeck()
        {
            for (int i = 0; i < handCards.Count; ++i)
            {
                Card card = handCards[i];

                yield return deckUI.SpawnHandCard(card);

                UpdateDeckCardCountUI();
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