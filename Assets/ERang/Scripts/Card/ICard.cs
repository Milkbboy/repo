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
        public void TakeDamage(int amount);
        public void RestoreHealth(int amount);
    }

    public interface IDefensible
    {
        public void IncreaseDefense(int amount);
        public void DecreaseDefense(int amount);
    }

    public interface IAttackable
    {
        public void IncreaseAttack(int amount);
        public void DecreaseAttack(int amount);
    }

    public interface IManaManageable
    {
        public void IncreaseMana(int amount);
        public void DecreaseMana(int amount);
    }
}