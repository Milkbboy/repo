using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BoardSlot : MonoBehaviour
    {
        [SerializeField] private int cardId;
        [SerializeField] private CardType cardType; // 보드에 장착할 수 있는 cardType
        [SerializeField] private int slot;
        private string cardUid;
        private bool isOccupied = false; // 현재 사용 중인지 여부
        private bool isOverlapCard = false; // 카드가 올라가 있는지 여부

        private CardUI cardUI;

        public bool IsOccupied { get { return isOccupied; } }
        public bool IsOverlapCard { get { return isOverlapCard; } }

        void Awake()
        {
            cardUI = GetComponent<CardUI>();
            cardUI.statObj.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("HandCard"))
            {
                isOverlapCard = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("HandCard"))
            {
                isOverlapCard = false;
            }
        }

        public void CreateSlot(int slot, CardType cardType)
        {
            this.slot = slot;
            this.cardType = cardType;
        }

        public void CreateMasterSlot(int slot, Master master)
        {
            this.slot = slot;
            this.cardType = CardType.Master;

            cardUI.statObj.SetActive(true);
            cardUI.SetMasterCard(master);
        }

        public void SetMasterStat(Master master)
        {
            cardUI.SetMasterStat(master);
        }

        /**
         * @brief 카드 장착
         * @param card 카드 정보
        */
        public void EquipCard(Card card)
        {
            this.cardId = card.id;
            this.cardType = card.type;
            this.isOccupied = true;
            this.isOverlapCard = false;

            cardUI.statObj.SetActive(true);
            cardUI.SetCard(card);
        }

        /**
         * @brief 슬롯의 카드 타입 얻기
        */
        public CardType GetCardType()
        {
            return cardType;
        }

        public int GetSlot()
        {
            return slot;
        }
    }
}