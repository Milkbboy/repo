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
        private float cardWidth = 0f;
        private float cardSpacing = 1f;

        // 핸드 카드 리스트
        private List<HandCard> handCards = new List<HandCard>();

        void Awake()
        {
            Instance = this;

            // 카드의 너비를 얻기 위해 cardPrefab의 BoxCollider 컴포넌트에서 size.x 값을 사용
            cardWidth = cardPrefab.GetComponent<BoxCollider>().size.x;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }

        // 카드 생성
        public void SpawnNewCard(Card card)
        {
            GameObject cardObj = Instantiate(cardPrefab, transform);

            cardObj.GetComponent<HandCard>().SetCard(card);
            handCards.Add(cardObj.GetComponent<HandCard>());
        }

        // 카드 위치 설정
        public void DrawCards()
        {
            // 겹치는 정도를 조절하기 위해 cardWidth의 일부를 사용
            float overlap = cardWidth * 0.05f; // 20% 겹치도록 설정
            cardSpacing = cardWidth + overlap; // 카드 간격 재조정

            // 카드 정렬 로직 시작
            float totalWidth = (handCards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2;

            for (int i = 0; i < handCards.Count; i++)
            {
                // 카드의 X 위치 설정, Y와 Z 위치는 고정
                float xPosition = startX + i * cardSpacing;
                // 카드의 Z 위치를 인덱스에 따라 조정하여 위에 있는 카드가 앞으로 오도록 설정
                // float zPosition = i * zOffset;

                handCards[i].SetDrawPostion(new Vector3(xPosition, 0, 0));
            }
        }

        // 핸드덱 카드 제거
        public void RemoveCard(string cardUid)
        {
            HandCard handCard = handCards.Find(x => x.GetUID() == cardUid);

            if (handCard == null)
            {
                return;
            }

            handCards.Remove(handCard);
            Destroy(handCard.gameObject);

            DrawCards();
        }
    }
}