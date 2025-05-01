using ERang.Data;

namespace ERang
{
    public class HpCard : BaseCard
    {
        public int Hp { get; set; }

        public HpCard(CardData cardData, int hp) : base(cardData)
        {
            Hp = hp;
        }
    }
}
