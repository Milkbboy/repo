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
        public List<CardAbility> CardAbilities { get => cardAbilities; set => cardAbilities = value; }
        public List<CardAbility> PriorCardAbilities { get => cardAbilities.Where(ability => Constants.CardPriorAbilities.Contains(ability.abilityType)).ToList(); }
        public List<CardAbility> PostCardAbilities { get => cardAbilities.Where(ability => Constants.CardPostAbilities.Contains(ability.abilityType)).ToList(); }
        public List<CardAbility> BrokenDefAbilities { get => cardAbilities.Where(ability => ability.abilityType == AbilityType.BrokenDef).ToList(); }
        public List<CardAbility> DefUpAbilities { get => cardAbilities.Where(ability => ability.abilityType == AbilityType.DefUp).ToList(); }
        public CardAbility ArmorBreakAbility { get => cardAbilities.FirstOrDefault(ability => ability.abilityType == AbilityType.ArmorBreak); }

        public string LogText => Utils.CardLog(this);

        public virtual int Hp { get; set; }
        public virtual int Def { get; set; }
        public virtual int Mana { get; set; }
        public virtual int Atk { get; set; }

        public virtual void TakeDamage(int amount) { }
        public virtual void RestoreHealth(int amount) { }
        public virtual void SetDefense(int amount) { }
        public virtual void IncreaseDefense(int amount) { }
        public virtual void DecreaseDefense(int amount) { }

        private List<CardAbility> cardAbilities = new();

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

        public void AddCardAbility(AbilityData abilityData, AiData aiData, int selfSlotNum, int targetSlotNum, int turnCount, AbilityWhereFrom whereFrom)
        {
            Ability ability = new()
            {
                whereFrom = whereFrom,
                applyTurn = turnCount,
                value = abilityData.value,
                duration = abilityData.duration,
                createdDt = DateTime.UtcNow.Ticks
            };

            CardAbility cardAbility = cardAbilities.Find(ability => ability.abilityId == abilityData.abilityId);

            if (cardAbility == null)
            {
                cardAbility = new()
                {
                    aiType = aiData.type,
                    abilityId = abilityData.abilityId,
                    abilityType = abilityData.abilityType,
                    abilityValue = abilityData.value,
                    workType = abilityData.workType,
                    duration = abilityData.duration,
                    aiDataId = aiData.ai_Id,
                    selfSlotNum = selfSlotNum,
                    targetSlotNum = targetSlotNum,
                };

                // ArmorBreak 능력은 가장 먼저 적용되어야 함
                if (cardAbility.abilityType == AbilityType.ArmorBreak)
                    cardAbilities.Insert(0, cardAbility);
                else
                    cardAbilities.Add(cardAbility);
            }

            cardAbility.AddAbility(ability);

            Debug.Log($"{cardAbility.LogText} 추가. value: {abilityData.value}, duration: {abilityData.duration}, workType: {abilityData.workType}");
        }

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