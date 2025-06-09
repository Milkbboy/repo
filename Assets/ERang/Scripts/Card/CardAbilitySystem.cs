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

        public List<CardAbility> CardAbilities => cardAbilities;
        public List<CardAbility> HandAbilities => handAbilities;
        public IReadOnlyList<CardAbility> PriorCardAbilities => cardAbilities.Where(ability => Constants.CardPriorAbilities.Contains(ability.abilityType)).ToList();
        public IReadOnlyList<CardAbility> PostCardAbilities => cardAbilities.Where(ability => Constants.CardPostAbilities.Contains(ability.abilityType)).ToList();
        public IReadOnlyList<CardAbility> BrokenDefAbilities => cardAbilities.Where(ability => ability.abilityType == AbilityType.BrokenDef).ToList();
        public IReadOnlyList<CardAbility> DefUpAbilities => cardAbilities.Where(ability => ability.abilityType == AbilityType.DefUp).ToList();
        public CardAbility ArmorBreakAbility => cardAbilities.FirstOrDefault(ability => ability.abilityType == AbilityType.ArmorBreak);

        /// <summary>
        /// 보드 슬롯에서 지속되어야 하는 어빌리티 추가
        /// - 턴 표시
        /// - duration 0 되었을때 발동되어야 하는 어빌리티
        /// </summary>
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

            Debug.Log($"{cardAbility.LogText} {(found == null ? "신규" : "AbilityItem 만")} 추가. value: {cardAbility.abilityValue}, duration: {cardAbility.duration}, workType: {cardAbility.workType}");
        }

        public void AddHandCardAbility(CardAbility cardAbility)
        {
            handAbilities.Add(cardAbility);
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