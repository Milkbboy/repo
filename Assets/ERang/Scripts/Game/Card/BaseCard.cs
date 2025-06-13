using System;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 모든 카드 타입의 기본 기능을 구현하는 추상 클래스
    /// </summary>
    [Serializable]
    public abstract class BaseCard
    {
        // 테이블 관련 멤버 변수 - 대부분은 설정자를 protected로 제한
        private string uid;
        private int id;
        private string name;
        private CardType cardType;
        private CardGrade cardGrade;
        private List<int> aiGroupIds;
        private bool inUse;
        private bool isExtinction;
        private Texture2D cardImage;

        // 인터페이스 구현 - 대부분 읽기 전용, 일부만 쓰기 가능
        public string Uid { get => uid; protected set => uid = value; }
        public int Id { get => id; protected set => id = value; }
        public string Name { get => name; protected set => name = value; }
        public CardType CardType { get => cardType; protected set => cardType = value; }
        public CardGrade CardGrade { get => cardGrade; protected set => cardGrade = value; }
        public List<int> AiGroupIds { get => aiGroupIds; protected set => aiGroupIds = value; }
        public Dictionary<int, int> AiGroupIndexes { get; set; } = new(); // 외부에서 변경 가능
        public bool InUse { get => inUse; protected set => inUse = value; }
        public bool IsExtinction { get => isExtinction; protected set => isExtinction = value; }
        public Texture2D CardImage { get => cardImage; protected set => cardImage = value; }

        // 게임 관련 멤버 변수
        public CardStat Stat { get; protected set; }
        public CardAbilitySystem AbilitySystem { get; protected set; }
        public CardTraits Traits { get; protected set; }

        // 가상 속성 - 상속 클래스에서 재정의 가능
        public virtual int Hp => Stat.Hp;
        public virtual int MaxHp => Stat.MaxHp;
        public virtual int Def => Stat.Def;
        public virtual int Mana => Stat.Mana;
        public virtual int Atk => Stat.Atk;

        // 스탯
        public virtual void TakeDamage(int amount) => Stat.TakeDamage(amount);
        public virtual void RestoreHealth(int amount) => Stat.RestoreHealth(amount);
        public virtual void SetDefense(int amount) => Stat.SetDef(amount);
        public virtual void IncreaseDefense(int amount) => Stat.IncreaseDefense(amount);
        public virtual void DecreaseDefense(int amount) => Stat.DecreaseDefense(amount);

        public void SetCardTraits(CardTraits cardTraits) => Traits = cardTraits;

        // 기본 생성자
        protected BaseCard()
        {
            Stat = new CardStat(0, 0, 0, 0);
            AbilitySystem = new CardAbilitySystem();
            Traits = CardTraits.None;
        }

        // CardData 기반 생성자
        protected BaseCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            Name = cardData.nameDesc;
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

            Stat = new CardStat(cardData.hp, cardData.def, cardData.costMana, cardData.atk);
            AbilitySystem = new CardAbilitySystem();
        }

        // ID, Type, AiGroupId 기반 생성자
        protected BaseCard(int cardId, CardType cardType, string name, int aiGroupId, Texture2D cardImage)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardId;
            Name = name;
            CardType = cardType;
            AiGroupIds = new List<int> { aiGroupId };
            AiGroupIndexes = new Dictionary<int, int> { { aiGroupId, 0 } };
            CardImage = cardImage;
            Traits = CardTraits.None;

            Stat = new CardStat(0, 0, 0, 0);
            AbilitySystem = new CardAbilitySystem();
        }
    }
}