using ERang.Data;

namespace ERang
{
    public class Card
    {
        public string uid;
        public int id;
        public CardType type;
        public int costMana; // 소환에 필요한 마나
        public int costGold; // 소환에 필요한 골드
        public int hp; // 체력 값
        public int atk; // 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int def; // 초기 방어력 값

        public Card(CardData cardData)
        {
            uid = Utils.GenerateShortUniqueID();
            id = cardData.card_id;
            type = cardData.cardType;
            costMana = cardData.costMana;
            costGold = cardData.costGold;
            hp = cardData.hp;
            atk = cardData.atk;
            def = cardData.def;
        }
    }
}