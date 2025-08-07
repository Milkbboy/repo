using ERang.Data;

namespace ERang
{
    // 크리쳐 카드
    public class CreatureCard : BaseCard, IAttackable, IManaManageable
    {
        public CreatureCard()
        {
        }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            Stat = new CardStat(cardData.hp, cardData.def, cardData.costMana, cardData.atk, cardData.hp, cardData.costMana, cardData.costSatiety);
        }

        // IAttackable 인터페이스 정의
        public void IncreaseAttack(int amount) => Stat.IncreaseAtk(amount);
        public void DecreaseAttack(int amount) => Stat.DecreaseAtk(amount);

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount) => Stat.IncreaseMana(amount);
        public void DecreaseMana(int amount) => Stat.DecreaseMana(amount);

        // 크리쳐 카드 함수 정의
        public void SetHp(int amount) => Stat.SetHp(amount);
        public void SetMana(int amount) => Stat.SetMana(amount);
        public void SetAttack(int amount) => Stat.SetAtk(amount);
    }
}