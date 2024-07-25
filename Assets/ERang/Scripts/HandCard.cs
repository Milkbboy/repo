using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class HandCard : MonoBehaviour
    {
        private CardData cardData;
        private CardUI cardUI;
        private Vector3 originalPosition;

        public int cardId;
        public int costMana;
        public int costGold;
        public int hp;
        public int atk;
        public int def;
        public int level;

        void Awake()
        {
            cardUI = GetComponent<CardUI>();
        }

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
            Debug.Log("Clicked card: " + cardData.card_id);
            originalPosition = transform.position;
        }

        void OnMouseDrag()
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = objPosition;
        }

        void OnMouseUp()
        {
            transform.position = originalPosition;
        }

        public void SetCard(CardData cardData)
        {
            this.cardData = cardData;

            // this.cardData 의 값을 변경하면 reference type 이기 때문에 cardData 의 값도 변경됨
            cardId = cardData.card_id;
            costMana = cardData.costMana;
            costGold = cardData.costGold;
            hp = cardData.hp;
            atk = cardData.atk;
            def = cardData.def;
            level = cardData.level;

            // 카드 ui 설정
            cardUI.SetCard(cardData);
        }
    }
}