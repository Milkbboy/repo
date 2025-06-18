namespace ERang
{
    public class MasterCard : BaseCard, IManaManageable
    {
        const int AI_GROUP_ID = 0;

        public int MaxMana => Stat.MaxMana;

        public MasterCard()
        {
        }

        public MasterCard(Player player) : base(player.MasterId, CardType.Master, player.NameDesc, AI_GROUP_ID, player.CardImage)
        {
            Stat = player.Stat;
        }

        public override void TakeDamage(int amount)
        {
            Stat.TakeDamage(amount);
        }

        public override void RestoreHealth(int amount)
        {
            Stat.RestoreHealth(amount);
        }

        // IManaManageable 인터페이스 정의
        public void IncreaseMana(int amount)
        {
            Stat.IncreaseMana(amount);
        }
        public void DecreaseMana(int amount)
        {
            Stat.DecreaseMana(amount);
        }

        // 마스터 카드 함수 정의
        public void SetHp(int amount)
        {
            Stat.SetHp(amount);
        }

        public void SetMana(int amount)
        {
            Stat.SetMana(amount);
        }

        public void ResetMana()
        {
            Stat.SetMana(0);
        }
    }
}