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

        // 카드 정보를 위한 추가 프로퍼티 (로그용)
        public string CardName { get; private set; } = "UnknownCard";

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

        // 카드 이름 설정 (GameCard에서 호출)
        public void SetCardName(string cardName)
        {
            CardName = cardName;
        }

        public void SetHp(int value, string reason = "")
        {
            int oldHp = Hp;
            Hp = Mathf.Clamp(value, 0, MaxHp);

            // 변화가 있을 때만 로그 출력
            if (oldHp != Hp)
            {
                GameLogger.LogCardState(CardName, "HP", oldHp, Hp, reason);
            }

            OnHpChanged?.Invoke(Hp);
        }

        public void SetMaxHp(int value, string reason = "")
        {
            int oldMaxHp = MaxHp;
            MaxHp = Mathf.Max(0, value);

            if (oldMaxHp != MaxHp)
            {
                GameLogger.LogCardState(CardName, "최대 HP", oldMaxHp, MaxHp, reason);
            }

            OnMaxHpChanged?.Invoke(MaxHp);
        }

        public void SetMaxMana(int value, string reason = "")
        {
            int oldMaxMana = MaxMana;
            MaxMana = Mathf.Max(0, value);

            if (oldMaxMana != MaxMana)
            {
                GameLogger.LogCardState(CardName, "최대 마나", oldMaxMana, MaxMana, reason);
            }

            OnMaxManaChanged?.Invoke(MaxMana);
        }

        public void SetDef(int value, string reason = "")
        {
            int oldDef = Def;
            Def = Mathf.Max(0, value);

            if (oldDef != Def)
            {
                GameLogger.LogCardState(CardName, "DEF", oldDef, Def, reason);
            }

            OnDefChanged?.Invoke(Def);
        }

        public void SetMana(int value, string reason = "")
        {
            int oldMana = Mana;
            Mana = Mathf.Clamp(value, 0, MaxMana);

            if (oldMana != Mana)
            {
                GameLogger.LogCardState(CardName, "마나", oldMana, Mana, reason);
            }

            OnManaChanged?.Invoke(Mana);
        }

        public void SetAtk(int value, string reason = "")
        {
            int oldAtk = Atk;
            Atk = Mathf.Max(0, value);

            if (oldAtk != Atk)
            {
                GameLogger.LogCardState(CardName, "ATK", oldAtk, Atk, reason);
            }

            OnAtkChanged?.Invoke(Atk);
        }

        public void TakeDamage(int amount, string source = "")
        {
            if (amount <= 0) return;

            int originalHp = Hp;
            int originalDef = Def;
            int remainingDamage = amount;

            GameLogger.LogAbilityDetail($"데미지 적용 전 상태 - HP: {Hp}, DEF: {Def}");

            // 방어력부터 차감
            if (Def > 0)
            {
                int defReduction = Mathf.Min(Def, remainingDamage);
                SetDef(Def - defReduction, source);
                remainingDamage -= defReduction;

                GameLogger.LogAbilityDetail($"방어력 감소: {defReduction}, 남은 데미지: {remainingDamage}");
            }

            // 남은 데미지로 체력 차감
            if (remainingDamage > 0)
            {
                SetHp(Hp - remainingDamage, source);
                GameLogger.LogAbilityDetail($"체력 감소: {remainingDamage}");
            }

            // 총 데미지 요약 로그
            int totalHpLoss = originalHp - Hp;
            int totalDefLoss = originalDef - Def;
            string sourceText = string.IsNullOrEmpty(source) ? "" : $" by {source}";

            if (totalHpLoss > 0 || totalDefLoss > 0)
            {
                string damageText = "";
                if (totalDefLoss > 0 && totalHpLoss > 0)
                {
                    damageText = $"DEF -{totalDefLoss}, HP -{totalHpLoss}";
                }
                else if (totalDefLoss > 0)
                {
                    damageText = $"DEF -{totalDefLoss}";
                }
                else
                {
                    damageText = $"HP -{totalHpLoss}";
                }

                GameLogger.LogAbilityDetail($"데미지 적용 완료 - {damageText}, 총 데미지: {amount}{sourceText}");
            }
        }

        public void RestoreHealth(int amount, string source = "")
        {
            if (amount <= 0) return;

            int actualRestore = Mathf.Min(amount, MaxHp - Hp);
            if (actualRestore > 0)
            {
                SetHp(Hp + actualRestore, source);
                GameLogger.LogAbilityDetail($"체력 회복: {actualRestore}/{amount} (최대치 제한)");
            }
            else
            {
                GameLogger.LogAbilityDetail($"체력 회복 불가 - 이미 최대치 ({Hp}/{MaxHp})");
            }
        }

        public void IncreaseDefense(int amount, string source = "")
        {
            if (amount <= 0) return;

            GameLogger.LogAbilityDetail($"방어력 증가: +{amount}");
            SetDef(Def + amount, source);
        }

        public void DecreaseDefense(int amount, string source = "")
        {
            if (amount <= 0) return;

            int actualDecrease = Mathf.Min(amount, Def);
            GameLogger.LogAbilityDetail($"방어력 감소: -{actualDecrease}");
            SetDef(Def - actualDecrease, source);
        }

        public void IncreaseAttack(int amount, string source = "")
        {
            if (amount <= 0) return;

            GameLogger.LogAbilityDetail($"공격력 증가: +{amount}");
            SetAtk(Atk + amount, source);
        }

        public void DecreaseAttack(int amount, string source = "")
        {
            if (amount <= 0) return;

            int actualDecrease = Mathf.Min(amount, Atk);
            GameLogger.LogAbilityDetail($"공격력 감소: -{actualDecrease}");
            SetAtk(Atk - actualDecrease, source);
        }

        public void IncreaseMana(int amount, string source = "")
        {
            if (amount <= 0) return;

            int actualIncrease = Mathf.Min(amount, MaxMana - Mana);
            if (actualIncrease > 0)
            {
                SetMana(Mana + actualIncrease, source);
                GameLogger.LogAbilityDetail($"마나 증가: +{actualIncrease}/{amount} (최대치 제한)");
            }
            else
            {
                GameLogger.LogAbilityDetail($"마나 증가 불가 - 이미 최대치 ({Mana}/{MaxMana})");
            }
        }

        public void DecreaseMana(int amount, string source = "")
        {
            if (amount <= 0) return;

            int actualDecrease = Mathf.Min(amount, Mana);
            GameLogger.LogAbilityDetail($"마나 감소: -{actualDecrease}");
            SetMana(Mana - actualDecrease, source);
        }

        public void ResetMana(string reason = "턴 시작")
        {
            if (Mana != 0)
            {
                GameLogger.LogAbilityDetail($"마나 초기화: {Mana} → 0");
                SetMana(0, reason);
            }
        }

        // 상태 요약 로그 (디버깅용)
        public void LogCurrentState(string context = "")
        {
            string contextText = string.IsNullOrEmpty(context) ? "" : $" ({context})";
            GameLogger.LogAbilityDetail($"{CardName} 현재 상태{contextText}: HP {Hp}/{MaxHp}, ATK {Atk}, DEF {Def}, 마나 {Mana}/{MaxMana}");
        }

        // 상태 이상 확인용 메서드들
        public bool IsDead => Hp <= 0;
        public bool IsFullHp => Hp >= MaxHp;
        public bool IsFullMana => Mana >= MaxMana;
        public bool HasDefense => Def > 0;

        // 상태 검증 로그 (기획자용)
        public void ValidateState()
        {
            bool hasIssues = false;
            var issues = new System.Text.StringBuilder();

            if (Hp < 0)
            {
                issues.AppendLine($"   ❌ HP가 음수: {Hp}");
                hasIssues = true;
            }
            if (Hp > MaxHp)
            {
                issues.AppendLine($"   ❌ HP가 최대치 초과: {Hp}/{MaxHp}");
                hasIssues = true;
            }
            if (Mana < 0)
            {
                issues.AppendLine($"   ❌ 마나가 음수: {Mana}");
                hasIssues = true;
            }
            if (Mana > MaxMana)
            {
                issues.AppendLine($"   ❌ 마나가 최대치 초과: {Mana}/{MaxMana}");
                hasIssues = true;
            }
            if (Def < 0)
            {
                issues.AppendLine($"   ❌ 방어력이 음수: {Def}");
                hasIssues = true;
            }
            if (Atk < 0)
            {
                issues.AppendLine($"   ❌ 공격력이 음수: {Atk}");
                hasIssues = true;
            }

            if (hasIssues)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {CardName} 상태 이상 감지:\n{issues.ToString()}");
            }
        }

        // ToString 오버라이드 (디버깅 편의성)
        public override string ToString()
        {
            return $"{CardName} [HP:{Hp}/{MaxHp}, ATK:{Atk}, DEF:{Def}, 마나:{Mana}/{MaxMana}]";
        }
    }
}