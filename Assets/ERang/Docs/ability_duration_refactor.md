# Ability Duration System Refactoring

## 목적
- 지속 시간이 있는 버프/디버프의 duration 관리 개선
- 효과 발동을 기준으로 지속시간 표기 및 감소 처리
- duration 관리 로직을 BaseAbility로 일원화

## 현재 문제점
1. 어빌리티의 duration이 턴 종료시에 일괄적으로 감소됨
2. 효과 발동 시점과 duration 감소 시점이 불일치
3. duration 관리 로직이 여러 곳에 분산되어 있음

## 수정 방안

### 1. BaseAbility 클래스 수정
```csharp
public abstract class BaseAbility : MonoBehaviour, IAbility
{
    public abstract AbilityType AbilityType { get; }

    protected virtual IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        // 각 어빌리티의 실제 효과 구현
        yield break;
    }

    // ApplySingle을 virtual로 선언하여 override 가능하게 함
    public virtual IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        // 1. 효과 적용
        yield return StartCoroutine(ApplyEffect(cardAbility, selfSlot, targetSlot));
        
        // 2. duration 감소
        cardAbility.DecreaseDuration(TurnManager.Instance.TurnCount);
        
        // 3. duration이 0이 되면 TurnManager에서 턴 종료시 일괄 해제함
        // (BaseAbility 내부에서 스스로 Release 하지 않음)
        
        // 4. 아이콘 갱신
        targetSlot.DrawAbilityIcons();
    }

    public abstract IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot);
}
```

### 2. BaseHandAbility 클래스 수정

#### 설계 목적
- 핸드 카드 전용 어빌리티를 위한 특수한 구현 제공
- 보드 슬롯 관련 메서드의 잘못된 사용 방지
- IHandAbility 인터페이스 구현을 통한 핸드 카드 전용 기능 제공

#### 핵심 변경사항
1. BaseAbility의 보드 슬롯 메서드를 sealed override로 막아서:
   - 자식 클래스에서 보드 슬롯 메서드를 잘못 override하는 것을 방지
   - 핸드 카드 전용 어빌리티가 보드 슬롯 메서드를 사용하지 못하도록 제한

2. IHandAbility 인터페이스 구현:
   - BaseCard를 파라미터로 받는 메서드 제공
   - 핸드 카드 전용 어빌리티의 인터페이스 통일

#### 구현 코드
```csharp
public abstract class BaseHandAbility : BaseAbility, IHandAbility
{
    // IHandAbility 구현
    public abstract IEnumerator ApplySingle(BaseCard card);
    public abstract IEnumerator Release(BaseCard card);

    // 보드 슬롯용 메서드는 sealed로 override 막음
    public sealed override IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        LogAbility("이 어빌리티는 핸드 카드 전용입니다.", LogType.Warning);
        yield break;
    }

    public sealed override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        LogAbility("이 어빌리티는 핸드 카드 전용입니다.", LogType.Warning);
        yield break;
    }
}
```

#### 사용 예시 (AbilityReducedMana)
```csharp
public class AbilityReducedMana : BaseHandAbility
{
    public override AbilityType AbilityType => AbilityType.ReducedMana;

    // BaseCard 버전의 ApplySingle 구현
    public override IEnumerator ApplySingle(BaseCard card)
    {
        if (!ValidateHandCard(card)) yield break;

        LogAbility($"마나 감소 적용 시작: {card.ToCardLogInfo()}");
        Apply(card, true);

        yield break;
    }

    // BaseCard 버전의 Release 구현
    public override IEnumerator Release(BaseCard card)
    {
        if (!ValidateHandCard(card)) yield break;

        LogAbility($"마나 감소 해제 시작: {card.ToCardLogInfo()}");
        Apply(card, false);

        yield break;
    }

    // 기존 BaseAbility의 메서드는 sealed로 막혀있어서 사용 불가
}
```

#### 주의사항
1. BaseHandAbility를 상속하는 클래스는 BaseCard 버전의 메서드만 구현해야 함
2. BaseAbility의 메서드(ApplySingle, Release)는 sealed로 막혀있어서 override 불가
3. 모든 핸드 카드 전용 어빌리티는 BaseHandAbility를 상속해야 함

