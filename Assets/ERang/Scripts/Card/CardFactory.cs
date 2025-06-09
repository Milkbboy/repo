using ERang.Data;

namespace ERang
{
    public class CardFactory : ICardFactory
    {
        private readonly AiLogic aiLogic;

        public CardFactory(AiLogic aiLogic)
        {
            this.aiLogic = aiLogic;
        }

        public BaseCard CreateCard(CardData cardData)
        {
            BaseCard card = cardData.cardType switch
            {
                CardType.Creature => new CreatureCard(cardData),
                CardType.Monster => new CreatureCard(cardData),
                CardType.Building => new BuildingCard(cardData),
                CardType.Charm => CreateMagicCard(cardData),
                CardType.Curse => CreateMagicCard(cardData),
                CardType.Magic => CreateMagicCard(cardData),
                _ => null
            };

            return card;
        }

        public MagicCard CreateMagicCard(CardData cardData)
        {
            var magicCard = new MagicCard(cardData);

            magicCard.SetHandOnCard(aiLogic.IsHandOnCard(magicCard));
            magicCard.SetTargetSlotNumbers(aiLogic.GetTargetSlotNumbers(magicCard));
            magicCard.SetSelectAttackType(aiLogic.IsSelectAttackType(magicCard));

            return magicCard;
        }

        public BaseCard CreateHpCard(CardData cardData, int hp) => new HpCard(cardData, hp);
        public BaseCard CreateGoldCard(CardData cardData, int gold) => new GoldCard(cardData, gold);
    }
}
