using ERang.Data;

namespace ERang
{
    // 크리쳐 카드
    public class CreatureCard : BaseCard, IDamageable, IDefensible, IAttackable
    {
        public int Hp => hp;
        public int MaxHp => maxHp;
        public int Def => def;
        public int Mana => mana;

        public int Atk => atk;

        private int hp;
        private int maxHp;
        private int atk;
        private int def;
        protected int mana;

        public CreatureCard()
        {
        }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            hp = cardData.hp;
            maxHp = cardData.hp;
            atk = cardData.atk;
            def = cardData.def;
            mana = cardData.costMana;
        }

        public CreatureCard(Master master) : base(master.MasterId, CardType.Master, 0, master.CardImage)
        {
            hp = master.Hp;
            maxHp = master.MaxHp;
            def = master.Def;
            mana = master.Mana;
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

        public void TakeDamage(int amount)
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

        public void RestoreHealth(int amount)
        {
            hp += amount;
        }

        public void IncreaseDefense(int amount)
        {
            def += amount;
        }

        public void DecreaseDefense(int amount)
        {
            def -= amount;

            if (def < 0)
                def = 0;
        }

        public void IncreaseAttack(int amount)
        {
            atk += amount;
        }

        public void DecreaseAttack(int amount)
        {
            atk -= amount;
        }

        public void SetAttack(int amount)
        {
            atk = amount;
        }
    }
}