namespace ERang
{
    public class MasterCard : BaseCard, IManaManageable
    {
        public int MaxMana => maxMana;

        private int maxMana;

        public MasterCard()
        {
        }

        public MasterCard(Master master) : base(master.MasterId, CardType.Master, 0, master.CardImage)
        {
            maxMana = master.MaxMana;
        }

        public override void RestoreHealth(int amount)
        {
            hp += amount;
        }

        public override void TakeDamage(int amount)
        {
            def -= amount;

            if (def < 0)
            {
                hp += def;
                def = 0;
            }

            if (hp <= 0)
                hp = 0;
        }

        public override void IncreaseDefense(int amount)
        {
            def += amount;
        }

        public override void DecreaseDefense(int amount)
        {
            def -= amount;

            if (def < 0)
                def = 0;
        }

        public void ResetMana()
        {
            mana = 0;
        }
    }
}