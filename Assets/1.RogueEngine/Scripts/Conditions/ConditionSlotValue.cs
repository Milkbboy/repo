using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// SlotValue compare each slot x and y to a specific value
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotValue", order = 11)]
    public class ConditionSlotValue : ConditionData
    {
        [Header("Slot Value")]
        public ConditionOperatorInt oper_x;
        public int value_x = 0;
        
        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return IsTargetConditionMet(data, ability, caster, card, target.slot);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            bool valid_x = CompareInt(target.x, oper_x, value_x);
            return valid_x;
        }
    }
}