### 3. 어빌리티 구현체 수정 예시 (AbilityAtkUp)
```csharp
public class AbilityAtkUp : BaseAbility
{
    public override AbilityType AbilityType => AbilityType.AtkUp;

    // ApplyEffect로 이름 변경
    protected override IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
    }

    // Release는 기존 구현 유지
    public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
    }

    // 나머지 코드는 동일...
}
```

### 4. 제거해야 할 코드

#### TurnManager.cs
```csharp
private IEnumerator ReleaseBoardCardAbility(List<BoardSlot> boardSlots)
{
    foreach (BoardSlot boardSlot in boardSlots)
    {
        if (boardSlot.Card == null)
            continue;

        BaseCard card = boardSlot.Card;
        List<CardAbility> removedCardAbilities = new();

        foreach (CardAbility cardAbility in card.AbilitySystem.CardAbilities)
        {
            cardAbility.DecreaseDuration();  // 제거
            
            if (cardAbility.duration == 0)
            {
                yield return StartCoroutine(AbilityLogic.Instance.AbilityRelease(cardAbility, AbilityWhereFrom.TurnEndBoardSlot));
                removedCardAbilities.Add(cardAbility);
            }
        }

        foreach (CardAbility cardAbility in removedCardAbilities)
        {
            card.AbilitySystem.RemoveCardAbility(cardAbility);
        }

        boardSlot.DrawAbilityIcons();
    }
}
```

#### CardAbilitySystem.cs
```csharp
// 이 메서드 전체 제거
public List<CardAbility> DecreaseDuration()
{
    List<CardAbility> removedCardAbilities = new();
    
    foreach (CardAbility cardAbility in cardAbilities)
    {
        cardAbility.DecreaseDuration();
        
        if (cardAbility.duration == 0)
            removedCardAbilities.Add(cardAbility);
    }
4. BaseHandAbility 구현시 sealed override로 보드 슬롯 메서드 막았는지 확인

## 2025-01-23 추가 수정 사항

### 스탯 버프 어빌리티 매턴 재적용 방식

#### 1. 수정 목적
- 스탯 버프(AtkUp, DefUp 등)도 다른 어빌리티처럼 매턴 효과 발동하도록 변경
- 효과 발동과 duration 감소를 동시에 처리
- 어빌리티 시스템 동작 방식 일원화

#### 2. 동작 방식
```
턴 시작시:
1. 카드 기본 스탯으로 초기화
2. 모든 살아있는 버프 순차 적용
3. 각 버프마다 duration 감소
4. duration = 0 된 버프는 제거
```

#### 3. 중첩 버프 처리
- 여러 개의 동일 스탯 버프가 있을 경우 처리
- AbilityItem Queue 활용하여 적용 순서 유지
- 예시:
  ```
  카드 기본 공격력: 5
  
  턴 1:
  - AtkUp(+2, 2턴) 적용 -> Atk 5 → 7
  - 다른 AtkUp(+3, 3턴) 적용 -> Atk 7 → 10
  
  턴 2: (두 버프 모두 재적용)
  - 기본값으로 복구 Atk 10 → 5
  - AtkUp(+2) 재적용 -> Atk 5 → 7
  - AtkUp(+3) 재적용 -> Atk 7 → 10
  - 각 버프 duration 감소
  
  턴 3:
  - 기본값으로 복구 Atk 10 → 5
  - 첫 번째 버프는 duration = 0이 되어 미적용
  - AtkUp(+3) 재적용 -> Atk 5 → 8
  - 두 번째 버프 duration 감소
  ```

#### 4. 구현시 고려사항
1. 스탯 초기화
   - 매 턴 시작시 카드의 기본 스탯으로 완전 초기화
   - 그 후 살아있는 모든 버프를 순차적으로 적용

2. 버프 적용 순서
   - CardAbility의 abilityItems Queue 순서대로 적용
   - 적용 순서 일관성 유지 필요

3. 성능 고려
   - 매턴 스탯 초기화/재계산의 성능 영향 검토
   - 불필요한 재계산 최소화 방안 고려

4. UI 업데이트
   - 스탯 변경시 UI 즉시 반영
   - 변경 애니메이션 처리 검토