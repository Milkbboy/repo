using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    public enum ConditionPlayerType
    {
        Self = 0,
        Opponent = 1,
        Both = 2,
    }

    /// <summary>
    /// Trigger condition that count the amount of cards in pile of your choise (deck/discard/hand/board...)
    /// Can also only count cards of a specific type/team/trait
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Count", order = 10)]
    public class ConditionCount : ConditionData
    {
        [Header("Count cards of type")]
        public ConditionPlayerType type;
        public PileType pile;
        public ConditionOperatorInt oper;
        public int value;

        [Header("Traits")]
        public ItemType has_type;
        public TeamData has_team;
        public TraitData has_trait;

        public override bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card)
        {
            int count = 0;
            if (type == ConditionPlayerType.Self || type == ConditionPlayerType.Both)
            {
                foreach (BattleCharacter character in data.characters)
                {
                    if (!character.IsEnemy())
                        count += CountPile(character, pile);
                }
            }
            if (type == ConditionPlayerType.Opponent || type == ConditionPlayerType.Both)
            {
                foreach (BattleCharacter character in data.characters)
                {
                    if(character.IsEnemy())
                        count += CountPile(character, pile);
                }
            }
            return CompareInt(count, oper, value);
        }

        private int CountPile(BattleCharacter player, PileType pile)
        {
            List<Card> card_pile = null;

            if (pile == PileType.Hand)
                card_pile = player.cards_hand;

            if (pile == PileType.Power)
                card_pile = player.cards_power;

            if (pile == PileType.Deck)
                card_pile = player.cards_deck;

            if (pile == PileType.Discard)
                card_pile = player.cards_discard;

            if (pile == PileType.Void)
                card_pile = player.cards_void;

            if (pile == PileType.Temp)
                card_pile = player.cards_temp;

            if (card_pile != null)
            {
                int count = 0;
                foreach (Card card in card_pile)
                {
                    if (IsTrait(card))
                        count++;
                }
                return count;
            }
            return 0;
        }

        private bool IsTrait(Card card)
        {
            bool is_type = card.CardData.item_type == has_type || has_type == ItemType.None;
            bool is_team = card.CardData.team == has_team || has_team == null;
            bool is_trait = card.HasTrait(has_trait) || has_trait == null;
            return (is_type && is_team && is_trait);
        }
    }
}