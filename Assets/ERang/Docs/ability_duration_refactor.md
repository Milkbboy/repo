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
    // 기존 abstract ApplySingle을 protected virtual로 변경
    protected virtual IEnumerator ApplyEffect(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        // 각 어빌리티의 실제 효과 구현
        yield break;
    }

    // 새로운 public ApplySingle
    public IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
    {
        // 1. 효과 적용
        yield return StartCoroutine(ApplyEffect(cardAbility, selfSlot, targetSlot));
        
        // 2. duration 감소
        cardAbility.DecreaseDuration();
        
        // 3. duration이 0이 되었으면 release 처리
        if (cardAbility.duration <= 0)
        {
            // ability 해제
            yield return StartCoroutine(Release(cardAbility, selfSlot, targetSlot));
            
            // ability 제거
            if (targetSlot.Card != null)
            {
                targetSlot.Card.AbilitySystem.RemoveCardAbility(cardAbility);
            }
        }
        
        // 4. 아이콘 갱신
        targetSlot.DrawAbilityIcons();
    }

    public abstract IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot);
}
```

### 2. 어빌리티 구현체 수정 예시 (AbilityAtkUp)
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

### 3. 제거해야 할 코드

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
    
    return removedCardAbilities;
}
```

## 적용 후 동작 Flow

1. 어빌리티 발동
   - AbilityLogic.AbilityAction() 호출
   - BaseAbility.ApplySingle() 호출

2. ApplySingle() 내부 처리
   - ApplyEffect() 호출하여 실제 효과 적용
   - duration 감소
   - duration 0 체크 및 release 처리
   - UI 갱신

3. Duration 관리
   - 각 어빌리티가 실제로 발동될 때 duration 감소
   - duration이 0이 되면 자동으로 release 및 제거
   - UI는 실시간으로 갱신

## 기대 효과

1. 어빌리티 시스템 일관성 향상
   - 효과 발동과 duration 감소가 동시에 처리됨
   - duration 관리가 BaseAbility로 일원화됨

2. 코드 품질 향상
   - 중복 코드 제거
   - 책임 분리가 명확해짐
   - 유지보수성 향상

3. 버그 발생 가능성 감소
   - duration 관리 로직이 한 곳으로 통합됨
   - 자동 release 처리로 누락 가능성 제거

## 주의사항

1. 기존 duration 감소 로직을 모두 제거했는지 확인
2. 각 어빌리티의 ApplySingle을 ApplyEffect로 변경했는지 확인
3. UI 갱신이 적절한 시점에 이루어지는지 확인