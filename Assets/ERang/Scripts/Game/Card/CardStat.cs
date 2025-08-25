using System.Linq;
using UnityEngine;

namespace ERang
{
    public class CardStat
    {
        private int baseHp;
        private int baseDef;
        private int baseMana;
        private int baseAtk;
        private int maxHp;
        private int maxMana;
        private int costSatiety;
        private CardAbilitySystem abilitySystem;

        public int Hp => CalculateStat(StatType.Hp, baseHp);
        public int Def => CalculateStat(StatType.Def, baseDef);
        public int Mana => CalculateStat(StatType.Mana, baseMana);
        public int Atk => CalculateStat(StatType.Atk, baseAtk);
        public int MaxHp => maxHp;
        public int MaxMana => maxMana;
        public int CostSatiety => costSatiety;

        public CardStat(int hp, int def, int mana, int atk, CardAbilitySystem abilitySystem, int maxHp = 0, int maxMana = 0, int costSatiety = 0)
        {
            this.baseHp = hp;
            this.baseDef = def;
            this.baseMana = mana;
            this.baseAtk = atk;
            this.maxHp = maxHp > 0 ? maxHp : hp;
            this.maxMana = maxMana > 0 ? maxMana : mana;
            this.costSatiety = costSatiety;
            this.abilitySystem = abilitySystem;
        }

        private int CalculateStat(StatType statType, int baseValue)
        {
            if (abilitySystem == null) return baseValue;

            int finalValue = baseValue;
            var statModifyingAbilities = GetStatModifyingAbilities(statType);

            // ArmorBreak가 있는지 확인
            if (statType == StatType.Def &&
                statModifyingAbilities.Any(a => a.abilityType == AbilityType.ArmorBreak))
            {
                return 0; // ArmorBreak가 있으면 다른 모든 효과를 무시하고 0 반환
            }

            foreach (var group in statModifyingAbilities.GroupBy(a => a.abilityId))
            {
                var latestAbility = group
                    .OrderByDescending(a => a.GetLatestItem()?.createdDt ?? 0)
                    .First();

                finalValue = ApplyAbilityToStat(finalValue, latestAbility);
            }

            return ApplyStatLimits(statType, finalValue);
        }

        public int CalculateStatWithoutAbility(StatType statType, CardAbility excludeAbility)
        {
            if (abilitySystem == null) return GetBaseStat(statType);

            int finalValue = GetBaseStat(statType);

            // ArmorBreak가 있는지 확인 (제외할 어빌리티가 ArmorBreak가 아닌 경우에만)
            var statModifyingAbilities = GetStatModifyingAbilities(statType)
                .Where(a => a != excludeAbility);

            if (statType == StatType.Def &&
                statModifyingAbilities.Any(a => a.abilityType == AbilityType.ArmorBreak))
            {
                return 0; // ArmorBreak가 있으면 다른 모든 효과를 무시하고 0 반환
            }

            foreach (var group in statModifyingAbilities.GroupBy(a => a.abilityId))
            {
                var latestAbility = group
                    .OrderByDescending(a => a.GetLatestItem()?.createdDt ?? 0)
                    .First();

                finalValue = ApplyAbilityToStat(finalValue, latestAbility);
            }

            return ApplyStatLimits(statType, finalValue);
        }

        private System.Collections.Generic.IEnumerable<CardAbility> GetStatModifyingAbilities(StatType statType)
        {
            return statType switch
            {
                StatType.Atk => abilitySystem.CardAbilities.Where(a =>
                    a.abilityType == AbilityType.AtkUp ||
                    a.abilityType == AbilityType.Weaken),
                StatType.Def => abilitySystem.CardAbilities.Where(a =>
                    a.abilityType == AbilityType.DefUp ||
                    a.abilityType == AbilityType.ArmorBreak ||
                    a.abilityType == AbilityType.BrokenDef),
                _ => Enumerable.Empty<CardAbility>()
            };
        }

        private int ApplyAbilityToStat(int currentValue, CardAbility ability)
        {
            return ability.abilityType switch
            {
                AbilityType.AtkUp => currentValue + ability.abilityValue,
                AbilityType.Weaken => currentValue - ability.abilityValue,
                AbilityType.DefUp => currentValue + ability.abilityValue,
                AbilityType.BrokenDef => currentValue - ability.abilityValue,
                _ => currentValue
            };
        }

        private int ApplyStatLimits(StatType statType, int value)
        {
            return statType switch
            {
                StatType.Hp => Mathf.Clamp(value, 0, maxHp),
                StatType.Mana => Mathf.Clamp(value, 0, maxMana),
                _ => Mathf.Max(0, value)
            };
        }

        private int GetBaseStat(StatType statType) => statType switch
        {
            StatType.Hp => baseHp,
            StatType.Def => baseDef,
            StatType.Mana => baseMana,
            StatType.Atk => baseAtk,
            _ => 0
        };

        public void SetBaseHp(int value) => baseHp = Mathf.Clamp(value, 0, maxHp);
        public void SetBaseDef(int value) => baseDef = Mathf.Max(0, value);
        public void SetBaseMana(int value) => baseMana = Mathf.Clamp(value, 0, maxMana);
        public void SetBaseAtk(int value) => baseAtk = Mathf.Max(0, value);
        public void SetCostSatiety(int value) => costSatiety = Mathf.Max(0, value);

        public void RestoreHealth(int amount) => SetBaseHp(baseHp + amount);
        public void IncreaseMana(int amount) => SetBaseMana(baseMana + amount);
        public void DecreaseMana(int amount) => SetBaseMana(baseMana - amount);
        public void ResetMana() => SetBaseMana(0);

        public void TakeDamage(int amount)
        {
            int remainingDamage = amount;
            int currentDef = Def;  // 현재 방어력 계산 (ArmorBreak 포함)

            if (currentDef > 0)
            {
                if (remainingDamage >= currentDef)
                {
                    remainingDamage -= currentDef;
                    baseDef = 0;
                }
                else
                {
                    baseDef = currentDef - remainingDamage;
                    remainingDamage = 0;
                }
            }

            if (remainingDamage > 0)
            {
                SetBaseHp(baseHp - remainingDamage);
            }
        }
    }
}