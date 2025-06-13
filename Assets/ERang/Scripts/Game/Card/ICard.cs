namespace ERang
{
    /// <summary>
    /// 데미지를 받을 수 있는 카드를 위한 인터페이스
    /// </summary>
    public interface IDamageable
    {
        // public void Die();
        void TakeDamage(int amount);
        void RestoreHealth(int amount);
    }

    /// <summary>
    /// 방어력을 가진 카드를 위한 인터페이스
    /// </summary>
    public interface IDefensible
    {
        void IncreaseDefense(int amount);
        void DecreaseDefense(int amount);
    }

    /// <summary>
    /// 공격력을 가진 카드를 위한 인터페이스
    /// </summary>
    public interface IAttackable
    {
        void IncreaseAttack(int amount);
        void DecreaseAttack(int amount);
    }

    /// <summary>
    /// 마나를 관리할 수 있는 카드를 위한 인터페이스
    /// </summary>
    public interface IManaManageable
    {
        void IncreaseMana(int amount);
        void DecreaseMana(int amount);
    }
}