namespace ERang
{
    public class MasterCard : BaseCard, IManaManageable
    {
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

        public override void TakeDamage(int amount)
        {
            State.TakeDamage(amount);
            master.SetHp(State.Hp);
        }

        public override void RestoreHealth(int amount)
        {
            State.RestoreHealth(amount);
            master.SetHp(State.Hp);
        }

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount) => State.IncreaseMana(amount);
        public void DecreaseMana(int amount) => State.DecreaseMana(amount);

        // 마스터 카드 함수 정의
        public void SetHp(int amount) => State.SetHp(amount);
        public void SetMana(int amount) => State.SetMana(amount);
        public void ResetMana() => State.ResetMana();
    }
}