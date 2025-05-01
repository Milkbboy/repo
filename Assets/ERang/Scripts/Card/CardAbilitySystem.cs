using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ERang
{
    public class CardAbilitySystem
    {
        private readonly List<CardAbility> cardAbilities = new();
        private readonly List<CardAbility> handAbilities = new();

        public event Action<CardAbility> OnAbilityAdded;
        public event Action<CardAbility> OnAbilityRemoved;
        public event Action<CardAbility> OnHandAbilityAdded;
        public event Action<CardAbility> OnHandAbilityRemoved;

        public IReadOnlyList<CardAbility> CardAbilities => cardAbilities;
        public IReadOnlyList<CardAbility> HandAbilities => handAbilities;
        public IReadOnlyList<CardAbility> PriorCardAbilities => cardAbilities.Where(ability => Constants.CardPriorAbilities.Contains(ability.abilityType)).ToList();
        public IReadOnlyList<CardAbility> PostCardAbilities => cardAbilities.Where(ability => Constants.CardPostAbilities.Contains(ability.abilityType)).ToList();
        public IReadOnlyList<CardAbility> BrokenDefAbilities => cardAbilities.Where(ability => ability.abilityType == AbilityType.BrokenDef).ToList();
        public IReadOnlyList<CardAbility> DefUpAbilities => cardAbilities.Where(ability => ability.abilityType == AbilityType.DefUp).ToList();
        public CardAbility ArmorBreakAbility => cardAbilities.FirstOrDefault(ability => ability.abilityType == AbilityType.ArmorBreak);

        public void AddCardAbility(CardAbility cardAbility, int turnCount, AbilityWhereFrom whereFrom)
        {
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
                if (cardAbility.abilityType == AbilityType.ArmorBreak)
                    cardAbilities.Insert(0, cardAbility);
                else
                    cardAbilities.Add(cardAbility);
            }

            cardAbility.AddAbilityItem(abilityItem);
            OnAbilityAdded?.Invoke(cardAbility);

            Debug.Log($"{cardAbility.LogText} {(found == null ? "신규" : "AbilityItem 만")} 추가. value: {cardAbility.abilityValue}, duration: {cardAbility.duration}, workType: {cardAbility.workType}");
        }

        public void AddHandCardAbility(CardAbility cardAbility)
        {
            handAbilities.Add(cardAbility);
            OnHandAbilityAdded?.Invoke(cardAbility);
            Debug.Log($"AddHandCardAbility. cardAbility: {cardAbility.LogText}");
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
            if (cardAbilities.Remove(cardAbility))
            {
                OnAbilityRemoved?.Invoke(cardAbility);
            }
        }

        public void RemoveHandCardAbility(CardAbility cardAbility)
        {
            if (handAbilities.Remove(cardAbility))
            {
                OnHandAbilityRemoved?.Invoke(cardAbility);
            }
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