using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Checks if its your turn
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/RolledValue", order = 10)]
    public class ConditionRolled : ConditionData
    {
        [Header("Value Rolled is")]
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card)
        {
            return CompareInt(data.rolled_value, oper, value);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareInt(data.rolled_value, oper, value);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            return CompareInt(data.rolled_value, oper, value);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return CompareInt(data.rolled_value, oper, value);
        }
    }
}