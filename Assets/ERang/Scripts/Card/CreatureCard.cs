using ERang.Data;

namespace ERang
{
    // 크리쳐 카드
    public class CreatureCard : BaseCard, IAttackable, IManaManageable
    {
        public override int Hp => State.Hp;
        public override int Def => State.Def;
        public override int Mana => State.Mana;
        public override int Atk => State.Atk;

        public CreatureCard()
        {
        }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            State = new CardState(cardData.hp, cardData.def, cardData.costMana, cardData.atk, cardData.hp);
        }

        public void SetHp(int amount)
        {
            State.SetHp(amount);
        }

        public void SetMana(int amount)
        {
            State.SetMana(amount);
        }

        public void SetAttack(int amount)
        {
            State.SetAtk(amount);
        }

        public void IncreaseAttack(int amount)
        {
            State.SetAtk(State.Atk + amount);
        }

        public void DecreaseAttack(int amount)
        {
            State.SetAtk(State.Atk - amount);
        }

        public void IncreaseMana(int amount)
        {
            State.IncreaseMana(amount);
        }

        public void DecreaseMana(int amount)
        {
            State.DecreaseMana(amount);
        }

        public override void SetDefense(int amount)
        {
            State.SetDef(amount);
        }

        public override void RestoreHealth(int amount)
        {
            State.SetHp(State.Hp + amount);
        }

        public override void TakeDamage(int amount)
        {
            State.SetHp(State.Hp - amount);
        }

        public override void IncreaseDefense(int amount)
        {
            State.SetDef(State.Def + amount);
        }

        public override void DecreaseDefense(int amount)
        {
            State.SetDef(State.Def - amount);
        }
    }
}