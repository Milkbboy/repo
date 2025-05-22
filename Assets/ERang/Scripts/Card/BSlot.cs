using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ERang
{
    public class BSlot : MonoBehaviour
    {
        /// <summary>
        /// 슬롯에 장착할 수 있는 카드 타입
        /// </summary>
        public CardType SlotCardType { get => slotCardType; set => slotCardType = value; }
        public GameCard Card => card;
        public int SlotNum => slotNum;
        public int Index => index;
        public bool IsOverlapCard => isOverlapCard;

        public GameObject cardObject;
        public AbilityIcons abilityIcons;

        // LogText 속성 추가
        public string LogText => Utils.BoardSlotLog(this);

        private GameCard card = null;
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
            cardUI = cardObject.GetComponent<CardUI>();

            aniAttack = GetComponent<Ani_Attack>();
            aniDamaged = GetComponent<Ani_Damaged>();

            abilityIcons.SetSlot(this);
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
            //             BoxCollider collider = GetComponent<BoxCollider>();
            //             if (collider != null)
            //             {
            //                 Bounds bounds = collider.bounds;

            //                 Vector3[] corners = new Vector3[8];

            //                 // 콜라이더의 8개 모서리 좌표 계산
            //                 corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            //                 corners[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            //                 corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            //                 corners[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            //                 corners[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            //                 corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            //                 corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            //                 corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);

            //                 Gizmos.color = Color.red;

            //                 // 각 모서리에 구를 그려서 표시
            //                 // 각 모서리에 구를 그려서 표시하고 인덱스 라벨 추가
            //                 for (int i = 0; i < corners.Length; i++)
            //                 {
            //                     Gizmos.DrawSphere(corners[i], 0.01f);
            // #if UNITY_EDITOR
            //                     Handles.Label(corners[i], i.ToString());
            // #endif
            //                 }
            //                 // foreach (Vector3 corner in corners)
            //                 // {
            //                 //     Gizmos.DrawSphere(corner, 0.01f);
            //                 // }
            //             }
        }

        public void CreateSlot(int slot, int index, CardType cardType)
        {
            slotNum = slot;
            this.index = index;
            slotCardType = cardType;

            slotUI.SetSlot(cardType);
        }

        public bool EquipCard(GameCard card)
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
            int beforeHp = card.State.Hp;
            int beforeDef = card.State.Def;

            card.TakeDamage(amount);

            cardUI.SetHp(card.State.Hp);
            cardUI.SetDef(card.State.Def);

            TakeDamageAnimation();

            if (card.State.Hp <= 0)
            {
                RemoveCard();

                yield return StartCoroutine(BattleLogic.Instance.RemoveBoardCard(slotNum));
            }

            Debug.Log($"{card?.LogText ?? "카드 없음"} Damage: {amount}. Hp: {beforeHp} -> {card?.State.Hp ?? 0}, Def: {beforeDef} -> {card?.State.Def ?? 0} - TakeDamage");
        }

        public void DrawAbilityIcons()
        {
            if (card == null)
            {
                // Debug.LogError($"{LogText} 카드가 없습니다.");
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
            cardUI.SetHp(card.State.Hp);
        }

        public void SetDefense(int amount)
        {
            card.SetDefense(amount);
            cardUI.SetDef(card.State.Def);
        }

        public void IncreaseDefense(int amount)
        {
            card.IncreaseDefense(amount);
            cardUI.SetDef(card.State.Def);
        }

        public void DecreaseDefense(int amount)
        {
            card.DecreaseDefense(amount);
            cardUI.SetDef(card.State.Def);
        }

        public void IncreaseAttack(int amount)
        {
            card.IncreaseAttack(amount);
            cardUI.SetAtk(card.State.Atk);
        }

        public void DecreaseAttack(int amount)
        {
            card.DecreaseAttack(amount);
            cardUI.SetAtk(card.State.Atk);
        }

        public void RemoveCard()
        {
            abilityIcons.RemoveAllIcons();
            cardObject.SetActive(false);
            card = null;
        }

        public void SetHp(int amount)
        {
            card.SetHp(amount);
            cardUI.SetHp(card.State.Hp);
        }

        public void SetMana(int amount)
        {
            card.SetMana(amount);
            cardUI.SetMana(card.State.Mana);
        }

        public void IncreaseMana(int amount)
        {
            card.IncreaseMana(amount);
            cardUI.SetMana(card.State.Mana);
        }

        public void DecreaseMana(int amount)
        {
            card.DecreaseMana(amount);
            cardUI.SetMana(card.State.Mana);
        }

        public void ResetMana()
        {
            // 마나 리셋
            card.SetMana(0);
            cardUI.SetMana(card.State.Mana);
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
