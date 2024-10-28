using ERang.Data;

namespace ERang
{
    public class Card
    {
        public int AiGroupIndex { get => aiGroupIndex; set => aiGroupIndex = value; }
        public string Uid => uid;
        public int Id => id;
        public CardType Type => type;
        public int AiGroupId => aiGroupId;

        public int costMana; // 소환에 필요한 마나
        public int costGold; // 소환에 필요한 골드
        public int hp; // 체력 값
        public int maxHp; // 최대 체력 값
        public int atk; // 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int def; // 초기 방어력 값
        public bool inUse; // 카드 사용 가능 여부
        public bool isExtinction; // Bool 값으로 True 시 해당 카드는 사용 시 해당 전투에서 카드 덱에서 삭제된다.

        private readonly string uid;
        private readonly int id;
        private CardType type;
        private int aiGroupId; // 해당 카드가 가지고 있는 Ai 그룹의 Id 값
        private int aiGroupIndex = 0; // 현재 설정된 Ai 그룹의 인덱스 값

        public Card(CardData cardData)
        {
            uid = Utils.GenerateShortUniqueID();
            id = cardData.card_id;
            type = cardData.cardType;
            costMana = cardData.costMana;
            costGold = cardData.costGold;
            hp = cardData.hp;
            maxHp = hp;
            atk = cardData.atk;
            def = cardData.def;
            inUse = cardData.inUse;
            isExtinction = cardData.extinction;
            aiGroupId = cardData.aiGroup_id;
        }

        public Card(int masterId, CardType type, int hp, int maxHp, int atk, int def)
        {
            uid = Utils.GenerateShortUniqueID();
            id = masterId;
            this.type = type;
            costMana = 0;
            costGold = 0;
            this.hp = hp;
            this.maxHp = maxHp;
            this.atk = atk;
            this.def = def;
            isExtinction = false;
            aiGroupId = 0;
        }

        public void SetHp(int hp)
        {
            this.hp = hp;
        }

        public void SetAtk(int atk)
        {
            this.atk = atk;
        }

        public void SetDef(int def)
        {
            this.def = def;
        }

        /// <summary>
        /// 카드의 체력을 증가 또는 감소시킨다.
        /// </summary>
        /// <param name="value"></param>
        public void AddHp(int value)
        {
            hp += value;

            if (hp > maxHp)
                hp = maxHp;

            if (hp <= 0)
                hp = 0;
        }

        /// <summary>
        /// 카드의 방어력을 증가 또는 감소시킨다.
        /// - 실제 데이터는 음수 값까지 허용하지만, UI에 표시할 때는 0 이상으로 표시한다.
        ///   어빌리티 적용 방식때문에 음수까지 허용
        /// </summary>
        public void AddDef(int value)
        {
            def += value;

            if (def < 0)
                def = 0;
        }

        /// <summary>
        /// 공격력을 증가 또는 감소시킨다.
        /// - 실제 데이터는 음수 값까지 허용하지만, UI에 표시할 때는 0 이상으로 표시한다.
        /// </summary>
        public void AddAtk(int value)
        {
            atk += value;

            if (atk < 0)
                atk = 0;
        }
    }
}