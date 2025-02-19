namespace ERang
{
    public enum DeckKind
    {
        Deck,
        Hand,
        Grave
    }

    public enum MasterType
    {
        None = 0,
        Luci = 1001,
        CrawlSeul = 1002,
        BarakRahum = 1003,
    }

    public enum StatType
    {
        None = 0,
        Hp,
        Atk,
        Def,
        Mana,
        Gold,
    }

    public enum CardGrade
    {
        None = 0,
        Common,
        Rare,
        Legendary,
    }

    public enum CardType
    {
        None = 0,
        Master,
        Magic, // 마법 카드로 사용 시 해당 카드가 가진 어빌리티를 호출한다. (Ex : 적 공격, 아군 체력 회복, 적 상태 이상 등)
        Individuality, // 마법 카드와 동일한 기능을 하나, 특정 캐릭터만 사용 가능한 전용 마법
        Creature, // 필드에 배치될 크리쳐 카드로 기본적으로 체력, 공격력, 방어력이 존재한다.
        Building, // 건물 카드로 마나와 골드를 소비하여 건물 필드에 배치할 수 있다.
        Charm, // 축복 카드로 해당 카드가 핸드에 들어오면 지정된 어빌리티를 발동시킨다.
        Curse, // 저주 카드로 해당 카드가 핸드에 들어오면 지정된 어빌리티를 발동시킨다.
        Monster, // 적군의 크리쳐 카드로 기본적으로 체력, 공격력, 방어력이 존재한다.
        EnemyMaster, // 적군의 마스터 카드로 기본적으로 체력, 공격력, 방어력이 존재한다.
    }

    public enum CardTraits
    {
        None = 0,
        NextTurnSelect = 1 << 0, // 다음 턴에 선택 가능한 카드
        // RandomSelect = 1 << 1, // 무작위로 선택되는 카드
    }

    // 행동 패턴의 진행 방식 지정으로 초안에서는 2개의 타입이 존재한다.
    public enum AiGroupType
    {
        None = 0,
        Repeat, // 순차적으로 AI 그룹을 호출하고, 마지막 그룹에 도달 시 최초 그룹으로 순환한다.
        Random, // 나열된 AI 그룹 중 하나를 임의로 선택한다. (선택 확율은 별도 지정 없이 동일한다. n/1)
    }

    // 행동의 타입을 정의하며 초안으로는 총 3가지 타입이 존재함.
    public enum AiDataType
    {
        None = 0,
        Melee, // 근거리 행동으로 Melee로 설정된 경우 행동 시 가장 가까운 적이 배치된 필드로 이동한다.
        Ranged, // 원거리 행동으로 Ranged로 설정된 경우 제자리에서 행동한다.
        Explosion, // 폭발 공격으로 Explosion로 설정된 경우 제자리에서 행동한다.
        Buff, // 이로운 효과를 주는 버프 행동으로 제자리에서 행동한다. (Ranged와 동일하나 데이터 가독성을 위해 분리)
        Debuff, // 해로운 효과를 주는 디버프 행동으로 제자리에서 행동한다. (Ranged와 동일하나 데이터 가독성을 위해 분리)
        Chain, // 복수의 어빌리티를 순차적으로 실행하는 타입
    }

    public enum Target
    {
        None = 0,
        NearEnemy, // 가장 가까운 적을 대상으로 설정한다.
        Enemy, // 적을 대상으로 설정한다.
        RandomEnemy, // 적 보스를 포함한 모든 적 중 임의의 대상을 선정한다.
        RandomEnemyCreature, // 적 보스를 제외한 모든 적을 임의의 대상으로 선정한다.
        AllEnemy, // 적 보스를 포함한 모든 적을 대상으로 한다
        AllEnemyCreature, // 적 보스를 제외한 모든 적을 대상으로 한다.
        Friendly, // 아군을 대상으로 설정한다.
        AllFriendly, // 마왕을 포함한 아군을 대상으로 한다.
        AllFriendlyCreature, // 마왕을 제외한 아군을 대상으로 한다.
        Self, // 자기 자신을 대상으로 설정한다.
        Enemy1, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        Enemy2, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        Enemy3, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        Enemy4, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        FriendlyCreature, // 아군 크리쳐 모두를 대상으로
        EnemyCreature, // 적 크리쳐 모두를 대상으로
        Card, // 카드를 대상으로
    }

    // 대상 혹은 복수 대상을 설정한다.
    public enum AiDataTarget
    {
        None = 0,
        NearEnemy, // 가장 가까운 적을 대상으로 설정한다.
        Enemy, // 적을 대상으로 설정한다.
        RandomEnemy, // 적 보스를 포함한 모든 적 중 임의의 대상을 선정한다.
        RandomEnemyCreature, // 적 보스를 제외한 모든 적을 임의의 대상으로 선정한다.
        AllEnemy, // 적 보스를 포함한 모든 적을 대상으로 한다
        AllEnemyCreature, // 적 보스를 제외한 모든 적을 대상으로 한다.
        Friendly, // 아군을 대상으로 설정한다.
        AllFriendly, // 마왕을 포함한 아군을 대상으로 한다.
        AllFriendlyCreature, // 마왕을 제외한 아군을 대상으로 한다.
        Self, // 자기 자신을 대상으로 설정한다.
        FirstEnemy, // 가장 앞에 있는 적을 공격한다. (Atk_Range 무시)
        SecondEnemy, // 첫번째 적을 건너뛰고 뒤에 있는 적을 공격한다. (Atk_Range 무시) 해당 타겟이 없다면, 첫번째 적을 타겟으로 한다.
        SelectEnemy, // 적 보스를 포함한 적군 중 하나를 대상으로 지정한다 (지정 UI 출력)
    }

    // 행동이 이루어지는 절차를 설정한다.
    public enum AiDataAttackType
    {
        None = 0,
        Automatic, // 자동으로 대상이 지정된다.
        SelectEnemy, // 적 보스를 포함한 적군 중 하나를 대상으로 지정한다 (지정 UI 출력)
        SelectFriendly, // 마왕을 포함한 아군 중 하나를 대상으로 지정한다 (지정 UI 출력)
        SelectEnemyCreature, // 적 보스를 제외한 적군 중 하나를 대상으로 지정한다 (지정 UI 출력)
        SelectFriendlyCreature, // 마왕을 제외한 아군 중 하나를 대상으로 지정한다 (지정 UI 출력)
    }

    /// <summary>
    /// 어디서 발동된 어빌리티인지 확인
    /// </summary>
    public enum AbilityWhereFrom
    {
        None = 0,
        BoardSlot, // 필드에 있는 카드가 발동
        TurnStarHandOn, // 턴 시작 핸드 온 액션
        TurnStartAction, // 턴시작 액션 발동
        TurnEndHandOn, // 턴 종료 핸드 온 액션
        TurnEndBoardSlot, // 턴 종료 필드 슬롯 액션
        TurnEndBuilding, // 턴 종료 건물 액션
        HandUse, // 핸드에 있는 카드가 발동
        OnStage, // 필드에 있는 카드가 발동
        EditorWindow, // 에디터에서 추가
        Test, // 테스트
    }

    /// <summary>
    /// 실질적인 효과를 스킬의 효과를 타입
    /// </summary>
    public enum AbilityType
    {
        None = 0,
        Damage, // 대상에게 Value 만큼의 데미지를 준다.
        Heal, // 대상의 체력을 Value 만큼 회복한다.
        AtkUp, // 대상의 공격력을 Value 만큼 Duration 턴 동안 상승시킨다.
        DefUp, // 대상의 방어력을 Value 만큼 Duration 턴 동안 상승시킨다.
        BrokenDef, // 대상의 방어력을 Value 만큼 Duration 턴 동안 감소시킨다.
        ChargeDamage, // Duration 만큼 공격을 충천 후, 대상에게 Value 만큼의 데미지를 준다.
        AddGoldPer, // Ratio에 지정된 퍼센트 만큼 획득하는 골드량이 증가한다.
        AddMana, // 대상의 마나를 Value 만큼 Duration 턴 동안 증가시킨다.
        SubMana, // 대상의 마나를 Value 만큼 Duration 턴 동안 감소시킨다.
        AddGold, // Value 만큼 골드를 획득한다.
        AddSatiety, // Value 만큼 포만감을 채운다.
        SubSatiety, // Value 만큼 포만감을 소모한다.
        SummonHand, // 핸드로 카드를 소환한다.
        SummonDrawDeck, // 뽑을 카드 덱으로 카드를 소환한다.
        SummonGraveDeck, // 무덤 덱으로 카드를 소환한다.
        Weaken, // 약화 상태로 N턴 동안 공격력 N이 감소된다.
        ArmorBreak, // 방어구 파괴 상태로 N턴 동안 방어력이 사라진다. 방어력을 감소 시키는 다른 상태 이상을 무시하며 방어구 파괴 상태일 시, 방어력은 0으로 고정된다.
        Doom, // 파멸 상태로 N턴 이후 캐릭터는 사망한다.
        Burn, // 화상 상태로 N턴 동안 매 행동 시작 시 N만큼의 피해를 받는다.
        Poison, // 중독 상태로 N턴 동안 매 행동 종료 시 N만큼의 피해를 받는다.
        Swallow, // 카드 한 장을 선택하여 삼킨다. 삼킨 카드는 다음 턴에 다시 핸드로 돌아온다.
        ReducedMana, // 마나 소모량을 Value 만큼 감소시킨다.
    }

    /// <summary>
    /// 어빌리티의 적용 방식을 타입으로 지정
    /// </summary>
    public enum AbilityWorkType
    {
        None = 0,
        Active, // 어빌리티가 호출되는 시점에 1회 발동
        Passive, // 세션이 시작 후 세션이 종료되기 전까지 지속
        OnHand, // 핸드에 카드가 있을 때만 효과 지속
        OnStage, // 이번 전투에 한하여 해당 효과 지속
    }

    /// <summary>
    /// 체크할 대상을 선정함
    /// </summary>
    public enum ConditionTarget
    {
        None = 0,
        NearEnemy, // 가장 가까운 적
        Self, // 자기 자신
        Enemy1, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        Enemy2, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        Enemy3, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        Enemy4, // : 적 진형을 설정 (1: 1열, 2: 2열, 3: 3열, 4: 적 마스터)
        FriendlyCreature, // 아군 크리쳐 모두를 대상으로
        EnemyCreature, // 적 크리쳐 모두를 대상으로
        Card, // 카드를 대상으로
    }

    /// <summary>
    /// 조건 타입 입력
    /// </summary>
    public enum ConditionType
    {
        None = 0,
        Buff, // 대상의 버프 상태 확인
        Debuff, // 대상의 디버프 상태 확인
        Hp, // 대상의 체력 상태 확인
        EveryTurn, // 대상의 모든 턴
        Extinction, // 대상의 소멸 상태 확인
        Acquisition, // 대상이 추가 될 때
    }

    /// <summary>
    /// 조건이 발동되는 시점
    /// </summary>
    public enum ConditionCheckPoint
    {
        None = 0,
        TurnStart, // 턴이 시작 될 때 발동
        TurnEnd, // 턴이 종료 될 때 발동
        Immediately, // 해당 조건이 달성되는 즉시
    }

    /// <summary>
    /// 해당 조건의 지속 방식
    /// </summary>
    public enum ConditionMethod
    {
        None = 0,
        Always, // 조건이 발동 된 이후에도 계속 해당 조건을 다시 체크
        StageOneTime, // 스테이지 한 판에 한번만 발동
        GameOneTime, // 게임 전체에 한번만 발동
    }

    /// <summary>
    /// 맵 이벤트 타입
    /// </summary>
    public enum EventType
    {
        None = 0,
        Store,  // 상점
        EliteBattle, // 앨리트 보스
        RandomEvent, // 랜덤 이벤트
        BossBattle, // 보스
    }

    /// <summary>
    /// 맵 RandomEvent 구분
    /// </summary>
    public enum RandomEventType
    {
        None = 0,
        RandomBattle,
        RandomBattleFix,
        Roulette,
        Matching,
        DecreaseHp,
        GetRelics,
        GetCards,
    }

    /// <summary>
    /// 대상 선택 공격 타입
    /// </summary>
    public static class Constants
    {
        public static readonly AiDataAttackType[] SelectAttackTypes = new[]
        {
            AiDataAttackType.SelectEnemy,
            AiDataAttackType.SelectEnemyCreature,
            AiDataAttackType.SelectFriendly,
            AiDataAttackType.SelectFriendlyCreature,
        };

        public static readonly int BoardSlotCount = 10;

        public static readonly int[] MySlotNumbers = new[] { 0, 1, 2, 3 };
        public static readonly int[] EnemySlotNumbers = new[] { 6, 7, 8, 9 };

        public static readonly int RewardCardCount = 3;

        // 행동 전 효과 발생 어빌리티
        public static readonly AbilityType[] CardPriorAbilities = new[]
        {
            AbilityType.Burn,
        };

        // 행동 후 효과 발생 어빌리티
        public static readonly AbilityType[] CardPostAbilities = new[]
        {
            AbilityType.Poison,
        };
    }
}