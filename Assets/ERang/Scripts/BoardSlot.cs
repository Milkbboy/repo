using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class BoardSlot : MonoBehaviour
    {
        [SerializeField] private int slot; // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 값
        [SerializeField] private int index; // 0, 1, 2, 3 값 3 은 마스터 카드
        [SerializeField] private CardType cardType; // 보드에 장착할 수 있는 cardType
        [SerializeField] private bool isOccupied = false; // 현재 사용 중인지 여부
        [SerializeField] private bool isOverlapCard = false; // 카드가 올라가 있는지 여부

        private Card card;
        private CardUI cardUI;

        public bool IsOccupied { get { return isOccupied; } }
        public bool IsOverlapCard { get { return isOverlapCard; } }
        public string CardUid { get { return card.uid; } }
        public int Index { get { return index; } }
        public int Slot { get { return slot; } }
        public CardType CardType { get { return cardType; } }
        public Card Card { get { return card; } }

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

        public void SetIndex(int index)
        {
            this.index = index;
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
            this.card = card;

            isOccupied = true;
            isOverlapCard = false;

            cardUI.statObj.SetActive(true);
            cardUI.SetCard(card);
        }

        public void SetCardHp(int hp)
        {
            cardUI.SetHp(hp);
        }

        public void RemoveCard()
        {
            Debug.Log($"Removing card from slot {slot}");

            isOccupied = false;
            isOverlapCard = false;

            card = null;

            cardUI.ResetStat();
            cardUI.statObj.SetActive(false);
        }
    }
}