using UnityEngine;

namespace ERang
{
    public class CardState
    {
        public int Hp { get; private set; }
        public int Def { get; private set; }
        public int Mana { get; private set; }
        public int Atk { get; private set; }
        public int MaxHp { get; private set; }
        public int MaxMana { get; private set; }

        public event System.Action<int> OnHpChanged;
        public event System.Action<int> OnDefChanged;
        public event System.Action<int> OnManaChanged;
        public event System.Action<int> OnAtkChanged;
        public event System.Action<int> OnMaxHpChanged;
        public event System.Action<int> OnMaxManaChanged;

        public CardState(int hp, int def, int mana, int atk, int maxHp = 0, int maxMana = 0)
        {
            Hp = hp;
            Def = def;
            Mana = mana;
            Atk = atk;
            MaxHp = maxHp > 0 ? maxHp : hp;
            MaxMana = maxMana > 0 ? maxMana : mana;
        }

        public void SetHp(int value)
        {
            Hp = Mathf.Clamp(value, 0, MaxHp);
            OnHpChanged?.Invoke(Hp);
        }

        public void SetMaxHp(int value)
        {
            MaxHp = Mathf.Max(0, value);
            OnMaxHpChanged?.Invoke(MaxHp);
        }

        public void SetMaxMana(int value)
        {
            MaxMana = Mathf.Max(0, value);
            OnMaxManaChanged?.Invoke(MaxMana);
        }

        public void SetDef(int value)
        {
            Def = Mathf.Max(0, value);
            OnDefChanged?.Invoke(Def);
        }

        public void SetMana(int value)
        {
            Mana = Mathf.Clamp(value, 0, MaxMana);
            OnManaChanged?.Invoke(Mana);
        }

        public void SetAtk(int value)
        {
            Atk = Mathf.Max(0, value);
            OnAtkChanged?.Invoke(Atk);
        }

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

        public void RestoreHealth(int amount)
        {
            SetHp(Hp + amount);
        }

        public void IncreaseDefense(int amount)
        {
            SetDef(Def + amount);
        }

        public void DecreaseDefense(int amount)
        {
            SetDef(Def - amount);
        }

        public void IncreaseAttack(int amount)
        {
            SetAtk(Atk + amount);
        }

        public void DecreaseAttack(int amount)
        {
            SetAtk(Atk - amount);
        }

        public void IncreaseMana(int amount)
        {
            SetMana(Mana + amount);
        }

        public void DecreaseMana(int amount)
        {
            SetMana(Mana - amount);
        }

        public void ResetMana()
        {
            SetMana(0);
        }
    }
}