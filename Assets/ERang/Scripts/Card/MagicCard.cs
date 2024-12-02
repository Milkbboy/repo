using ERang.Data;

namespace ERang
{
    // 마법 카드
    public class MagicCard : BaseCard
    {
        public override int Atk { get; set; }
        public override int Mana { get; set; }

        public MagicCard(CardData cardData) : base(cardData)
        {
            Atk = cardData.atk;
            Mana = cardData.costMana;
        }

        public void SetMana(int amount)
        {
            Mana = amount;
        }

        public void SetAttack(int amount)
        {
            Atk = amount;
        }
    }
}