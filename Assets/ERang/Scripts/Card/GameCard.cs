using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 모든 카드 타입의 기본 기능을 구현하는 통합 카드 클래스
    /// </summary>
    [Serializable]
    public abstract class GameCard : ICard, ICombatAbilityCard, IAiCard
    {
        // 기능 활성화 플래그
        protected bool usesCombat = true;
        protected bool usesAbilities = true;
        protected bool usesAi = true;

        // 기본 속성
        private string uid;
        private int id;
        private string name;
        private CardType cardType;
        private int aiGroupId;
        private bool inUse;
        private bool isExtinction;
        private Texture2D cardImage;

        public string Uid { get => uid; protected set => uid = value; }
        public int Id { get => id; protected set => id = value; }
        public string Name { get => name; protected set => name = value; }
        public CardType CardType { get => cardType; protected set => cardType = value; }
        public int AiGroupId { get => usesAi ? aiGroupId : 0; protected set => aiGroupId = value; }
        public int AiGroupIndex { get; set; }
        public bool InUse { get => inUse; protected set => inUse = value; }
        public bool IsExtinction { get => isExtinction; protected set => isExtinction = value; }
        public Texture2D CardImage { get => cardImage; protected set => cardImage = value; }

        // 게임 관련 멤버 변수
        public CardState State { get; protected set; }
        public CardAbilitySystem AbilitySystem { get; protected set; }
        public CardTraits Traits { get; protected set; }

        public string LogText => Utils.CardLog(this);

        // 생성자
        protected GameCard()
        {
            Uid = Utils.GenerateShortUniqueID();
            State = new CardState(0, 0, 0, 0);
            State.SetCardName(Name); // CardState에 카드 이름 설정

            if (usesAbilities)
            {
                AbilitySystem = new CardAbilitySystem();
            }

            Traits = CardTraits.None;
        }

        // CardData 기반 생성자
        protected GameCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            Name = cardData.nameDesc;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            AiGroupIndex = 0;
            CardImage = cardData.GetCardTexture();

            State = new CardState(cardData.hp, cardData.def, cardData.costMana, cardData.atk);
            State.SetCardName(Name); // CardState에 카드 이름 설정

            if (usesAbilities)
            {
                AbilitySystem = new CardAbilitySystem();
            }

            Traits = CardTraits.None;
        }

        #region 기본 값 접근 메서드 (모든 카드가 사용 가능)
        public void SetTraits(CardTraits traits)
        {
            Traits = traits;
        }

        public void SetCardType(CardType cardType)
        {
            CardType = cardType;
        }

        public void SetAiGroupId(int aiGroupId)
        {
            AiGroupId = aiGroupId;
        }

        // CardData 업데이트
        public void UpdateCardData(CardData cardData)
        {
            Id = cardData.card_id;
            Name = cardData.nameDesc;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            CardImage = cardData.GetCardTexture();
            State.SetCardName(Name); // CardState에 카드 이름 설정
        }

        #endregion

        #region ICombatAbilityCard 구현

        public virtual void AddCardAbility(CardAbility cardAbility, int turnCount, AbilityWhereFrom whereFrom)
        {
            if (!usesAbilities) return;

            AbilityItem abilityItem = new()
            {
                whereFrom = whereFrom,
                applyTurn = turnCount,
                value = cardAbility.abilityValue,
                duration = cardAbility.duration,
                createdDt = DateTime.UtcNow.Ticks
            };

            CardAbility found = AbilitySystem.CardAbilities.Find(ability => ability.abilityId == cardAbility.abilityId);

            if (found == null)
            {
                if (cardAbility.abilityType == AbilityType.ArmorBreak)
                    AbilitySystem.CardAbilities.Insert(0, cardAbility);
                else
                    AbilitySystem.CardAbilities.Add(cardAbility);
            }

            cardAbility.AddAbilityItem(abilityItem);

            Debug.Log($"{cardAbility.LogText} {(found == null ? "신규" : "AbilityItem 만")} 추가. value: {cardAbility.abilityValue}, duration: {cardAbility.duration}, workType: {cardAbility.workType}");

            // ApplyAbilityToValues(cardAbility);
        }

        protected virtual void ApplyAbilityToValues(CardAbility cardAbility)
        {
            GameLogger.LogAbilityDetail($"{Name}에 {cardAbility.nameDesc} 어빌리티 적용 시작");

            // 어빌리티 효과를 값 시스템에 적용
            // 전투 관련 어빌리티는 usesCombat 체크
            switch (cardAbility.abilityType)
            {
                case AbilityType.Damage:
                    if (usesCombat) TakeDamage(cardAbility.abilityValue);
                    break;
                case AbilityType.Heal:
                    if (usesCombat) RestoreHealth(cardAbility.abilityValue);
                    break;
                case AbilityType.AtkUp:
                    if (usesCombat) IncreaseAttack(cardAbility.abilityValue);
                    break;
                case AbilityType.DefUp:
                    if (usesCombat) IncreaseDefense(cardAbility.abilityValue);
                    break;
                case AbilityType.BrokenDef:
                    if (usesCombat) DecreaseDefense(cardAbility.abilityValue);
                    break;
                case AbilityType.AddGoldPer:
                    // 골드는 IGoldCard 인터페이스를 구현한 카드만 처리
                    if (this is IGoldCard goldCard)
                    {
                        goldCard.AddGold(cardAbility.abilityValue);
                        GameLogger.LogCardState(Name, "골드", 0, cardAbility.abilityValue, "골드 획득");
                    }
                    break;
                default:
                    GameLogger.LogAbilityDetail($"{cardAbility.abilityType} 어빌리티는 값 시스템에 직접 적용되지 않음");
                    break;
            }
        }

        // 전투 관련 메서드들 (useeCombat 체크)
        public void SetHp(int hp)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 체력을 설정할 수 없습니다.");
                return;
            }

            State.SetHp(hp);
        }

        public void SetDefense(int defense)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 방어력을 설정할 수 없습니다.");
                return;
            }

            State.SetDef(defense);
        }

        public void SetMana(int mana)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 마나를 설정할 수 없습니다.");
                return;
            }

            State.SetMana(mana);
        }

        public void SetAttack(int attack)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 공격력을 설정할 수 없습니다.");
                return;
            }

            State.SetAtk(attack);
        }

        public virtual void TakeDamage(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 데미지를 입을 수 없습니다.");
                return;
            }

            GameLogger.LogAbilityDetail($"{Name} 데미지 처리 시작: {amount}");
            State.TakeDamage(amount, "데미지 받음");
        }

        public virtual void RestoreHealth(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 체력을 회복할 수 없습니다.");
                return;
            }

            GameLogger.LogAbilityDetail($"{Name} 체력 회복 시작: {amount}");
            State.RestoreHealth(amount, "체력 회복");
        }

        public virtual void IncreaseDefense(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 방어력을 증가할 수 없습니다.");
                return;
            }

            State.IncreaseDefense(amount);
        }

        public virtual void DecreaseDefense(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 방어력을 감소할 수 없습니다.");
                return;
            }

            State.DecreaseDefense(amount, "버프");
        }

        public virtual void IncreaseAttack(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 공격력을 증가할 수 없습니다.");
                return;
            }

            State.IncreaseAttack(amount, "버프");
        }

        public virtual void DecreaseAttack(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 공격력을 감소할 수 없습니다.");
                return;
            }

            State.DecreaseAttack(amount, "디버프");
        }

        public virtual void IncreaseMana(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 마나를 증가할 수 없습니다.");
                return;
            }

            State.IncreaseMana(amount, "마나 증가");
        }

        public virtual void DecreaseMana(int amount)
        {
            if (!usesCombat)
            {
                Debug.LogWarning($"카드({Id})는 전투 카드가 아니므로 마나를 감소할 수 없습니다.");
                return;
            }

            State.DecreaseMana(amount, "마나 소모");
        }

        // 어빌리티 관련 메서드들 (usesAbilities 체크)
        public virtual void AddHandCardAbility(CardAbility cardAbility)
        {
            if (!usesAbilities)
            {
                Debug.LogWarning($"카드({Id})는 어빌리티 카드가 아니므로 핸드 카드 어빌리티를 추가할 수 없습니다.");
                return;
            }

            AbilitySystem.AddHandCardAbility(cardAbility);
        }

        public virtual List<CardAbility> DecreaseDuration()
        {
            if (!usesAbilities)
            {
                Debug.LogWarning($"카드({Id})는 어빌리티 카드가 아니므로 지속 시간을 감소할 수 없습니다.");
                return new List<CardAbility>();
            }

            return AbilitySystem.DecreaseDuration();
        }

        public virtual void RemoveCardAbility(CardAbility cardAbility)
        {
            if (!usesAbilities)
            {
                Debug.LogWarning($"카드({Id})는 어빌리티 카드가 아니므로 어빌리티를 제거할 수 없습니다.");
                return;
            }

            AbilitySystem.RemoveCardAbility(cardAbility);
        }

        public virtual void RemoveHandCardAbility(CardAbility cardAbility)
        {
            if (!usesAbilities)
            {
                Debug.LogWarning($"카드({Id})는 어빌리티 카드가 아니므로 핸드 카드 어빌리티를 제거할 수 없습니다.");
                return;
            }

            AbilitySystem.RemoveHandCardAbility(cardAbility);
        }

        public virtual int GetBuffCount()
        {
            if (!usesAbilities)
            {
                Debug.LogWarning($"카드({Id})는 어빌리티 카드가 아니므로 버프 개수를 조회할 수 없습니다.");
                return 0;
            }

            return AbilitySystem.GetBuffCount();
        }

        public virtual int GetDeBuffCount()
        {
            if (!usesAbilities)
            {
                Debug.LogWarning($"카드({Id})는 어빌리티 카드가 아니므로 디버프 개수를 조회할 수 없습니다.");
                return 0;
            }

            return AbilitySystem.GetDeBuffCount();
        }

        #endregion

        #region 라이프사이클 메서드

        public virtual void OnPlay() { }
        public virtual void OnTurnStart() { }
        public virtual void OnTurnEnd() { }
        public virtual void OnDiscard() { }

        #endregion
    }
}