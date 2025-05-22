using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 카드 생성을 담당하는 팩토리 클래스
    /// </summary>
    public static class CardFactory
    {
        /// <summary>
        /// CardData를 기반으로 GameCard 인스턴스 생성
        /// </summary>
        public static GameCard CreateCard(CardData cardData)
        {
            switch (cardData.cardType)
            {
                case CardType.Master:
                    return new MasterCard(cardData);

                case CardType.Magic:
                case CardType.Individuality:
                    return new MagicCard(cardData);

                case CardType.Creature:
                    return new CreatureCard(cardData);

                case CardType.Building:
                    return new BuildingCard(cardData);

                case CardType.Monster:
                    return new CreatureCard(cardData); // 몬스터도 크리쳐와 동일한 방식으로 처리

                case CardType.EnemyMaster:
                    return new MasterCard(cardData);

                case CardType.Charm:
                case CardType.Curse:
                    return new MagicCard(cardData); // 축복/저주 카드도 마법 카드로 처리

                default:
                    Debug.LogWarning($"Unknown card type: {cardData.cardType}. Creating as Creature.");
                    return new CreatureCard(cardData);
            }
        }

        /// <summary>
        /// 골드 카드 생성
        /// </summary>
        public static GoldCard CreateGoldCard(int gold)
        {
            return new GoldCard(gold);
        }

        /// <summary>
        /// HP 회복 카드 생성
        /// </summary>
        public static GameCard CreateHpCard(int hp)
        {
            var hpCard = new CreatureCard();
            hpCard.SetHp(hp);
            return hpCard;
        }

        /// <summary>
        /// 빈 카드 생성 (기본값으로)
        /// </summary>
        public static GameCard CreateEmptyCard(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Master:
                case CardType.EnemyMaster:
                    return new MasterCard();

                case CardType.Magic:
                case CardType.Individuality:
                case CardType.Charm:
                case CardType.Curse:
                    return new MagicCard();

                case CardType.Creature:
                case CardType.Monster:
                    return new CreatureCard();

                case CardType.Building:
                    return new BuildingCard();

                default:
                    return new CreatureCard();
            }
        }
    }
}