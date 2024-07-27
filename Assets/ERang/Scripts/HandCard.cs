using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class HandCard : MonoBehaviour
    {
        private CardUI cardUI;
        private Vector3 originalPosition;

        public int cardId;
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
            originalPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnMouseDown()
        {
            Debug.Log("Clicked card: " + cardId);

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
            GameObject fieldSlotObj = Field.Instance.NeareastFieldSlot(transform.position);

            if (fieldSlotObj != null)
            {
                // Debug.Log($"slot: {fieldSlot.slot}");
                // 카드를 놓을 수 있는 fieldSlot 이 존재하면 카드를 놓는다.
                FieldSlot fieldSlot = fieldSlotObj.GetComponent<FieldSlot>();
                fieldSlot.GetComponent<CardUI>().SetCard(cardId);

                // HandDeck 에서 카드를 제거
                HandDeck.Instance.RemoveCard(this);
            }
            else
            {
                // Debug.Log("No slot");
                // 원래 위치로 돌아가게 함
                transform.position = originalPosition;
            }
        }

        public void SetCard(int cardId)
        {
            this.cardId = cardId;

            // 카드 ui 설정
            cardUI.SetCard(cardId);
        }

        public bool IsDrag()
        {
            return drag;
        }
    }
}