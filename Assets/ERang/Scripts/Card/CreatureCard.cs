using ERang.Data;

namespace ERang
{
    // 크리쳐 카드
    public class CreatureCard : BaseCard, IAttackable, IManaManageable
    {
        public CreatureCard()
        {
        }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            State = new CardState(cardData.hp, cardData.def, cardData.costMana, cardData.atk, cardData.hp);
        }

        // IAttackable 인터페이스 정의
        public void IncreaseAttack(int amount) => State.IncreaseAtk(amount);
        public void DecreaseAttack(int amount) => State.DecreaseAtk(amount);

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount) => State.IncreaseMana(amount);
        public void DecreaseMana(int amount) => State.DecreaseMana(amount);

        // 크리쳐 카드 함수 정의
        public void SetHp(int amount) => State.SetHp(amount);
        public void SetMana(int amount) => State.SetMana(amount);
        public void SetAttack(int amount) => State.SetAtk(amount);
    }
}