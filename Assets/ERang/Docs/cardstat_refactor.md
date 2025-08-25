# CardStat 리팩토링 구현 가이드

Date: 2025-08-25

## 1. CardStat 클래스 수정

```csharp
public class CardStat
{
    // 기본 스탯
    private int baseHp;
    private int baseDef;
    private int baseMana;
    private int baseAtk;
    private int maxHp;
    private int maxMana;
    private int costSatiety;

    private CardAbilitySystem abilitySystem;

    public CardStat(int hp, int def, int mana, int atk, int maxHp = 0, int maxMana = 0, int costSatiety = 0, CardAbilitySystem abilitySystem = null)
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

    // 스탯 Property - 실시간 계산
    public int Hp => CalculateStat(StatType.Hp, baseHp);
    public int Def => CalculateStat(StatType.Def, baseDef);
    public int Mana => CalculateStat(StatType.Mana, baseMana);
    public int Atk => CalculateStat(StatType.Atk, baseAtk);
    public int MaxHp => maxHp;
    public int MaxMana => maxMana;
    public int CostSatiety => costSatiety;

    // 기본 스탯 설정 메서드들
    public void SetBaseHp(int value) => baseHp = Mathf.Clamp(value, 0, maxHp);
    public void SetBaseDef(int value) => baseDef = Mathf.Max(0, value);
    public void SetBaseMana(int value) => baseMana = Mathf.Clamp(value, 0, maxMana);
    public void SetBaseAtk(int value) => baseAtk = Mathf.Max(0, value);
    public void SetCostSatiety(int value) => costSatiety = Mathf.Max(0, value);

    // 스탯 계산 메서드
    private int CalculateStat(StatType statType, int baseValue)
    {
        if (abilitySystem == null) return baseValue;

        int finalValue = baseValue;

        // ability_id로 그룹화하여 계산
        var statModifyingAbilities = abilitySystem.CardAbilities
            .Where(a => IsStatModifyingAbility(a.abilityType, statType))
            .GroupBy(a => a.abilityId);

        foreach (var group in statModifyingAbilities)
        {
            // 각 ability_id 그룹에서 가장 최근 아이템의 value만 사용
            var latestAbility = group
                .OrderByDescending(a => a.GetLatestItem()?.createdDt ?? 0)
                .First();

            finalValue = ApplyAbilityToStat(finalValue, latestAbility);
        }

        return ApplyStatLimits(statType, finalValue);
    }

    // 어빌리티 타입이 해당 스탯을 수정하는지 확인
    private bool IsStatModifyingAbility(AbilityType abilityType, StatType statType)
    {
        return (statType, abilityType) switch
        {
            (StatType.Atk, AbilityType.AtkUp) => true,
            (StatType.Atk, AbilityType.Weaken) => true,
            (StatType.Def, AbilityType.DefUp) => true,
            (StatType.Def, AbilityType.ArmorBreak) => true,
            _ => false
        };
    }

    // 어빌리티를 스탯에 적용
    private int ApplyAbilityToStat(int currentValue, CardAbility ability)
    {
        // 어빌리티 타입에 따라 다른 적용 방식 사용
        return ability.abilityType switch
        {
            AbilityType.AtkUp => currentValue + ability.abilityValue,
            AbilityType.Weaken => currentValue - ability.abilityValue,
            AbilityType.DefUp => currentValue + ability.abilityValue,
            AbilityType.ArmorBreak => currentValue - ability.abilityValue,
            _ => currentValue
        };
    }

    // 스탯 한계값 적용
    private int ApplyStatLimits(StatType statType, int value)
    {
        return statType switch
        {
            StatType.Hp => Mathf.Clamp(value, 0, maxHp),
            StatType.Mana => Mathf.Clamp(value, 0, maxMana),
            _ => Mathf.Max(0, value)
        };
    }

    // 데미지 처리
    public void TakeDamage(int amount)
    {
        int remainingDamage = amount;

        // 방어력이 있으면 먼저 방어력 감소
        if (Def > 0)
        {
            SetBaseDef(baseDef - remainingDamage);
            remainingDamage = Mathf.Max(0, -baseDef);
            baseDef = Mathf.Max(0, baseDef);
        }

        // 남은 데미지로 체력 감소
        if (remainingDamage > 0)
        {
            SetBaseHp(baseHp - remainingDamage);
        }
    }

    // 기타 유틸리티 메서드들
    public void RestoreHealth(int amount) => SetBaseHp(baseHp + amount);
    public void IncreaseMana(int amount) => SetBaseMana(baseMana + amount);
    public void DecreaseMana(int amount) => SetBaseMana(baseMana - amount);
    public void ResetMana() => SetBaseMana(0);
}

// 스탯 타입 열거형
public enum StatType
{
    Hp,
    Def,
    Mana,
    Atk
}
```

## 2. 주요 변경사항

### 기본 스탯과 계산 스탯 분리
- 모든 기본 스탯은 private 필드로 관리
- 실제 스탯 값은 getter를 통해 실시간 계산
- 기본 스탯 수정은 Set* 메서드를 통해서만 가능

### 스탯 계산 로직
1. 기본 스탯 값으로 시작
2. 관련된 모든 어빌리티를 ability_id로 그룹화
3. 각 그룹에서 가장 최근 value만 사용
4. 모든 어빌리티 효과를 순차적으로 적용
5. 최종값에 한계치 적용

### 어빌리티 효과 적용 규칙
- 같은 ability_id를 가진 어빌리티는 가장 최근 value만 사용
- 다른 ability_id를 가진 같은 타입 어빌리티는 value 합산
- 모든 스탯은 0 이상의 값을 가짐
- HP와 Mana는 최대값 제한이 있음

## 3. 사용 예시

```csharp
// 생성
var abilitySystem = new CardAbilitySystem();
var cardStat = new CardStat(hp: 10, def: 5, mana: 3, atk: 4, abilitySystem: abilitySystem);

// 어빌리티 적용
var atkUpAbility1 = new CardAbility(abilityId: 1)
{
    abilityType = AbilityType.AtkUp,
    abilityValue = 2
};
abilitySystem.AddCardAbility(atkUpAbility1);

var atkUpAbility2 = new CardAbility(abilityId: 2)
{
    abilityType = AbilityType.AtkUp,
    abilityValue = 3
};
abilitySystem.AddCardAbility(atkUpAbility2);

// 스탯 확인
int finalAtk = cardStat.Atk; // 9 (기본 4 + 어빌리티1의 2 + 어빌리티2의 3)
```

## 4. 주의사항

1. 성능 고려사항
   - 스탯 접근마다 계산이 발생하므로 한 프레임에 과도한 접근 주의
   - 필요한 경우 캐싱 메커니즘 추가 고려

2. 확장성
   - 새로운 스탯 타입 추가 시 StatType 열거형에 추가
   - 새로운 어빌리티 효과 추가 시 IsStatModifyingAbility와 ApplyAbilityToStat 메서드 수정

3. 디버깅
   - 스탯 계산 과정 로깅 추가 고려
   - 어빌리티 효과 추적을 위한 디버그 정보 추가 고려