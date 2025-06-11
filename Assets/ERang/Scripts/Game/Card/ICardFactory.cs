using ERang.Data;

namespace ERang
{
    public interface ICardFactory
    {
        BaseCard CreateCard(CardData cardData);
        BaseCard CreateHpCard(CardData cardData, int hp);
        BaseCard CreateGoldCard(CardData cardData, int gold);
    }
}