using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    // 마법 카드
    public class MagicCard : BaseCard, IManaManageable
    {
        public bool IsSelectAttackType { get; private set; }
        public bool IsHandOnCard { get; private set; }
        public IReadOnlyList<int> TargetSlotNumbers => targetSlotNumbers;

        /// <summary>
        /// 핸드 카드 공격 타입이 Select 이면 선택 가능한 슬롯 번호
        /// </summary>
        private List<int> targetSlotNumbers = new();

        public MagicCard(CardData cardData) : base(cardData)
        {
            Stat = new CardStat(0, 0, cardData.costMana, cardData.atk);
        }

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount) => Stat.IncreaseMana(amount);
        public void DecreaseMana(int amount) => Stat.DecreaseMana(amount);

        // 마법 카드 함수 정의
        public void SetMana(int amount) => Stat.SetMana(amount);
        public void SetAttack(int amount) => Stat.SetAtk(amount);
        public void SetHandOnCard(bool isHandOnCard) => IsHandOnCard = isHandOnCard;
        public void SetSelectAttackType(bool isSelectAttackType) => IsSelectAttackType = isSelectAttackType;
        public void SetTargetSlotNumbers(List<int> slotNumbers) => targetSlotNumbers = slotNumbers;
    }
}