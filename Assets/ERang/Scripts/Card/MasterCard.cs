using System.Diagnostics;

namespace ERang
{
    public class MasterCard : BaseCard, IDefensible, IManaManageable
    {
        public override int Hp => hp;
        public override int Def => def;

        public int Mana => mana;
        public int MaxMana => maxMana;

        private int hp;
        private int def;
        private int mana;
        private int maxMana;

        public MasterCard()
        {
        }

        public MasterCard(Master master) : base(master.MasterId, CardType.Master, 0, master.CardImage)
        {
            hp = master.Hp;
            def = master.Def;
            mana = master.Mana;
            maxMana = master.MaxMana;
        }

        public void SetHp(int amount)
        {
            hp = amount;
        }

        public void SetDefense(int amount)
        {
            def = amount;
        }

        public void SetMana(int amount)
        {
            mana = amount;
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