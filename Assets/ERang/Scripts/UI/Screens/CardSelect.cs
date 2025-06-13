using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ERang
{
    public class CardSelect : MonoBehaviour
    {
        public int maxSelectCount = 0;
        // 핸드 카드 생성을 위한 프리팹
        public GameObject cardPrefab;
        public GameObject backgroundObject;
        public TextMeshProUGUI selectCardCount;
        public Transform discardTransform;
        // 선택한 카드
        public List<SelectCard> selectCards = new();
        public UnityAction<List<SelectCard>> OnCardSelectComplete;

        private float cardWidth = 0f;
        private float cardSpacing = 1f;

        // 선택 카드 리스트
        private readonly List<SelectCard> cards = new();
        private readonly List<GameObject> cardObjects = new();

        // Start is called before the first frame update
        void Awake()
        {
            // 카드의 너비를 얻기 위해 cardPrefab의 BoxCollider 컴포넌트에서 size.x 값을 사용
            BoxCollider boxCollider = cardPrefab.GetComponent<BoxCollider>();
            cardWidth = boxCollider.size.x * cardPrefab.transform.localScale.x;
        }

        public void SetMaxSelectCardCount(int maxSelectCount)
        {
            this.maxSelectCount = maxSelectCount;
            selectCardCount.text = $"{selectCards.Count}/{this.maxSelectCount}";
        }

        public void DrawCards(IReadOnlyList<BaseCard> baseCards)
        {
            foreach (BaseCard card in baseCards)
            {
                GameObject cardObject = Instantiate(cardPrefab, transform);
                cardObject.name = $"SelectCard_{card.Id}";

                SelectCard SelectCard = cardObject.GetComponent<SelectCard>();
                SelectCard.SetCard(card);
                SelectCard.OnClick += OnSelectCard;

                cards.Add(SelectCard);
                cardObjects.Add(cardObject);

                // 카드 데이터 설정 (예: 카드 UI 업데이트)
                CardUI cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    Renderer[] renderers = cardObject.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer renderer in renderers)
                        renderer.sortingOrder = 3000; // 높은 값으로 설정하여 맨 앞으로 이동
                }
            }

            // 겹치는 정도를 조절하기 위해 cardWidth의 일부를 사용
            float overlap = cardWidth * 0.05f;
            cardSpacing = cardWidth + overlap;

            // 카드 정렬 로직 시작
            float totalWidth = (cards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2;

            for (int i = 0; i < cards.Count; ++i)
            {
                // 카드의 X 위치 설정, Y와 Z 위치는 고정
                float xPosition = startX + i * cardSpacing;
                // 카드의 Z 위치를 인덱스에 따라 조정하여 위에 있는 카드가 앞으로 오도록 설정
                // float zPosition = i * zOffset;

                cards[i].SetDrawPostion(new Vector3(xPosition, 0, 0));
            }
        }

        public void OnConfirmClick()
        {
            // 선택한 카드들의 목록을 AbilitySwallow 의 UnityAction으로 전달
            OnCardSelectComplete?.Invoke(selectCards);
        }

        public IEnumerator CloseCardSelect()
        {
            // 선택한 카드를 HandCard 에서 제거하고 다음 턴 핸드 카드에 바로 나오게 해야 한다.
            foreach (SelectCard selectCard in selectCards)
            {
                yield return StartCoroutine(selectCard.DiscardAnimation(discardTransform));
            }

            foreach (GameObject cardObject in cardObjects)
            {
                Destroy(cardObject);
            }

            cards.Clear();
            selectCards.Clear();

            gameObject.SetActive(false);
        }

        public IEnumerator OnConfirmClickCoroutine()
        {
            // 선택한 카드를 HandCard 에서 제거하고 다음 턴 핸드 카드에 바로 나오게 해야 한다.
            foreach (SelectCard selectCard in selectCards)
            {
                DeckManager.Instance.HandCardToBoard(selectCard.Card);
                yield return StartCoroutine(selectCard.DiscardAnimation(discardTransform));
            }

            foreach (GameObject cardObject in cardObjects)
            {
                Destroy(cardObject);
            }

            cards.Clear();
            selectCards.Clear();

            gameObject.SetActive(false);
        }

        private void OnSelectCard(SelectCard selectedCard)
        {
            Debug.Log($"Selected Card: {selectedCard.Card.Uid}");

            selectedCard.isScaleFixed = !selectedCard.isScaleFixed;

            // selectCards에 이미 있는지 확인
            if (selectCards.Contains(selectedCard))
            {
                selectCards.Remove(selectedCard);
            }
            else
            {
                // 최대 선택 카드 수를 초과하면 첫 번째 카드를 제거
                if (selectCards.Count >= maxSelectCount)
                {
                    Debug.Log("Max select count exceeded");
                    selectedCard.isScaleFixed = false;
                    return;
                }

                selectCards.Add(selectedCard);
            }

            selectCardCount.text = $"{selectCards.Count}/{maxSelectCount}";
        }
    }
}