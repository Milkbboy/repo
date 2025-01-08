using ERang.Data;

namespace ERang
{
    // 크리쳐 카드
    public class CreatureCard : BaseCard, IAttackable
    {
        public override int Hp => hp;
        public override int Def => def;
        public override int Mana => mana;
        public override int Atk => atk;

        public int MaxHp => maxHp;

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

        public void SetHp(int amount)
        {
            hp = amount;
        }

        public void SetMana(int amount)
        {
            mana = amount;
        }

        public override void SetDefense(int amount)
        {
            def = amount;

            if (def < 0)
                def = 0;
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