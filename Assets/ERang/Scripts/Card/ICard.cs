using ERang.Data;

namespace ERang
{
    public interface ICard
    {
        public string Uid { get; set; }
        public int Id { get; set; }
        public CardType CardType { get; set; }
        public int AiGroupId { get; set; } // 해당 카드가 가지고 있는 Ai 그룹의 Id 값
        public int AiGroupIndex { get; set; } // 현재 설정된 Ai 그룹의 인덱스 값
    }

    public interface IDamageable
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Def { get; set; }

        // public void Die();
        // public void TakeDamage(int amount);
        // public void RestoreHealth(int amount);
    }

    // 기본 카드
    public class BaseCard : ICard
    {
        public string Uid { get; set; }
        public int Id { get; set; }
        public CardType CardType { get; set; }
        public int AiGroupId { get; set; }
        public int AiGroupIndex { get; set; }
        public bool inUse { get; set; }
        public bool isExtinction { get; set; }

        public BaseCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            CardType = cardData.cardType;
            inUse = cardData.inUse;
            isExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            AiGroupIndex = 0;
        }
    }

    // 크리쳐 카드
    public class CreatureCard : BaseCard, IDamageable
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Mana { get; set; }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            Hp = cardData.hp;
            MaxHp = cardData.hp;
            Atk = cardData.atk;
            Def = cardData.def;
            Mana = cardData.costMana;
        }
    }

    // 건물 카드
    public class BuildingCard : BaseCard
    {
        public int Gold { get; set; }

        public BuildingCard(CardData cardData) : base(cardData)
        {
            Gold = cardData.costGold;
        }
    }

    // 마법 카드
    public class MagicCard : BaseCard
    {
        public int Atk { get; set; }
        public int Mana { get; set; }

        public MagicCard(CardData cardData) : base(cardData)
        {
            Atk = cardData.atk;
            Mana = cardData.costMana;
        }
    }
}