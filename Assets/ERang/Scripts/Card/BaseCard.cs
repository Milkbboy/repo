using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    // 기본 카드
    [Serializable]
    public class BaseCard : ICard
    {
        // 테이블 관련 멤버 변수
        public string Uid { get; set; }
        public int Id { get; set; }
        public CardType CardType { get; set; }
        public int AiGroupId { get; set; }
        public int AiGroupIndex { get; set; }
        public bool InUse { get; set; }
        public bool IsExtinction { get; set; }
        public Texture2D CardImage { get; set; }

        public int Hp { get { return hp; } set { hp = value; } }
        public int MaxHp { get { return maxHp; } set { maxHp = value; } }
        public int Def { get { return def; } set { def = value; } }
        public int Mana { get { return mana; } set { mana = value; } }
        public int Atk { get { return atk; } set { atk = value; } }
        public CardTraits Traits { get; set; }

        // 게임 관련 멤버 변수
        public List<CardAbility> CardAbilities { get => cardAbilities; set => cardAbilities = value; }
        public List<CardAbility> HandAbilities { get => handAbilities; set => handAbilities = value; }

        public List<CardAbility> PriorCardAbilities { get => cardAbilities.Where(ability => Constants.CardPriorAbilities.Contains(ability.abilityType)).ToList(); }
        public List<CardAbility> PostCardAbilities { get => cardAbilities.Where(ability => Constants.CardPostAbilities.Contains(ability.abilityType)).ToList(); }
        public List<CardAbility> BrokenDefAbilities { get => cardAbilities.Where(ability => ability.abilityType == AbilityType.BrokenDef).ToList(); }
        public List<CardAbility> DefUpAbilities { get => cardAbilities.Where(ability => ability.abilityType == AbilityType.DefUp).ToList(); }
        public CardAbility ArmorBreakAbility { get => cardAbilities.FirstOrDefault(ability => ability.abilityType == AbilityType.ArmorBreak); }

        public string LogText => Utils.CardLog(this);

        protected int hp;
        protected int maxHp;
        protected int atk;
        protected int def;
        protected int mana;

        public virtual void TakeDamage(int amount) { }
        public virtual void RestoreHealth(int amount) { }
        public virtual void SetDefense(int amount) { }
        public virtual void IncreaseDefense(int amount) { }
        public virtual void DecreaseDefense(int amount) { }

        // 보드에서 적용되는 어빌리티
        private List<CardAbility> cardAbilities = new();
        // 핸드에서 적용되는 어빌리티
        private List<CardAbility> handAbilities = new();

        public BaseCard()
        {
        }

        public BaseCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            AiGroupIndex = 0;
            CardImage = cardData.GetCardTexture();
            Traits = CardTraits.None;

            hp = cardData.hp;
            maxHp = cardData.hp;
            atk = cardData.atk;
            def = cardData.def;
            mana = cardData.costMana;
        }

        public BaseCard(Master master)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = master.MasterId;
            CardType = CardType.Master;
            AiGroupId = 0;
            AiGroupIndex = 0;
            CardImage = master.CardImage;
            Traits = CardTraits.None;

            hp = master.Hp;
            maxHp = master.MaxHp;
            atk = master.Atk;
            def = master.Def;
            mana = master.Mana;
        }

        public BaseCard(int cardId, CardType cardType, int aiGroupId, Texture2D cardImage)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardId;
            CardType = cardType;
            AiGroupId = aiGroupId;
            AiGroupIndex = 0;
            CardImage = cardImage;
            Traits = CardTraits.None;
        }

        public void UpdateCardData(CardData cardData)
        {
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            CardImage = cardData.GetCardTexture();
        }

        public void UpdateCardData(int cardId, CardType cardType, bool inUse, int aiGroupId, Texture2D cardImage)
        {
            Id = cardId;
            CardType = cardType;
            InUse = inUse;
            AiGroupId = aiGroupId;
            AiGroupIndex = 0;
            CardImage = cardImage;
        }

        public void IncreaseAttack(int amount)
        {
            atk += amount;
        }

        public void DecreaseAttack(int amount)
        {
            atk -= amount;
        }

        public void IncreaseMana(int amount)
        {
            mana += amount;
        }

        public void DecreaseMana(int amount)
        {
            mana -= amount;

            if (mana < 0)
                mana = 0;
        }

        /// <summary>
        /// 보드 슬롯에서 지속되어야 하는 어빌리티 추가
        ///  - 턴 표시
        ///  - duration 0 되었을때 발동되어야 하는 어빌리티
        /// </summary>
        public void AddCardAbility(CardAbility cardAbility, int turnCount, AbilityWhereFrom whereFrom)
        {
            // 어빌리티 아이템 - 동일한 어빌리티가 추가되면 AbilityItem 이 추가되고 duration 이 증가. 효과는 변하지 않음
            AbilityItem abilityItem = new()
            {
                whereFrom = whereFrom,
                applyTurn = turnCount,
                value = cardAbility.abilityValue,
                duration = cardAbility.duration,
                createdDt = DateTime.UtcNow.Ticks
            };

            CardAbility found = cardAbilities.Find(ability => ability.abilityId == cardAbility.abilityId);

            if (found == null)
            {
                // ArmorBreak 는 다른 def 효과를 무시하기 때문에 제일 앞에 추가해서 가장 먼저 적용되게 설정
                if (cardAbility.abilityType == AbilityType.ArmorBreak)
                    cardAbilities.Insert(0, cardAbility);
                else
                    cardAbilities.Add(cardAbility);
            }

            cardAbility.AddAbilityItem(abilityItem);

            Debug.Log($"{cardAbility.LogText} {(found == null ? "신규" : "AbilityItem 만")} 추가. value: {cardAbility.abilityValue}, duration: {cardAbility.duration}, workType: {cardAbility.workType}");
        }

        /// <summary>
        /// 핸드에 들어올때 적용되는 어빌리티 추가
        /// </summary>
        public void AddHandCardAbility(CardAbility cardAbility)
        {
            Debug.Log($"AddHandCardAbility. cardAbility: {cardAbility.LogText}");
            handAbilities.Add(cardAbility);
        }

        /// <summary>
        /// 어빌리티 duration 감소
        /// - AbilityItem 의 duration 감소
        /// </summary>
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

        public void RemoveCardAbility(CardAbility cardAbility)
        {
            cardAbilities.Remove(cardAbility);
        }

        public void RemoveHandCardAbility(CardAbility cardAbility)
        {
            handAbilities.Remove(cardAbility);
        }

        public int GetBuffCount()
        {
            return cardAbilities.Count(ability => ability.aiType == AiDataType.Buff);
        }

        public int GetDeBuffCount()
        {
            return cardAbilities.Count(ability => ability.aiType == AiDataType.Debuff);
        }
    }
}