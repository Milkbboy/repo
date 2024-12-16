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
        public string Uid { get; set; }
        public int Id { get; set; }
        public CardType CardType { get; set; }
        public int AiGroupId { get; set; }
        public int AiGroupIndex { get; set; }
        public bool InUse { get; set; }
        public bool IsExtinction { get; set; }
        public Texture2D CardImage { get; set; }
        public List<CardAbility> Abilities { get => abilities; set => abilities = value; }

        public string LogText => Utils.CardLog(this);

        private List<CardAbility> abilities = new();

        public virtual int Hp { get; set; }
        public virtual int Def { get; set; }
        public virtual int Mana { get; set; }
        public virtual int Atk { get; set; }

        public virtual void TakeDamage(int amount) { }
        public virtual void RestoreHealth(int amount) { }
        public virtual void IncreaseDefense(int amount) { }
        public virtual void DecreaseDefense(int amount) { }

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
        }

        public BaseCard(int cardId, CardType cardType, int aiGroupId, Texture2D cardImage)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardId;
            CardType = cardType;
            AiGroupId = aiGroupId;
            AiGroupIndex = 0;
            CardImage = cardImage;
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

        public void AddAbility(CardAbility ability)
        {
            abilities.Add(ability);
        }

        public int GetBuffCount()
        {
            return abilities.Count(ability => ability.aiType == AiDataType.Buff);
        }

        public int GetDeBuffCount()
        {
            return abilities.Count(ability => ability.aiType == AiDataType.Debuff);
        }
    }
}