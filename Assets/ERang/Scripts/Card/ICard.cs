using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 모든 카드 타입의 기본 인터페이스
    /// </summary>
    public interface ICard
    {
        // 기본 식별 및 속성
        string Uid { get; }
        int Id { get; }
        CardType CardType { get; }
        List<int> AiGroupIds { get; }
        Dictionary<int, int> AiGroupIndexes { get; set; } // AI 그룹 내 인덱스는 변경 가능
        bool InUse { get; }
        bool IsExtinction { get; }
        Texture2D CardImage { get; }

        // 카드 상태
        CardState State { get; }
        CardTraits Traits { get; }

        // 어빌리티 시스템 관련
        CardAbilitySystem AbilitySystem { get; }

        // 로그 정보
        string LogText { get; }

        // 가상 속성 - 자식 클래스에서 구현
        int Hp { get; }
        int Def { get; }
        int Mana { get; }
        int Atk { get; }

        // 카드 상태 변경 메서드
        void TakeDamage(int amount);
        void RestoreHealth(int amount);
        void SetDefense(int amount);
        void IncreaseDefense(int amount);
        void DecreaseDefense(int amount);

        // 카드 데이터 업데이트
        void UpdateCardData(CardData cardData);
    }

    /// <summary>
    /// 데미지를 받을 수 있는 카드를 위한 인터페이스
    /// </summary>
    public interface IDamageable
    {
        // public void Die();
        void TakeDamage(int amount);
        void RestoreHealth(int amount);
    }

    /// <summary>
    /// 방어력을 가진 카드를 위한 인터페이스
    /// </summary>
    public interface IDefensible
    {
        void IncreaseDefense(int amount);
        void DecreaseDefense(int amount);
    }

    /// <summary>
    /// 공격력을 가진 카드를 위한 인터페이스
    /// </summary>
    public interface IAttackable
    {
        void IncreaseAttack(int amount);
        void DecreaseAttack(int amount);
    }

    /// <summary>
    /// 마나를 관리할 수 있는 카드를 위한 인터페이스
    /// </summary>
    public interface IManaManageable
    {
        void IncreaseMana(int amount);
        void DecreaseMana(int amount);
    }
}