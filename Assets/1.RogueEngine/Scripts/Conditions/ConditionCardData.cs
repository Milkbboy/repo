using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that checks the card data matches
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardData", order = 10)]
    public class ConditionCardData : ConditionData
    {
        [Header("Card is")]
        public CardData card_type;

        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, Card target)
        {
            return CompareBool(target.card_id == card_type.id, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, BattleCharacter target)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, Slot target)
        {
            return false; //Not a card
        }
    }
}