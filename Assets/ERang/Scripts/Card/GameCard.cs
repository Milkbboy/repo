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
    public abstract class GameCard : ICard, ICombatAbilityCard, IValueCard, IAiCard
    {
        // 기능 활성화 플래그
        protected bool usesCombat = true;
        protected bool usesAbilities = true;
        protected bool usesValues = true;
        protected bool usesAi = true;
        
        // 기본 속성
        private string uid;
        private int id;
        private CardType cardType;
        private int aiGroupId;
        private bool inUse;
        private bool isExtinction;
        private Texture2D cardImage;
        
        public string Uid { get => uid; protected set => uid = value; }
        public int Id { get => id; protected set => id = value; }
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
        
        // 값 저장소
        protected Dictionary<ValueType, int> values = new Dictionary<ValueType, int>();
        
        public string LogText => Utils.CardLog(this);
        
        // 생성자
        protected GameCard()
        {
            Uid = Utils.GenerateShortUniqueID();
            State = new CardState(0, 0, 0, 0);
            
            if (usesAbilities)
            {
                AbilitySystem = new CardAbilitySystem();
            }
            
            if (usesValues)
            {
                InitializeValues();
            }
            
            Traits = CardTraits.None;
        }
        
        // CardData 기반 생성자
        protected GameCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            AiGroupIndex = 0;
            CardImage = cardData.GetCardTexture();
            
            State = new CardState(cardData.hp, cardData.def, cardData.costMana, cardData.atk);
            
            if (usesAbilities)
            {
                AbilitySystem = new CardAbilitySystem();
            }
            
            if (usesValues)
            {
                InitializeValues(cardData);
            }
            
            Traits = CardTraits.None;
        }
        
        // 값 시스템 초기화
        protected virtual void InitializeValues(CardData cardData = null)
        {
            values.Clear();
            
            if (cardData != null)
            {
                values[ValueType.Hp] = cardData.hp;
                values[ValueType.MaxHp] = cardData.hp;
                values[ValueType.Mana] = cardData.costMana;
                values[ValueType.MaxMana] = cardData.costMana;
                values[ValueType.Attack] = cardData.atk;
                values[ValueType.Defense] = cardData.def;
                values[ValueType.Gold] = 0;
            }
            else
            {
                values[ValueType.Hp] = State.Hp;
                values[ValueType.MaxHp] = State.MaxHp;
                values[ValueType.Mana] = State.Mana;
                values[ValueType.MaxMana] = State.MaxMana;
                values[ValueType.Attack] = State.Atk;
                values[ValueType.Defense] = State.Def;
                values[ValueType.Gold] = 0;
            }
        }
        
        // CardData 업데이트
        public virtual void UpdateCardData(CardData cardData)
        {
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            CardImage = cardData.GetCardTexture();
            
            if (usesValues)
            {
                InitializeValues(cardData);
            }
        }
        
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
            
            // 값 시스템 연동
            if (usesValues)
            {
                ApplyAbilityToValues(cardAbility);
            }
        }
        
        protected virtual void ApplyAbilityToValues(CardAbility cardAbility)
        {
            // 어빌리티 효과를 값 시스템에 적용
            switch (cardAbility.abilityType)
            {
                case AbilityType.Damage:
                    // 데미지는 TakeDamage 메서드에서 처리
                    break;
                case AbilityType.Heal:
                    ModifyValue(ValueType.Hp, cardAbility.abilityValue);
                    break;
                case AbilityType.AtkUp:
                    ModifyValue(ValueType.Attack, cardAbility.abilityValue);
                    break;
                case AbilityType.DefUp:
                    ModifyValue(ValueType.Defense, cardAbility.abilityValue);
                    break;
                case AbilityType.BrokenDef:
                    ModifyValue(ValueType.Defense, -cardAbility.abilityValue);
                    break;
                // 다른 어빌리티 타입도 필요에 따라 추가
            }
        }
        
        public virtual void AddHandCardAbility(CardAbility cardAbility)
        {
            if (!usesAbilities) return;
            
            AbilitySystem.AddHandCardAbility(cardAbility);
        }
        
        public virtual List<CardAbility> DecreaseDuration()
        {
            if (!usesAbilities) return new List<CardAbility>();
            
            return AbilitySystem.DecreaseDuration();
        }
        
        public virtual void RemoveCardAbility(CardAbility cardAbility)
        {
            if (!usesAbilities) return;
            
            AbilitySystem.RemoveCardAbility(cardAbility);
        }
        
        public virtual void RemoveHandCardAbility(CardAbility cardAbility)
        {
            if (!usesAbilities) return;
            
            AbilitySystem.RemoveHandCardAbility(cardAbility);
        }
        
        public virtual int GetBuffCount()
        {
            if (!usesAbilities) return 0;
            
            return AbilitySystem.GetBuffCount();
        }
        
        public virtual int GetDeBuffCount()
        {
            if (!usesAbilities) return 0;
            
            return AbilitySystem.GetDeBuffCount();
        }
        
        public virtual void TakeDamage(int amount)
        {
            if (!usesCombat) return;
            
            State.TakeDamage(amount);
            
            if (usesValues)
            {
                values[ValueType.Hp] = State.Hp;
                values[ValueType.Defense] = State.Def;
            }
        }
        
        public virtual void RestoreHealth(int amount)
        {
            if (!usesCombat) return;
            
            State.RestoreHealth(amount);
            
            if (usesValues)
            {
                values[ValueType.Hp] = State.Hp;
            }
        }
        
        public virtual void IncreaseDefense(int amount)
        {
            if (!usesCombat) return;
            
            State.IncreaseDefense(amount);
            
            if (usesValues)
            {
                values[ValueType.Defense] = State.Def;
            }
        }
        
        public virtual void DecreaseDefense(int amount)
        {
            if (!usesCombat) return;
            
            State.DecreaseDefense(amount);
            
            if (usesValues)
            {
                values[ValueType.Defense] = State.Def;
            }
        }
        
        #endregion
        
        #region IValueCard 구현
        
        public virtual int GetValue(ValueType valueType)
        {
            if (!usesValues) 
            {
                // 값 시스템을 사용하지 않는 경우 CardState에서 값을 가져옴
                switch (valueType)
                {
                    case ValueType.Hp: return State.Hp;
                    case ValueType.MaxHp: return State.MaxHp;
                    case ValueType.Mana: return State.Mana;
                    case ValueType.MaxMana: return State.MaxMana;
                    case ValueType.Attack: return State.Atk;
                    case ValueType.Defense: return State.Def;
                    default: return 0;
                }
            }
            
            if (values.TryGetValue(valueType, out int value))
            {
                return value;
            }
            
            return 0;
        }
        
        public virtual void SetValue(ValueType valueType, int value)
        {
            if (!usesValues) 
            {
                // 값 시스템을 사용하지 않는 경우 CardState에 값을 설정
                switch (valueType)
                {
                    case ValueType.Hp: State.SetHp(value); break;
                    case ValueType.MaxHp: State.MaxHp = value; break;
                    case ValueType.Mana: State.SetMana(value); break;
                    case ValueType.MaxMana: State.MaxMana = value; break;
                    case ValueType.Attack: State.SetAtk(value); break;
                    case ValueType.Defense: State.SetDef(value); break;
                }
                return;
            }
            
            values[valueType] = value;
            
            // CardState 동기화
            switch (valueType)
            {
                case ValueType.Hp: State.SetHp(value); break;
                case ValueType.MaxHp: State.MaxHp = value; break;
                case ValueType.Mana: State.SetMana(value); break;
                case ValueType.MaxMana: State.MaxMana = value; break;
                case ValueType.Attack: State.SetAtk(value); break;
                case ValueType.Defense: State.SetDef(value); break;
            }
        }
        
        public virtual void ModifyValue(ValueType valueType, int delta)
        {
            if (!usesValues)
            {
                // 값 시스템을 사용하지 않는 경우 CardState의 값을 변경
                switch (valueType)
                {
                    case ValueType.Hp: 
                        if (delta > 0) State.RestoreHealth(delta);
                        else State.TakeDamage(-delta);
                        break;
                    case ValueType.Mana:
                        if (delta > 0) State.IncreaseMana(delta);
                        else State.DecreaseMana(-delta);
                        break;
                    case ValueType.Attack:
                        State.SetAtk(State.Atk + delta);
                        break;
                    case ValueType.Defense:
                        if (delta > 0) State.IncreaseDefense(delta);
                        else State.DecreaseDefense(-delta);
                        break;
                }
                return;
            }
            
            int currentValue = GetValue(valueType);
            SetValue(valueType, currentValue + delta);
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