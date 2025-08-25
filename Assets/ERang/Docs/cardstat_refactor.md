# CardStat 클래스 리팩토링 (2025-08-25)

## 주요 변경사항

### 1. 스탯 계산 로직 변경 (CalculateStat)
- 기본값 → 어빌리티 적용 → 한계값 적용 순서로 처리
- ArmorBreak 우선 처리 추가:
  ```csharp
  if (statType == StatType.Def && statModifyingAbilities.Any(a => a.abilityType == AbilityType.ArmorBreak))
  {
      return 0; // ArmorBreak가 있으면 다른 모든 효과를 무시하고 0 반환
  }
  ```
- 같은 ability_id를 가진 어빌리티는 가장 최근 값만 사용
- 다른 ability_id를 가진 같은 타입 어빌리티는 효과 합산

### 2. 어빌리티 제외 계산 로직 (CalculateStatWithoutAbility)
- 특정 어빌리티를 제외한 스탯 계산
- ArmorBreak 처리 추가:
  ```csharp
  var statModifyingAbilities = GetStatModifyingAbilities(statType)
      .Where(a => a != excludeAbility);

  if (statType == StatType.Def && statModifyingAbilities.Any(a => a.abilityType == AbilityType.ArmorBreak))
  {
      return 0;
  }
  ```

### 3. 어빌리티 효과 우선순위
1. ArmorBreak (방어력을 0으로 만들고 다른 모든 효과 무시)
2. 기본 스탯 적용
3. 나머지 어빌리티들 적용 (ability_id 기준 그룹화)
4. 한계값 적용 (음수 방지, HP/마나 최대값)

### 4. 스탯 수정 효과 적용 규칙
- AtkUp: 현재값 + value
- Weaken: 현재값 - value
- DefUp: 현재값 + value
- BrokenDef: 현재값 - value
- ArmorBreak: 즉시 0 반환 (다른 효과 무시)

## 구현 원칙
1. 모든 스탯은 실시간 계산 (getter에서 계산)
2. 기본 스탯과 계산된 스탯 명확히 구분
3. 같은 ability_id의 value는 중첩되지 않음
4. 다른 ability_id의 value는 합산됨
5. ArmorBreak는 모든 방어력 효과를 무시하는 특수 케이스로 처리