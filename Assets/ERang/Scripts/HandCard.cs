using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ERang
{
    public class HandCard : MonoBehaviour
    {
        private CardUI cardUI;
        public Vector3 originalPosition;
        public string cardUid;
        public int cardId;
        public CardType cardType;
        private bool drag = false;

        // Start is called before the first frame update
        void Awake()
        {
            cardUI = GetComponent<CardUI>();
        }

        // 카드 hp, atk, def, costMana, costGold 등은 cardData 의 기본 값에서 해당 카드의 ability 로 최종 값을 결정하자.

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnMouseDown()
        {
            // Debug.Log("OnMouseDown card: " + cardId);
            drag = true;
        }

        void OnMouseDrag()
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = new Vector3(objPosition.x, objPosition.y, originalPosition.z - 0.05f);
            drag = false;
        }

        void OnMouseUp()
        {
            // CardType 별 동작
            switch (cardType)
            {
                case CardType.Creature:
                case CardType.Building:
                    BoardSlot boardSlot = Board.Instance.NeareastBoardSlot(transform.position);

                    if (boardSlot != null && boardSlot.cardType == this.cardType)
                    {
                        // 보드 슬롯에 카드를 장착
                        Actions.OnBoardSlotEquipCard?.Invoke(boardSlot, cardUid);
                    }
                    break;
                case CardType.Curse:
                case CardType.Charm:
                case CardType.Magic:
                case CardType.Individuality:
                    Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                    if (IsUpperCenter(screenPosition))
                    {
                        // 카드 사용
                        Actions.OnHandCardUsed?.Invoke(cardUid);
                    }
                    break;
            }

            // 원래 위치로 돌아가게 함
            transform.position = originalPosition;
        }

        public void SetCard(Card card)
        {
            cardUid = card.uid;
            cardId = card.id;
            cardType = card.cardType;

            // 카드 ui 설정
            cardUI.SetCard(card);
        }

        public bool IsDrag()
        {
            return drag;
        }
        private bool IsUpperCenter(Vector3 screenPosition)
        {
            float screenHeight = Screen.height;

            // Define the upper center area (e.g., top 50% of the screen and middle 50% width)
            float upperHeight = screenHeight * 0.5f;

            return screenPosition.y > screenHeight - upperHeight;
        }
    }
}