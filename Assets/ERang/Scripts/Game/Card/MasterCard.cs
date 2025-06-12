namespace ERang
{
    public class MasterCard : BaseCard, IManaManageable
    {
        const int AI_GROUP_ID = 0;

        public int MaxMana => Stat.MaxMana;
        private Player player;

        public MasterCard()
        {
        }

        public MasterCard(Player player) : base(player.MasterId, CardType.Master, player.NameDesc, AI_GROUP_ID, player.CardImage)
        {
            this.player = player;
            Stat = new CardStat(player.Hp, player.Def, player.Mana, AI_GROUP_ID, player.MaxHp, player.MaxMana);
        }

        public override void TakeDamage(int amount)
        {
            Stat.TakeDamage(amount);
            player.SetHp(Stat.Hp);
        }

        public override void RestoreHealth(int amount)
        {
            Stat.RestoreHealth(amount);
            player.SetHp(Stat.Hp);
        }

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount)
        {
            Stat.IncreaseMana(amount);
            player.SetMana(Stat.Mana);
        }
        public void DecreaseMana(int amount)
        {
            Stat.DecreaseMana(amount);
            player.SetMana(Stat.Mana);
        }

        // 마스터 카드 함수 정의
        public void SetHp(int amount)
        {
            Stat.SetHp(amount);
            player.SetHp(Stat.Hp);
        }

        public void SetMana(int amount)
        {
            Stat.SetMana(amount);
            player.SetMana(Stat.Mana);
        }

        public void ResetMana()
        {
            Stat.SetMana(0);
            player.SetMana(Stat.Mana);
        }
    }
}