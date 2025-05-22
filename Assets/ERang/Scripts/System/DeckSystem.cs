using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class DeckSystem : MonoBehaviour
    {
        public static DeckSystem Instance { get; private set; }

        public int DeckCardCount => deckCards.Count;
        public int HandCardCount => handCards.Count;
        public int ExtinctionCardCount => extinctionCards.Count;
        public int GraveCardCount => graveCards.Count;

        public List<GameCard> DeckCards => deckCards;
        public List<GameCard> HandCards => handCards;
        public List<GameCard> GraveCards => graveCards;
        public List<GameCard> ExtinctionCards => extinctionCards;
        public List<GameCard> BuildingCards => buildingCards;

        private bool isCreatedStarCard = false;
        private readonly int maxHandCardCount = 5;
        private readonly System.Random random = new();

        private readonly List<GameCard> creatureCards = new(); // 마스터 크리쳐 카드
        private readonly List<GameCard> buildingCards = new(); // 건물 카드

        [SerializeField] private List<GameCard> deckCards = new List<GameCard>();
        private List<GameCard> handCards = new();
        private readonly List<GameCard> graveCards = new();
        private readonly List<GameCard> extinctionCards = new();

        void Awake()
        {
            Instance = this;
        }

        public GameCard FindHandCard(string cardUid)
        {
            return handCards.Find(card => card.Uid == cardUid);
        }

        /// <summary>
        /// 마스터 덱 카드 생성
        /// </summary>
        public void CreateMasterCards(MasterCard masterCard)
        {
            if (isCreatedStarCard == true)
            {
                Debug.LogError("이미 마스터 시작 카드 생성으로 allCards => deckCards 복사");

                Clear();

                // allCards 를 deckCards 로 복사
                deckCards.AddRange(Player.Instance.AllCards);

                Debug.Log($"deckCards.Count: {deckCards.Count}, {string.Join(", ", deckCards.Select(card => card.Id))}");
                return;
            }

            foreach (int cardId in masterCard.CardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음");
                    continue;
                }

                GameCard card = Utils.MakeCard(cardData);

                if (card is MagicCard magicCard)
                    magicCard.SetSelectAttackType(AiLogic.Instance.IsSelectAttackType(card));

                Debug.Log($"CreateMasterCards. cardId: {cardId}, card: {card}, {card.LogText}");

                // 카드 타입별로 생성
                Player.Instance.AllCards.Add(card);
                deckCards.Add(card);
            }

            isCreatedStarCard = true;
        }

        /// <summary>
        /// 핸드 카드 생성
        /// </summary>
        public void MakeHandCards()
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
                    GameCard card = deckCards[0];

                    // 덱에서 카드를 뽑아 손에 추가
                    deckCards.RemoveAt(0);
                    handCards.Add(card);
                }
            }

            // Traits가 NextTurnSelect인 카드를 앞으로 이동시키고 Traits를 None으로 설정
            handCards = handCards.OrderByDescending(card => (card.Traits & CardTraits.NextTurnSelect) == CardTraits.NextTurnSelect).ToList();

            // 핸드 카드 어빌리티 설정
            foreach (var card in handCards)
            {
                // Traits를 None으로 설정
                if ((card.Traits & CardTraits.NextTurnSelect) == CardTraits.NextTurnSelect)
                {
                    card.SetTraits(CardTraits.None);
                }

                Debug.Log($"MakeHandCards. card: {card}, {card.LogText}, handAbilities.Count: {card.AbilitySystem.HandAbilities.Count}");

                StartCoroutine(AbilityLogic.Instance.HandCardAbilityAction(card));
            }

            // Debug.Log($"MakeHandCards. handCards.Count: {handCards.Count}, {string.Join(", ", handCards.Select(card => card.Uid))}");
        }

        /// <summary>
        /// 핸드 카드를 보드로 이동
        /// </summary>
        public void HandCardToBoard(GameCard card)
        {
            if (card == null)
            {
                Debug.LogError($"핸드덱에 {card.Id} 카드 없음");
                return;
            }

            // 핸드 카드 어빌리티 해제
            StartCoroutine(AbilityLogic.Instance.HandCardAbilityRelease(card));

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
                GameCard card = handCards[i];

                // 핸드 카드 어빌리티 해제
                StartCoroutine(AbilityLogic.Instance.HandCardAbilityRelease(card));

                RemoveHandCard(card);
                graveCards.Add(card);
            }
        }

        /// <summary>
        /// 사용한 핸드 카드 제거
        /// </summary>
        public void RemoveUsedHandCard(string cardUid)
        {
            GameCard card = handCards.Find(card => card.Uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {card.Id} 카드 없음");
                return;
            }

            // 핸드 카드 어빌리티 해제
            StartCoroutine(AbilityLogic.Instance.HandCardAbilityRelease(card));

            RemoveHandCard(card);

            if (card.IsExtinction)
                extinctionCards.Add(card);
            else
                graveCards.Add(card);
        }

        /// <summary>
        /// 카드 덱으로 이동
        /// </summary>
        public void HandCardToDeck(GameCard card)
        {
            if (card == null)
            {
                Debug.LogError($"핸드덱에 {card.Id} 카드 없음");
                return;
            }

            deckCards.Add(card);
            handCards.Remove(card);
        }

        public void Clear()
        {
            deckCards.Clear();
            handCards.Clear();
            graveCards.Clear();
            buildingCards.Clear();
        }

        /// <summary>
        /// 카드 랜덤 섞기
        /// </summary>
        /// <param name="cards">카드 리스트</param>
        private void ShuffleCards(List<GameCard> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                GameCard temp = cards[i];
                int randomIdex = random.Next(i, cards.Count);
                cards[i] = cards[randomIdex];
                cards[randomIdex] = temp;
            }
        }

        private void RemoveHandCard(GameCard card)
        {
            handCards.Remove(card);
        }

        public void AddHandCard(GameCard card)
        {
            handCards.Add(card);
        }

        public GameCard GetHandCard(string cardUid)
        {
            GameCard card = handCards.Find(card => card.Uid == cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {card.Id} 카드 없음");
                return null;
            }

            return card;
        }

        public void AddDeckCard(GameCard card)
        {
            deckCards.Add(card);
        }

        public void AddGraveCard(GameCard card)
        {
            graveCards.Add(card);
        }
    }
}