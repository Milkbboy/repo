using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that checks the type, team and traits of a card
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Type", order = 10)]
    public class ConditionType : ConditionData
    {
        [Header("Card is of type")]
        public ItemType has_type;
        public CardType has_play_type;
        public TeamData has_team;
        public TraitData has_trait;

        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            return CompareBool(IsTrait(target), oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareBool(IsTrait(target), oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, CardData target)
        {
            bool is_type = target.item_type == has_type || has_type == ItemType.None;
            bool is_play_type = card.CardData.card_type == has_play_type || has_play_type == CardType.None;
            bool is_team = target.team == has_team || has_team == null;
            bool is_trait = target.HasTrait(has_trait) || has_trait == null;
            return (is_type && is_play_type && is_team && is_trait);
        }

        private bool IsTrait(Card card)
        {
            bool is_type = card.CardData.item_type == has_type || has_type == ItemType.None;
            bool is_play_type = card.CardData.card_type == has_play_type || has_play_type == CardType.None;
            bool is_team = card.CardData.team == has_team || has_team == null;
            bool is_trait = card.HasTrait(has_trait) || has_trait == null;
            return (is_type && is_play_type && is_team && is_trait);
        }

        private bool IsTrait(BattleCharacter card)
        {
            if (card.is_champion)
            {
                bool is_trait = card.HasTrait(has_trait) || has_trait == null;
                return is_trait;
            }
            else
            {
                bool is_trait = card.HasTrait(has_trait) || has_trait == null;
                return is_trait;
            }
        }
    }
}