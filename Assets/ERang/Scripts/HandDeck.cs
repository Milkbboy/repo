using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /**
    * @brief 손에 들고 있는 카드들을 관리하는 클래스
    */
    public class HandDeck : MonoBehaviour
    {
        public static HandDeck Instance { get; private set; }

        public GameObject cardPrefab;
        private float cardSpacing = 1f;
        private List<HandCard> cards = new List<HandCard>();

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Debug.Log(cardPrefab.GetComponent<BoxCollider>().size);
        }

        // Update is called once per frame
        void Update()
        {

        }

        // 카드 최초 생성
        public void SpawnNewCards(List<int> cardIds)
        {
            foreach (int cardId in cardIds)
            {
                GameObject cardObj = Instantiate(cardPrefab, transform);

                cardObj.GetComponent<CardUI>().SetCard(cardId);

                HandCard handCard = cardObj.GetComponent<HandCard>();
                handCard.SetCard(cardId);

                cards.Add(handCard);

                Debug.Log("Spawned card: " + cardId);
            }

            // 카드의 너비를 얻기 위해 cardPrefab의 BoxCollider 컴포넌트에서 size.x 값을 사용
            float cardWidth = cardPrefab.GetComponent<BoxCollider>().size.x;
            // 겹치는 정도를 조절하기 위해 cardWidth의 일부를 사용
            float overlap = cardWidth * 0.05f; // 20% 겹치도록 설정
            cardSpacing = cardWidth + overlap; // 카드 간격 재조정

            // 카드 정렬 로직 시작
            float totalWidth = (cards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2;

            for (int i = 0; i < cards.Count; i++)
            {
                // 카드의 X 위치 설정, Y와 Z 위치는 고정
                float xPosition = startX + i * cardSpacing;
                // 카드의 Z 위치를 인덱스에 따라 조정하여 위에 있는 카드가 앞으로 오도록 설정
                // float zPosition = i * zOffset;

                cards[i].transform.localPosition = new Vector3(xPosition, 0, 0);
            }
        }

        public void RemoveCard(HandCard handCard)
        {
            cards.Remove(handCard);
            Destroy(handCard.gameObject);
        }
    }
}