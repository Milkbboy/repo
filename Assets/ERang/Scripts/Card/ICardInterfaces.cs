using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    // 값 타입 열거형
    public enum ValueType
    {
        Hp,
        MaxHp,
        Mana,
        MaxMana,
        Attack,
        Defense,
        Gold
    }

    /// <summary>
    /// 모든 카드의 기본 인터페이스
    /// </summary>
    public interface ICard
    {
        string Uid { get; }
        int Id { get; }
        CardType CardType { get; }
        bool InUse { get; }
        Texture2D CardImage { get; }
        CardState State { get; }
        
        void OnPlay();
        void OnTurnStart();
        void OnTurnEnd();
        void OnDiscard();
        void UpdateCardData(CardData cardData);
    }

    /// <summary>
    /// 전투 및 어빌리티 통합 인터페이스
    /// </summary>
    public interface ICombatAbilityCard : ICard
    {
        CardAbilitySystem AbilitySystem { get; }
        
        void AddCardAbility(CardAbility cardAbility, int turnCount, AbilityWhereFrom whereFrom);
        void RemoveCardAbility(CardAbility cardAbility);
        List<CardAbility> DecreaseDuration();
        
        void TakeDamage(int amount);
        void RestoreHealth(int amount);
        void IncreaseDefense(int amount);
        void DecreaseDefense(int amount);
        
        int GetBuffCount();
        int GetDeBuffCount();
    }

    /// <summary>
    /// 값 관리 인터페이스
    /// </summary>
    public interface IValueCard : ICard
    {
        int GetValue(ValueType valueType);
        void SetValue(ValueType valueType, int value);
        void ModifyValue(ValueType valueType, int delta);
    }

    /// <summary>
    /// AI 기능을 가진 카드를 위한 인터페이스
    /// </summary>
    public interface IAiCard : ICard
    {
        int AiGroupId { get; }
        int AiGroupIndex { get; set; }
    }

    /// <summary>
    /// 타겟팅이 필요한 카드를 위한 인터페이스
    /// </summary>
    public interface ITargetingCard : ICard
    {
        List<CardType> ValidTargets { get; }
        bool RequiresTarget { get; }
        bool CanTargetSelf { get; }
        bool CanTargetEmpty { get; }
        
        bool IsValidTarget(ICard target);
        void OnTargetSelected(ICard target);
    }
}