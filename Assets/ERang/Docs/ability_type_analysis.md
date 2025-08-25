# 어빌리티 타입 분석 및 매턴 적용 여부

[이전 내용 유지...]

## 6. 턴 처리 흐름

### 턴 시작 (StartTurn)
1. 마스터 선행 어빌리티 (PriorAbility) 실행
2. 마스터 스탯 설정 (HP, 마나)
3. 핸드 카드 생성
4. 핸드 카드의 HandOn 어빌리티 실행
5. 몬스터의 턴 시작 리액션 실행

### 턴 종료 (EndTurn)
1. 크리쳐 카드 BoardTurnEnd
2. 마스터 후행 어빌리티 (PostAbility)
3. 몬스터 카드 BoardTurnEnd
4. 건물 카드 BoardTurnEnd
5. 핸드 온 어빌리티 해제
6. duration이 0이 된 어빌리티 해제
7. 핸드덱 카드 제거
8. 마스터 마나 리셋
9. 턴 카운트 증가

### BoardTurnEnd 처리 순서
각 BoardSlot에 대해 순차적으로:
1. 선행 어빌리티 (PriorAbility) 실행
2. AI 액션 실행
3. 후행 어빌리티 (PostAbility) 실행

### 어빌리티 적용 시점
- 선행 어빌리티 (PriorAbility)
  - 턴 시작 시 마스터
  - BoardTurnEnd 시 각 카드
- 후행 어빌리티 (PostAbility)
  - 턴 종료 시 마스터
  - BoardTurnEnd 시 각 카드
- HandOn 어빌리티
  - 턴 시작 시 적용
  - 턴 종료 시 해제
- 지속 어빌리티
  - duration이 0이 되면 턴 종료 시 해제

### 스탯 계산 적용 시점
스탯 계산이 필요한 시점:
1. 선행 어빌리티 실행 후
2. AI 액션 실행 전
3. 후행 어빌리티 실행 후
4. 어빌리티 해제 시
5. 스탯 참조 시 (getter 호출 시)

이러한 처리 흐름에서 어빌리티의 적용과 해제가 명확한 시점에 이루어지므로, 스탯 계산도 이에 맞춰 실시간으로 처리되어야 합니다.