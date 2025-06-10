using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 모든 카드 타입의 기본 기능을 구현하는 추상 클래스
    /// </summary>
    [Serializable]
    public abstract class BaseCard : ICard
    {
        // 테이블 관련 멤버 변수 - 대부분은 설정자를 protected로 제한
        private string uid;
        private int id;
        private CardType cardType;
        private CardGrade cardGrade;
        private List<int> aiGroupIds;
        private bool inUse;
        private bool isExtinction;
        private Texture2D cardImage;

        // 인터페이스 구현 - 대부분 읽기 전용, 일부만 쓰기 가능
        public string Uid { get => uid; protected set => uid = value; }
        public int Id { get => id; protected set => id = value; }
        public CardType CardType { get => cardType; protected set => cardType = value; }
        public CardGrade CardGrade { get => cardGrade; protected set => cardGrade = value; }
        public List<int> AiGroupIds { get => aiGroupIds; protected set => aiGroupIds = value; }
        public Dictionary<int, int> AiGroupIndexes { get; set; } = new(); // 외부에서 변경 가능
        public bool InUse { get => inUse; protected set => inUse = value; }
        public bool IsExtinction { get => isExtinction; protected set => isExtinction = value; }
        public Texture2D CardImage { get => cardImage; protected set => cardImage = value; }

        // 게임 관련 멤버 변수
        public CardState State { get; protected set; }
        public CardAbilitySystem AbilitySystem { get; protected set; }
        public CardTraits Traits { get; protected set; }
        public string LogText => Utils.CardLog(this);

        // 가상 속성 - 상속 클래스에서 재정의 가능
        public virtual int Hp => State.Hp;
        public virtual int MaxHp => State.MaxHp;
        public virtual int Def => State.Def;
        public virtual int Mana => State.Mana;
        public virtual int Atk => State.Atk;

        // 스탯
        public virtual void TakeDamage(int amount) => State.TakeDamage(amount);
        public virtual void RestoreHealth(int amount) => State.RestoreHealth(amount);
        public virtual void SetDefense(int amount) => State.SetDef(amount);
        public virtual void IncreaseDefense(int amount) => State.IncreaseDefense(amount);
        public virtual void DecreaseDefense(int amount) => State.DecreaseDefense(amount);

        // 어빌리티
        public virtual void AddCardAbility(CardAbility cardAbility, int turnCount, AbilityWhereFrom whereFrom) => AbilitySystem.AddCardAbility(cardAbility, turnCount, whereFrom);
        public virtual void AddHandCardAbility(CardAbility cardAbility) => AbilitySystem.AddHandCardAbility(cardAbility);
        public virtual List<CardAbility> DecreaseDuration() => AbilitySystem.DecreaseDuration();
        public virtual void RemoveCardAbility(CardAbility cardAbility) => AbilitySystem.RemoveCardAbility(cardAbility);
        public virtual void RemoveHandCardAbility(CardAbility cardAbility) => AbilitySystem.RemoveHandCardAbility(cardAbility);
        public virtual int GetBuffCount() => AbilitySystem.GetBuffCount();
        public virtual int GetDeBuffCount() => AbilitySystem.GetDeBuffCount();
        public void SetCardTraits(CardTraits cardTraits) => Traits = cardTraits;

        // 기본 생성자
        protected BaseCard()
        {
            State = new CardState(0, 0, 0, 0);
            AbilitySystem = new CardAbilitySystem();
            Traits = CardTraits.None;
        }

        // CardData 기반 생성자
        protected BaseCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupIds = cardData.aiGroup_ids != null ? new(cardData.aiGroup_ids) : new();
            AiGroupIndexes = new Dictionary<int, int>();
            foreach (int aiGroupId in AiGroupIds)
            {
                AiGroupIndexes[aiGroupId] = 0;
            }
            CardImage = cardData.GetCardTexture();
            Traits = CardTraits.None;
            CardGrade = cardData.cardGrade;

            State = new CardState(cardData.hp, cardData.def, cardData.costMana, cardData.atk);
            AbilitySystem = new CardAbilitySystem();
        }

        // ID, Type, AiGroupId 기반 생성자
        protected BaseCard(int cardId, CardType cardType, int aiGroupId, Texture2D cardImage)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardId;
            CardType = cardType;
            AiGroupIds = new List<int> { aiGroupId };
            AiGroupIndexes = new Dictionary<int, int> { { aiGroupId, 0 } };
            CardImage = cardImage;
            Traits = CardTraits.None;

            State = new CardState(0, 0, 0, 0);
            AbilitySystem = new CardAbilitySystem();
        }

        // CardData 업데이트
        public virtual void UpdateCardData(CardData cardData)
        {
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupIds = new List<int>(cardData.aiGroup_ids);
            AiGroupIndexes = AiGroupIds.ToDictionary(id => id, id => 0);
            CardImage = cardData.GetCardTexture();
        }

        // 카드 데이터 직접 업데이트
        public virtual void UpdateCardData(int cardId, CardType cardType, bool inUse, int aiGroupId, Texture2D cardImage)
        {
            Id = cardId;
            CardType = cardType;
            InUse = inUse;
            AiGroupIds = new List<int> { aiGroupId };
            AiGroupIndexes = AiGroupIds.ToDictionary(id => id, id => 0);
            CardImage = cardImage;
        }

        // 하위 클래스에서 구현할 수 있는 카드 라이프사이클 이벤트 메서드
        public virtual void OnPlay() { }
        public virtual void OnTurnStart() { }
        public virtual void OnTurnEnd() { }
        public virtual void OnDiscard() { }
    }
}