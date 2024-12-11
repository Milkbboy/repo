using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class ShowAllCard : MonoBehaviour
    {
        public GameObject cardPrefab;
        public GameObject showCards;
        public Canvas canvas;

        private List<GameObject> cardObjects = new List<GameObject>();

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

            for (int i = 0; i < Player.Instance.AllCards.Count; i++)
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

                    cardUI.SetCard(Player.Instance.AllCards[i]);
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
    }
}