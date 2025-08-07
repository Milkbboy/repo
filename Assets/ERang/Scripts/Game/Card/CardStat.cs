using UnityEngine;

namespace ERang
{
    public class CardStat
    {
        public int Hp { get; private set; }
        public int Def { get; private set; }
        public int Mana { get; private set; }
        public int Atk { get; private set; }
        public int MaxHp { get; private set; }
        public int MaxMana { get; private set; }
        public int CostSatiety { get; private set; }

        public CardStat(int hp, int def, int mana, int atk, int maxHp = 0, int maxMana = 0, int costSatiety = 0)
        {
            Hp = hp;
            Def = def;
            Mana = mana;
            Atk = atk;
            MaxHp = maxHp > 0 ? maxHp : hp;
            MaxMana = maxMana > 0 ? maxMana : mana;
            CostSatiety = costSatiety;
        }

        public void SetHp(int value) => Hp = Mathf.Clamp(value, 0, MaxHp);
        public void SetDef(int value) => Def = Mathf.Max(0, value);
        public void SetMana(int value) => Mana = Mathf.Clamp(value, 0, MaxMana);
        public void SetAtk(int value) => Atk = Mathf.Max(0, value);
        public void SetCostSatiety(int value) => CostSatiety = Mathf.Max(0, value);

        public void RestoreHealth(int amount) => SetHp(Hp + amount);
        public void IncreaseAtk(int value) => SetAtk(Atk + value);
        public void DecreaseAtk(int value) => SetAtk(Atk - value);
        public void IncreaseDefense(int amount) => SetDef(Def + amount);
        public void DecreaseDefense(int amount) => SetDef(Def - amount);
        public void IncreaseMana(int amount) => SetMana(Mana + amount);
        public void DecreaseMana(int amount) => SetMana(Mana - amount);
        public void ResetMana() => SetMana(0);

        public void TakeDamage(int amount)
        {
            int remainingDamage = amount;

            if (Def > 0)
            {
                Def -= remainingDamage;
                if (Def < 0)
                {
                    remainingDamage = -Def;
                    Def = 0;
                }
                else
                {
                    remainingDamage = 0;
                }
            }

            if (remainingDamage > 0)
            {
                SetHp(Hp - remainingDamage);
            }
        }
    }
}