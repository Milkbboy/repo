using System.Collections;
using UnityEngine;

namespace ERang
{
    [System.Serializable]
    public class BoardSlot : MonoBehaviour
    {
        public bool IsOccupied => isOccupied;
        public bool IsOverlapCard => isOverlapCard;
        public int Index => index;
        public int Slot => slot;
        public CardType CardType => cardType;
        public Card Card => card;

        [SerializeField] private int slot; // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 값
        [SerializeField] private int index; // 0, 1, 2, 3 값 3 은 마스터 카드
        [SerializeField] private CardType cardType; // 보드에 장착할 수 있는 cardType
        [SerializeField] private bool isOccupied = false; // 현재 사용 중인지 여부
        [SerializeField] private bool isOverlapCard = false; // 카드가 올라가 있는지 여부

        private Card card;
        private BoardSlotUI boardSlotUI;

        void Awake()
        {
            boardSlotUI = GetComponent<BoardSlotUI>();
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
            boardSlotUI.SetDesc(Slot, Index);
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetMasterSlot(Master master)
        {
            isOccupied = true;
            card = new Card(master.MasterId, CardType.Master, master.Hp, master.MaxHp, master.Atk, master.Def);

            boardSlotUI.SetMasterCard(master);
        }

        public void SetEnemyMasterSlot(Enemy enemy)
        {
            boardSlotUI.SetEnemyMasterCard(enemy);
        }

        public void SetMana(int mana)
        {
            boardSlotUI.SetMana(mana);
        }

        /// <summary>
        /// 카드 장착
        /// </summary>
        /// <param name="card"></param>
        public void EquipCard(Card card)
        {
            this.card = card;

            isOccupied = true;
            isOverlapCard = false;

            boardSlotUI.SetCard(card);

            Debug.Log($"{slot} 슬롯에 카드 장착 hp: {card.hp}, atk: {card.atk}, def: {card.def}, maxHp: {card.maxHp}");
        }

        /// <summary>
        /// 카드 hp 설정
        /// </summary>
        /// <param name="hp"></param>
        public void SetCardHp(int hp)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.SetHp(hp);
            boardSlotUI.SetHp(hp);
        }

        /// <summary>
        /// 카드 데미지 설정
        /// - 카드의 방어력이 0보다 크면 방어력을 먼저 깎고, 방어력이 0보다 작으면 체력을 깎는다.
        /// </summary>
        /// <param name="damage"></param>
        public IEnumerator SetDamage(int damage)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                yield break;
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

            boardSlotUI.SetHpDef(card.hp, card.def);

            // hp 0 으로 카드 제거
            if (card.hp <= 0)
                yield return StartCoroutine(BattleLogic.Instance.RemoveBoardCard(this));

            yield return null;
        }

        public void SetCardAtk(int atk)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.SetAtk(atk);
            boardSlotUI.SetAtk(card.atk);
        }

        public void SetCardDef(int def)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.SetDef(def);
            boardSlotUI.SetDef(card.def);
        }

        public void AddCardHp(int hp)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.AddHp(hp);
            boardSlotUI.SetHp(card.hp);
        }

        public void AddCardAtk(int atk)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.AddAtk(atk);
            boardSlotUI.SetAtk(card.atk);
        }

        public void AddCardDef(int def)
        {
            if (card == null)
            {
                Debug.LogWarning($"{slot}번 슬롯 카드 없음");
                return;
            }

            card.AddDef(def);
            boardSlotUI.SetDef(card.def);
        }

        public void SetFloatingGold(int beforeGlod, int afterGold)
        {
            boardSlotUI.SetFloatingGold(beforeGlod, afterGold);
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

            boardSlotUI.SetResetStat();
        }

        public void AniAttack()
        {
            boardSlotUI.AniAttack();
        }

        public void AniDamaged()
        {
            boardSlotUI.AniDamaged();
        }

        public void StartFlashing()
        {
            boardSlotUI.StartFlashing();
        }

        public void StopFlashing()
        {
            boardSlotUI.StopFlashing();
        }
    }
}