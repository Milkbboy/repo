namespace ERang
{
    public class MasterCard : BaseCard, IManaManageable
    {
        const int AI_GROUP_ID = 0;

        public int MaxMana => State.MaxMana;
        private Player player;

        public MasterCard()
        {
        }

        public MasterCard(Player player) : base(player.MasterId, CardType.Master, AI_GROUP_ID, player.CardImage)
        {
            this.player = player;
            State = new CardState(player.Hp, player.Def, player.Mana, AI_GROUP_ID, player.MaxHp, player.MaxMana);
        }

        public override void TakeDamage(int amount)
        {
            State.TakeDamage(amount);
            player.SetHp(State.Hp);
        }

        public override void RestoreHealth(int amount)
        {
            State.RestoreHealth(amount);
            player.SetHp(State.Hp);
        }

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount)
        {
            State.IncreaseMana(amount);
            player.SetMana(State.Mana);
        }
        public void DecreaseMana(int amount)
        {
            State.DecreaseMana(amount);
            player.SetMana(State.Mana);
        }

        // 마스터 카드 함수 정의
        public void SetHp(int amount)
        {
            State.SetHp(amount);
            player.SetHp(State.Hp);
        }

        public void SetMana(int amount)
        {
            State.SetMana(amount);
            player.SetMana(State.Mana);
        }

        public void ResetMana()
        {
            State.SetMana(0);
            player.SetMana(State.Mana);
        }
    }
}