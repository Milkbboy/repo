using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    // 마법 카드
    public class MagicCard : BaseCard, IManaManageable
    {
        public override int Atk { get; set; }
        public override int Mana => mana;

        public bool IsSelectAttackType => isSelectAttackType;
        public bool IsHandOnCard => isHandOnCard;
        public List<int> TargetSlotNumbers => targetSlotNumbers;

        /// <summary>
        /// 핸드 카드 위에 마우스가 올라가 있는지 여부
        /// </summary>
        private bool isHandOnCard = false;
        /// <summary>
        /// 핸드 카드 공격 타입이 Select 인지 여부
        /// </summary>
        private bool isSelectAttackType = false;
        /// <summary>
        /// 핸드 카드 공격 타입이 Select 이면 선택 가능한 슬롯 번호
        /// </summary>
        private List<int> targetSlotNumbers = new();

        private int mana;

        public MagicCard(CardData cardData) : base(cardData)
        {
            Atk = cardData.atk;
            mana = cardData.costMana;
        }

        public void SetMana(int amount)
        {
            mana = amount;
        }

        public void SetAttack(int amount)
        {
            Atk = amount;
        }

        public void SetHandOnCard(bool isHandOnCard)
        {
            this.isHandOnCard = isHandOnCard;
        }

        public void SetSelectAttackType(bool isSelectAttackType)
        {
            this.isSelectAttackType = isSelectAttackType;
        }

        public void SetTargetSlotNumbers(List<int> slotNumbers)
        {
            targetSlotNumbers = slotNumbers;
        }

        public void IncreaseMana(int amount)
        {
            mana += amount;
        }

        public void DecreaseMana(int amount)
        {
            mana -= amount;

            if (mana < 0)
                mana = 0;
        }
    }
}