using ERang.Data;

namespace ERang
{
    public class GoldCard : BaseCard
    {
        public int Gold { get; set; }

        public GoldCard(CardData cardData, int gold) : base(cardData)
        {
            Gold = gold;
        }
    }
}
