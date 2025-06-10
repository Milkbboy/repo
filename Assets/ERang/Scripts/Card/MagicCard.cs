using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    // 마법 카드
    public class MagicCard : BaseCard, IManaManageable
    {
        public override int Atk => State.Atk;
        public override int Mana => State.Mana;

        public bool IsSelectAttackType { get; private set; }
        public bool IsHandOnCard { get; private set; }
        public IReadOnlyList<int> TargetSlotNumbers => targetSlotNumbers;

        /// <summary>
        /// 핸드 카드 공격 타입이 Select 이면 선택 가능한 슬롯 번호
        /// </summary>
        private List<int> targetSlotNumbers = new();

        public MagicCard(CardData cardData) : base(cardData)
        {
            State = new CardState(0, 0, cardData.costMana, cardData.atk);
        }

        public void SetMana(int amount) => State.SetMana(amount);
        public void SetAttack(int amount) => State.SetAtk(amount);
        public void SetHandOnCard(bool isHandOnCard) => IsHandOnCard = isHandOnCard;
        public void SetSelectAttackType(bool isSelectAttackType) => IsSelectAttackType = isSelectAttackType;
        public void SetTargetSlotNumbers(List<int> slotNumbers) => targetSlotNumbers = slotNumbers;

        public void IncreaseMana(int amount) => State.IncreaseMana(amount);
        public void DecreaseMana(int amount) => State.DecreaseMana(amount);
    }
}