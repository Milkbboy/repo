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
            AbilitySystem = new CardAbilitySystem();
            Stat = new CardStat(cardData.hp, cardData.def, cardData.costMana, cardData.atk, AbilitySystem, cardData.hp, cardData.costMana, cardData.costSatiety);
        }

        // IAttackable 인터페이스 정의
        public void IncreaseAttack(int amount) => Stat.SetBaseAtk(Stat.Atk + amount);
        public void DecreaseAttack(int amount) => Stat.SetBaseAtk(Stat.Atk - amount);

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount) => Stat.SetBaseMana(Stat.Mana + amount);
        public void DecreaseMana(int amount) => Stat.SetBaseMana(Stat.Mana - amount);

        // 크리쳐 카드 함수 정의
        public void SetHp(int amount) => Stat.SetBaseHp(amount);
        public void SetMana(int amount) => Stat.SetBaseMana(amount);
        public void SetAttack(int amount) => Stat.SetBaseAtk(amount);
    }
}