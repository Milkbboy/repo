using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Checks if a slot contains a card or not
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotEmpty", order = 11)]
    public class ConditionSlotEmpty : ConditionData
    {
        [Header("Slot Is Empty")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            return CompareBool(false, oper); //Target is not empty slot
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareBool(false, oper); //Target is not empty slot
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        { 
            BattleCharacter slot_card = data.GetSlotCharacter(target);
            return CompareBool(slot_card == null, oper);
        }
    }
}