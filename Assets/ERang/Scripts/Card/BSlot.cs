using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BSlot : MonoBehaviour
    {
        [Header("Display")]
        public Texture2D cardTexture;
        public MeshRenderer meshRenderer;

        /// <summary>
        /// 슬롯에 장착할 수 있는 카드 타입
        /// </summary>
        public CardType SlotCardType { get => slotCardType; set => slotCardType = value; }
        public BaseCard Card => card;
        public int SlotNum => slotNum;
        public int Index => index;
        public bool IsOverlapCard => isOverlapCard;

        public GameObject cardObject;

        // LogText 속성 추가
        public string LogText => Utils.BoardSlotLog(this);

        private BaseCard card = null;
        private CardUI cardUI;
        private Ani_Attack aniAttack;
        private Ani_Damaged aniDamaged;

        private CardType slotCardType;
        [SerializeField] private int slotNum; // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 값
        [SerializeField] private int index; // 0, 1, 2, 3 값 3 은 마스터 카드
        [SerializeField] private bool isOverlapCard = false; // 카드가 올라가 있는지 여부

        void Awake()
        {
            cardUI = cardObject.GetComponent<CardUI>();

            aniAttack = GetComponent<Ani_Attack>();
            aniDamaged = GetComponent<Ani_Damaged>();
        }

        void Start()
        {
            cardObject.SetActive(false);
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

        public void CreateSlot(int slot, int index, CardType cardType)
        {
            slotNum = slot;
            this.index = index;
            slotCardType = cardType;

            switch (cardType)
            {
                case CardType.None: cardTexture = Resources.Load<Texture2D>("Textures/Blank_Red"); break;
                case CardType.Creature: cardTexture = Resources.Load<Texture2D>("Textures/Blank_Green"); break;
                case CardType.Monster: cardTexture = Resources.Load<Texture2D>("Textures/Blank_Purple"); break;
            }

            if (cardTexture != null)
                meshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
        }

        public bool EquipCard(BaseCard card)
        {
            if (SlotCardType != card.CardType)
            {
                Debug.LogError($"카드 타입이 일치하지 않습니다. 슬롯 타입: {SlotCardType}, 카드 타입: {card.CardType}");
                return false;
            }

            Debug.Log($"{card.CardType} 카드({card.Id}) 장착. {card.GetType()} ");

            this.card = card;

            cardObject.SetActive(true);
            cardUI.SetCard(card);

            return true;
        }

        public IEnumerator TakeDamage(int amount)
        {
            if (card is not CreatureCard)
            {
                Debug.LogWarning($"카드 타입이 크리쳐가 아닌 {card.CardType}");
                yield break;
            }

            CreatureCard creatureCard = card as CreatureCard;

            creatureCard.TakeDamage(amount);
            
            cardUI.SetHp(creatureCard.Hp);
            cardUI.SetDef(creatureCard.Def);

            if (creatureCard.Hp <= 0)
            {
                RemoveCard();

                yield return null;

                StartCoroutine(BattleLogic.Instance.RemoveBoardCard(slotNum));
            }
        }

        public void RemoveCard()
        {
            card = null;
            cardObject.SetActive(false);
        }

        public void AdjustMana(int amount)
        {
            if (card is not MasterCard)
            {
                Debug.LogWarning($"{SlotNum} 슬롯 카드 타입이 마스터가 아닌 {(card != null ? card.CardType : "카드 없음")}");
                return;
            }

            MasterCard masterCard = card as MasterCard;

            if (amount > 0)
                masterCard.IncreaseMana(amount);
            else
                masterCard.DecreaseMana(-amount);

            cardUI.SetMana(masterCard.Mana);
        }

        public void ResetMana()
        {
            if (card is not MasterCard)
            {
                Debug.LogWarning($"{SlotNum} 슬롯 카드 타입이 마스터가 아닌 {(card != null ? card.CardType : "카드 없음")}");
                return;
            }

            MasterCard masterCard = card as MasterCard;

            masterCard.ResetMana();
            cardUI.SetMana(masterCard.Mana);
        }

        public void ApplyDamageAnimation()
        {
            aniAttack.PlaySequence();
        }

        public void TakeDamageAnimation()
        {
            aniDamaged.PlaySequence();
        }
    }
}
