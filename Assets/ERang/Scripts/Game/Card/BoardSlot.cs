using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ERang
{
    public class BoardSlot : MonoBehaviour
    {
        /// <summary>
        /// 슬롯에 장착할 수 있는 카드 타입
        /// </summary>
        public CardType SlotCardType { get => slotCardType; set => slotCardType = value; }
        public BaseCard Card => card;
        public int SlotNum => slotNum;
        public int Index => index;
        public bool IsOverlapCard => isOverlapCard;
        public GameObject cardPrefab;
        public AbilityIcons abilityIcons;

        private GameObject cardObject;
        private BaseCard card = null;
        private SlotUI slotUI;
        private CardUI cardUI;
        private Ani_Attack aniAttack;
        private Ani_Damaged aniDamaged;

        private CardType slotCardType;
        [SerializeField] private int slotNum; // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 값
        [SerializeField] private int index; // 0, 1, 2, 3 값 3 은 마스터 카드
        [SerializeField] private bool isOverlapCard = false; // 카드가 올라가 있는지 여부

        void Awake()
        {
            slotUI = GetComponent<SlotUI>();
            aniAttack = GetComponent<Ani_Attack>();
            aniDamaged = GetComponent<Ani_Damaged>();
        }

        void Start()
        {
            cardObject = Instantiate(cardPrefab, transform);
            cardObject.transform.localScale = Vector3.one;
            abilityIcons.SetSlot(this, cardObject);

            cardUI = cardObject.GetComponent<CardUI>();
            // Dragable 스크립트 동작 방지를 위해
            BoxCollider boxCollider = cardObject.GetComponent<BoxCollider>();
            if (boxCollider)
                boxCollider.enabled = false;

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

        void OnDrawGizmos()
        {
            Transform cardFrameBackTransform = cardObject.transform.Find("Card_Frame_Back");

            if (cardFrameBackTransform == null)
                Debug.LogError("Card_Frame_Back 오브젝트를 찾을 수 없습니다.");

            if (cardFrameBackTransform != null)
            {
                MeshRenderer meshRenderer = cardFrameBackTransform.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                {
                    Bounds bounds = meshRenderer.bounds;

                    Vector3[] corners = new Vector3[4];

                    // 카드 이미지의 네 모서리 좌표 계산
                    corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z); // 왼쪽 아래 모서리
                    corners[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z); // 오른쪽 아래 모서리
                    corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z); // 왼쪽 위 모서리
                    corners[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z); // 오른쪽 위 모서리

                    Gizmos.color = Color.red;

                    // 각 모서리에 작은 구를 그려서 표시하고 인덱스 라벨 추가
                    for (int i = 0; i < corners.Length; i++)
                    {
                        Gizmos.DrawSphere(corners[i], 0.05f);
#if UNITY_EDITOR
                        Handles.Label(corners[i], i.ToString());
#endif
                    }
                }
            }
        }

        public void CreateSlot(int slot, int index, CardType cardType)
        {
            slotNum = slot;
            this.index = index;
            slotCardType = cardType;

            slotUI.SetSlot(cardType);
        }

        public bool EquipCard(BaseCard card)
        {
            if (SlotCardType != card.CardType)
            {
                Debug.LogError($"{card.ToCardLogInfo()} 타입이 일치하지 않습니다. 슬롯 타입: {SlotCardType}, 카드 타입: {card.CardType}");
                return false;
            }

            Debug.Log($"{card.ToCardLogInfo()} 장착. {card.GetType()} ");

            this.card = card;

            cardObject.SetActive(true);
            cardObject.GetComponent<HandCard>().SetCard(card);
            cardUI.SetCard(card);

            return true;
        }

        public IEnumerator TakeDamage(int amount)
        {
            card.TakeDamage(amount);

            cardUI.SetHp(card.Hp);
            cardUI.SetDef(card.Def);

            TakeDamageAnimation();

            if (card.Hp <= 0)
            {
                RemoveCard();

                yield return StartCoroutine(BattleLogic.Instance.RemoveBoardCard(slotNum));
            }
        }

        public void DrawAbilityIcons()
        {
            if (card == null)
            {
                return;
            }

            List<int> abilityIds = card.AbilitySystem.CardAbilities.Select(ability => ability.abilityId).ToList();

            if (abilityIds.Count == 0)
            {
                abilityIcons.RemoveAllIcons();
                return;
            }

            abilityIcons.SetIcons(card.AbilitySystem.CardAbilities);
        }

        public void RestoreHealth(int amount)
        {
            card.RestoreHealth(amount);
            cardUI.SetHp(card.Hp);
        }

        public void SetDefense(int amount)
        {
            card.SetDefense(amount);
            cardUI.SetDef(card.Def);
        }

        public void IncreaseDefense(int amount)
        {
            card.IncreaseDefense(amount);
            cardUI.SetDef(card.Def);
        }

        public void DecreaseDefense(int amount)
        {
            card.DecreaseDefense(amount);
            cardUI.SetDef(card.Def);
        }

        public void IncreaseAttack(int amount)
        {
            if (card is not CreatureCard creatureCard)
            {
                Debug.LogWarning($"{this.ToSlotLogInfo()}: 슬롯 카드가 CreatureCard 가 아닙니다.");
                return;
            }

            creatureCard.IncreaseAttack(amount);
            cardUI.SetAtk(creatureCard.Atk);
        }

        public void DecreaseAttack(int amount)
        {
            if (card is not CreatureCard creatureCard)
            {
                Debug.LogWarning($"{this.ToSlotLogInfo()}: 슬롯 카드가 CreatureCard 가 아닙니다.");
                return;
            }

            creatureCard.DecreaseAttack(amount);
            cardUI.SetAtk(creatureCard.Atk);
        }

        public void RemoveCard()
        {
            abilityIcons.RemoveAllIcons();
            cardObject.SetActive(false);
            card = null;
        }

        public void SetHp(int amount)
        {
            if (card is not MasterCard masterCard)
            {
                Debug.LogWarning($"{SlotNum} 슬롯 카드 타입이 마스터가 아닌 {(card != null ? card.CardType : "카드 없음")}");
                return;
            }

            masterCard.SetHp(amount);

            cardUI.SetHp(masterCard.Hp);
        }

        public void SetMana(int amount)
        {
            if (card is not MasterCard masterCard)
            {
                Debug.LogWarning($"{SlotNum} 슬롯 카드 타입이 마스터가 아닌 {(card != null ? card.CardType : "카드 없음")}");
                return;
            }

            masterCard.SetMana(amount);

            cardUI.SetMana(masterCard.Mana);
        }

        public void IncreaseMana(int amount)
        {
            if (card is not MasterCard masterCard)
            {
                Debug.LogWarning($"{SlotNum} 슬롯 카드 타입이 마스터가 아닌 {(card != null ? card.CardType : "카드 없음")}");
                return;
            }

            masterCard.IncreaseMana(amount);

            cardUI.SetMana(masterCard.Mana);
        }

        public void DecreaseMana(int amount)
        {
            if (card is not MasterCard masterCard)
            {
                Debug.LogWarning($"{SlotNum} 슬롯 카드 타입이 마스터가 아닌 {(card != null ? card.CardType : "카드 없음")}");
                return;
            }

            masterCard.DecreaseMana(amount);

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
