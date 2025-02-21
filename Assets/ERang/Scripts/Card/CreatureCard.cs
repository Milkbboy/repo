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
        }

        public override void SetDefense(int def)
        {
            this.def = def;

            if (this.def < 0)
                this.def = 0;
        }

        public override void RestoreHealth(int hp)
        {
            this.hp += hp;
        }

        public override void TakeDamage(int damage)
        {
            def -= damage;

            if (def < 0)
            {
                hp += def;
                def = 0;
            }

            if (hp <= 0)
                hp = 0;
        }

        public override void IncreaseDefense(int def)
        {
            this.def += def;
        }

        public override void DecreaseDefense(int def)
        {
            this.def -= def;

            if (this.def < 0)
                this.def = 0;
        }
    }
}