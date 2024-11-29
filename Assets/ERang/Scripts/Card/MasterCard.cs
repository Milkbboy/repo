namespace ERang
{
    public class MasterCard : CreatureCard, IManaManageable
    {
        public int MaxMana => maxMana;
        private int maxMana;

        public MasterCard()
        {
        }

        public MasterCard(Master master) : base(master)
        {
            maxMana = master.MaxMana;
        }

        public void IncreaseMana(int amount)
        {
            mana += amount;
        }

        public void DecreaseMana(int amount)
        {
            mana -= amount;
        }

        public void ResetMana()
        {
            mana = 0;
        }
    }
}