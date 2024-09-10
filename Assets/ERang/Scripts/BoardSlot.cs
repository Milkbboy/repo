using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    [System.Serializable]
    public class BoardSlot : MonoBehaviour
    {
        [SerializeField] private int slot; // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 값
        [SerializeField] private int index; // 0, 1, 2, 3 값 3 은 마스터 카드
        [SerializeField] private CardType cardType; // 보드에 장착할 수 있는 cardType
        [SerializeField] private bool isOccupied = false; // 현재 사용 중인지 여부
        [SerializeField] private bool isOverlapCard = false; // 카드가 올라가 있는지 여부

        private Card card;
        private CardUI cardUI;
        private BoardSlotUI boardSlotUI;
        private Ani_Attack aniAttack;
        private Ani_Damaged aniDamaged;

        public bool IsOccupied { get { return isOccupied; } }
        public bool IsOverlapCard { get { return isOverlapCard; } }
        public int Index { get { return index; } }
        public int Slot { get { return slot; } }
        public CardType CardType { get { return cardType; } }
        public Card Card { get { return card; } }

        void Awake()
        {
            cardUI = GetComponent<CardUI>();
            cardUI.cardObject.SetActive(false);

            boardSlotUI = GetComponent<BoardSlotUI>();

            aniAttack = GetComponent<Ani_Attack>();
            aniDamaged = GetComponent<Ani_Damaged>();
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

            boardSlotUI.SetSlotType(cardType);

            aniAttack.isAttackingFromLeft = cardType == CardType.Master || cardType == CardType.Creature;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetMasterSlot(Master master)
        {
            isOccupied = true;

            cardUI.cardObject.SetActive(true);
            cardUI.SetMasterCard(master);

            card = new Card(master.MasterId, CardType.Master, master.Hp, master.MaxHp, master.Atk, master.Def);
        }

        public void SetEnemyMasterSlot(Enemy enemy)
        {
            cardUI.cardObject.SetActive(true);
            cardUI.SetEnemyMasterCard(enemy);
        }

        public void SetMasterMana(int mana)
        {
            cardUI.SetMasterMana(mana);
        }

        /**
         * @brief 카드 장착
         * @param card 카드 정보
        */
        public void EquipCard(Card card)
        {
            this.card = card;

            Debug.Log($"{slot} 슬롯에 카드 장착 hp: {card.hp}, atk: {card.atk}, def: {card.def}, maxHp: {card.maxHp}");

            isOccupied = true;
            isOverlapCard = false;

            cardUI.cardObject.SetActive(true);
            cardUI.SetCard(card);
        }

        public void SetCardHp(int hp)
        {
            card.SetHp(hp);
            cardUI.SetHp(card.hp);
        }

        /// <summary>
        /// 카드 데미지 설정
        /// - 카드의 방어력이 0보다 크면 방어력을 먼저 깎고, 방어력이 0보다 작으면 체력을 깎는다.
        /// </summary>
        /// <param name="damage"></param>
        public void SetDamage(int damage)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            if (card.def > 0)
            {
                card.def -= damage;

                if (card.def < 0)
                {
                    card.hp += card.def;
                    card.def = 0;
                }
            }
            else
            {
                card.hp -= damage;
            }

            cardUI.SetHp(card.hp);
            cardUI.SetDef(card.def);

            if (card.hp <= 0)
                BattleLogic.Instance.RemoveBoardCard(this);
        }

        public void SetCardAtk(int atk)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.SetAtk(atk);
            cardUI.SetAtk(card.atk);
        }

        public void SetCardDef(int def)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.SetDef(def);
            cardUI.SetDef(card.def);
        }

        public void AddCardHp(int hp)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.AddHp(hp);
            cardUI.SetHp(card.hp);
        }

        public void AddCardAtk(int atk)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.AddAtk(atk);
            cardUI.SetAtk(card.atk);
        }

        public void AddCardDef(int def)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.AddDef(def);
            cardUI.SetDef(card.def);
        }

        public void SetGoldUI(int beforeGlod, int afterGold)
        {
            cardUI.SetGold(beforeGlod, afterGold);
        }

        public void RemoveCard()
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            isOccupied = false;
            isOverlapCard = false;

            card = null;

            cardUI.ResetStat();
            cardUI.cardObject.SetActive(false);
        }

        public void StartFlashing(Color? color = null)
        {
            boardSlotUI.StartFlashing(color);
        }

        public void StopFlashing()
        {
            boardSlotUI.StopFlashing();
        }

        public void AniAttack()
        {
            aniAttack.PlaySequence();
        }

        public void AniDamaged()
        {
            aniDamaged.PlaySequence();
        }
    }
}