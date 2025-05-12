namespace ERang
{
    public class MasterCard : BaseCard, IManaManageable
    {
        public override int Hp => State.Hp;
        public override int Def => State.Def;
        public override int Mana => State.Mana;
        public int MaxMana => State.MaxMana;

        private Master master;

        public MasterCard()
        {
        }

        public MasterCard(Master master) : base(master.MasterId, CardType.Master, 0, master.CardImage)
        {
            this.master = master;
            State = new CardState(master.Hp, master.Def, master.Mana, 0, master.MaxHp, master.MaxMana);
        }

        public void SetHp(int amount)
        {
            State.SetHp(amount);
        }

        public override void SetDefense(int amount)
        {
            State.SetDef(amount);
        }

        public void SetMana(int amount)
        {
            State.SetMana(amount);
        }

        public override void RestoreHealth(int amount)
        {
            State.RestoreHealth(amount);
            master.SetHp(State.Hp);
        }

        public override void TakeDamage(int amount)
        {
            State.TakeDamage(amount);
            master.SetHp(State.Hp);
        }

        public override void IncreaseDefense(int amount)
        {
            State.IncreaseDefense(amount);
        }

        public override void DecreaseDefense(int amount)
        {
            State.DecreaseDefense(amount);
        }

        public void IncreaseMana(int amount)
        {
            State.IncreaseMana(amount);
        }

        public void DecreaseMana(int amount)
        {
            State.DecreaseMana(amount);
        }

        public void ResetMana()
        {
            State.ResetMana();
        }
    }
}