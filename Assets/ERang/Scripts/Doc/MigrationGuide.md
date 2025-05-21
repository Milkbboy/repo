# ERang 카드 시스템 리팩토링 마이그레이션 가이드

## 개요
이 문서는 ERang 카드 시스템의 리팩토링 과정에서 기존 코드를 새로운 시스템으로 마이그레이션하는 방법을 설명합니다.

## 주요 변경사항

### 1. 기본 클래스 변경
- **이전**: `BaseCard` 추상 클래스
- **현재**: `GameCard` 통합 클래스

### 2. 새로운 인터페이스 도입
- `ICard`: 모든 카드의 기본 인터페이스
- `ICombatAbilityCard`: 전투 및 어빌리티 기능
- `IValueCard`: 값 관리 기능
- `IAiCard`: AI 기능
- `ITargetingCard`: 타겟팅 기능

### 3. 통합 값 시스템
- 모든 카드 속성(HP, 마나, 골드 등)을 `ValueType` 열거형으로 통합
- `GetValue()`, `SetValue()`, `ModifyValue()` 메서드로 일관된 접근

## 마이그레이션 단계

### 단계 1: 카드 생성 방식 변경

**이전 방식:**
```csharp
var card = new CreatureCard(cardData);
```

**새로운 방식:**
```csharp
var card = CardFactory.CreateCard(cardData);
// 또는 특정 타입으로
var creatureCard = new CreatureCard(cardData);
```

### 단계 2: UI 컴포넌트 업데이트

**HCard 클래스:**
```csharp
// 이전
public void SetCard(BaseCard card)

// 현재
public void SetCard(GameCard card)
```

**CardUI 클래스:**
- `GameCard` 매개변수를 받는 `SetCard` 메서드 추가
- 이전 버전 호환을 위한 `BaseCard` 매개변수 메서드 유지 (임시)

### 단계 3: 값 접근 방식 변경

**이전 방식:**
```csharp
int hp = card.State.Hp;
card.State.SetHp(newHp);
```

**새로운 방식:**
```csharp
int hp = card.GetValue(ValueType.Hp);
card.SetValue(ValueType.Hp, newHp);
// 또는
card.ModifyValue(ValueType.Hp, deltaHp);
```

### 단계 4: 기능별 활성화 확인

각 카드 타입별로 사용하는 기능:

- **CreatureCard**: 모든 기능 (combat, abilities, values, ai)
- **MasterCard**: 모든 기능
- **MagicCard**: combat, abilities, ai (values 비활성화)
- **GoldCard**: values만 활성화
- **BuildingCard**: abilities, values, ai

## 호환성 관리

### 1. 점진적 마이그레이션
기존 `BaseCard`를 사용하는 코드와 새로운 `GameCard`를 사용하는 코드가 공존할 수 있도록 설계되었습니다.

### 2. CardUI 호환성
`CardUI` 클래스는 두 가지 `SetCard` 메서드를 제공합니다:
- `SetCard(GameCard card)`: 새로운 방식
- `SetCard(BaseCard card)`: 이전 방식 (내부적으로 변환)

### 3. 변환 유틸리티
`ConvertToGameCard` 메서드가 기존 `BaseCard` 인스턴스를 `GameCard`로 변환합니다.

## 테스트 방법

### 1. 단위 테스트
`CardSystemTest` 클래스를 사용하여 새로운 시스템을 테스트할 수 있습니다:

```csharp
// 테스트 씬에 CardSystemTest 컴포넌트 추가
// Start() 메서드에서 자동으로 테스트 실행
```

### 2. 통합 테스트
- 기존 게임 플레이 시나리오 실행
- 카드 생성, 배치, 전투 테스트
- UI 상호작용 테스트

## 성능 최적화

### 1. 선택적 기능 활성화
각 카드는 필요한 기능만 활성화하여 메모리와 성능을 최적화합니다:

```csharp
// GoldCard 예시
public GoldCard() : base()
{
    usesCombat = false;     // 전투 기능 비활성화
    usesAbilities = false;  // 어빌리티 기능 비활성화
    usesValues = true;      // 값 관리만 활성화
    usesAi = false;         // AI 기능 비활성화
}
```

### 2. 중복 코드 제거
- HP, 마나 등의 상태 관리 코드가 `GameCard`에 통합되어 중복 제거
- 공통 UI 로직이 `CardView` 기본 클래스에 통합

## 주의사항 및 알려진 이슈

### 1. 기능 플래그 확인
새로운 시스템에서는 각 메서드가 해당 기능 플래그를 확인합니다:

```csharp
public virtual void TakeDamage(int amount)
{
    if (!usesCombat) return; // 전투 기능이 비활성화되면 실행하지 않음
    
    State.TakeDamage(amount);
    // ...
}
```

### 2. 값 시스템과 CardState 동기화
값 시스템을 사용하는 카드의 경우 `ValueType`과 `CardState`가 동기화됩니다.

### 3. 임시 변환 코드
현재 `CardUI.ConvertToGameCard` 메서드는 임시 변환을 위한 것입니다. 모든 코드가 `GameCard`를 사용하도록 변경된 후에는 제거해야 합니다.

## 향후 개선 계획

### 1. 완전한 마이그레이션
- 모든 `BaseCard` 참조를 `GameCard`로 변경
- 임시 변환 코드 제거
- 이전 카드 클래스들 제거

### 2. 추가 기능
- 카드 이벤트 시스템 강화
- 더 세밀한 기능 플래그 제어
- 카드 상태 저장/로드 시스템

### 3. 성능 최적화
- 메모리 풀링 시스템 도입
- 불필요한 계산 최소화
- UI 업데이트 최적화

## 결론

이 리팩토링을 통해 ERang 카드 시스템은 다음과 같은 이점을 얻었습니다:

1. **코드 중복 제거**: 공통 기능이 통합됨
2. **선택적 기능 활성화**: 각 카드 타입에 필요한 기능만 사용
3. **일관된 값 관리**: 모든 속성을 동일한 방식으로 관리
4. **확장성 향상**: 새로운 카드 타입 추가가 용이
5. **유지보수성 개선**: 코드 구조가 명확해짐

점진적 마이그레이션을 통해 기존 코드와의 호환성을 유지하면서 새로운 시스템으로 전환할 수 있습니다.