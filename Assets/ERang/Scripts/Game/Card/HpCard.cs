using ERang.Data;

namespace ERang
{
    public class HpCard : BaseCard
    {
        public HpCard(CardData cardData, int hp) : base(cardData)
        {
            Stat.SetHp(hp);
        }
    }
}
