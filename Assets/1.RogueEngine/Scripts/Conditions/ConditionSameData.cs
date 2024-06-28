using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check if the target is the same as the caster (same card data, not same instance)
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardSameData", order = 10)]
    public class ConditionSameData : ConditionData
    {
        [Header("Target card is dupplicate of caster")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            return CompareBool(card.card_id == target.card_id, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareBool(caster == target, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return false;
        }
    }
}