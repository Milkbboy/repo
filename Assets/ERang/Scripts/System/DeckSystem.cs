using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class DeckSystem : MonoBehaviour
    {
        public static DeckSystem Instance { get; private set; }
        public GameObject cardPrefab;
        public GameObject showCards;
        public Canvas canvas;

        public int AllCardCount => allCards.Count;
        public int DeckCardCount => deckCards.Count;
        public int HandCardCount => handCards.Count;
        public int ExtinctionCardCount => extinctionCards.Count;
        public int GraveCardCount => graveCards.Count;

        public List<BaseCard> AllCards => allCards;
        public List<BaseCard> DeckCards => deckCards;
        public List<BaseCard> HandCards => handCards;
        public List<BaseCard> GraveCards => graveCards;
        public List<BaseCard> ExtinctionCards => extinctionCards;
        public List<BaseCard> BuildingCards => buildingCards;

        private bool isCreatedStarCard = false;
        private readonly int maxHandCardCount = 5;
        private readonly System.Random random = new();

        private readonly List<BaseCard> creatureCards = new(); // 마스터 크리쳐 카드
        private readonly List<BaseCard> buildingCards = new(); // 건물 카드

        [SerializeField]
        private List<BaseCard> allCards = new List<BaseCard>();
        [SerializeField]
        private List<BaseCard> deckCards = new List<BaseCard>();
        private readonly List<BaseCard> handCards = new();
        private readonly List<BaseCard> graveCards = new();
        private readonly List<BaseCard> extinctionCards = new();

        private List<GameObject> cardObjects = new List<GameObject>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("DeckSystem 생성됨");

                // 씬 로드 이벤트에 핸들러 등록
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else if (Instance != this)
            {
                Debug.Log("DeckSystem 파괴됨");
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            // 씬 로드 이벤트에서 핸들러 제거
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 현재 씬의 카메라를 Canvas의 Event Camera로 설정
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        public BaseCard FindHandCard(string cardUid)
        {
            return handCards.Find(card => card.Uid == cardUid);
        }

        /// <summary>
        /// 마스터 덱 카드 생성
        /// </summary>
        public void CreateMasterCards(Master master)
        {
            if (isCreatedStarCard == true)
            {
                Debug.LogError("이미 마스터 시작 카드 생성으로 allCards => deckCards 복사");

                Clear();

                // allCards 를 deckCards 로 복사
                deckCards.AddRange(allCards);

                Debug.Log($"deckCards.Count: {deckCards.Count}, {string.Join(", ", deckCards.Select(card => card.Id))}");
                return;
            }

            foreach (int cardId in master.StartCardIds)
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
                    BaseCard card = deckCards[0];

                    // 덱에서 카드를 뽑아 손에 추가
                    deckCards.RemoveAt(0);
                    handCards.Add(card);
                }
            }
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

            RemoveHandCard(card);

            if (card.IsExtinction)
                extinctionCards.Add(card);
            else
                graveCards.Add(card);
        }

        public void AddCard(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);

            if (cardData == null)
            {
                Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음 - AddCard");
                return;
            }

            BaseCard card = Utils.MakeCard(cardData);

            // 카드 타입별로 생성
            allCards.Add(card);
        }

        public void Clear()
        {
            deckCards.Clear();
            handCards.Clear();
            graveCards.Clear();
            buildingCards.Clear();
        }

        public void ShowAllCards()
        {
            // 스크린의 왼쪽 상단을 기준으로 시작 위치 계산
            Vector3 screenTopLeft = new(0, Screen.height, Camera.main.nearClipPlane);
            Vector3 worldTopLeft = Camera.main.ScreenToWorldPoint(screenTopLeft);
            worldTopLeft.z = 0; // Z축 위치를 0으로 설정하여 2D 평면에 배치

            // 카드 프리팹의 실제 크기 계산
            BoxCollider cardCollider = cardPrefab.GetComponent<BoxCollider>();
            if (cardCollider == null)
            {
                Debug.LogError("cardPrefab에 BoxCollider 컴포넌트가 없습니다.");
                return;
            }

            Vector3 cardSize = cardCollider.size;
            float cardWidth = cardSize.x * cardPrefab.transform.localScale.x + 0.1f; // 카드 간격 포함
            float cardHeight = cardSize.y * cardPrefab.transform.localScale.y + 0.1f; // 카드 간격 포함

            for (int i = 0; i < allCards.Count; i++)
            {
                // 카드 인스턴스화
                GameObject cardObject = Instantiate(cardPrefab, showCards.transform);

                // 카드 위치 설정
                int row = i / 10;
                int col = i % 10;
                
                Vector3 position = worldTopLeft + new Vector3(col * cardWidth, -row * cardHeight, 0f);
                cardObject.transform.localPosition = position;

                cardObjects.Add(cardObject);

                // 카드 데이터 설정 (예: 카드 UI 업데이트)
                CardUI cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    Renderer[] renderers = cardObject.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer renderer in renderers)
                        renderer.sortingOrder = 3000; // 높은 값으로 설정하여 맨 앞으로 이동

                    cardUI.SetCard(allCards[i]);
                }
            }
        }

        public void ClearAllCards()
        {
            foreach (GameObject cardObject in cardObjects)
            {
                Destroy(cardObject);
            }
            cardObjects.Clear();
        }

        public void ToggleShowCards()
        {
            if (cardObjects.Count > 0)
            {
                ClearAllCards();
            }
            else
            {
                ShowAllCards();
            }
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
        }
    }
}