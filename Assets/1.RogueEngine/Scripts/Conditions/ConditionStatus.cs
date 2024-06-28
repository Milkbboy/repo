using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Checks if a player or card has a status effect
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardStatus", order = 10)]
    public class ConditionStatus : ConditionData
    {
        [Header("Card has status")]
        public StatusEffect has_status;
        public int value = 0;
        public ConditionOperatorBool oper;

        public override bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card)
        {
            bool hstatus = character.HasStatus(has_status) && character.GetStatusValue(has_status) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            bool hstatus = target.HasStatus(has_status) && target.GetStatusValue(has_status) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            bool hstatus = target.HasStatus(has_status) && target.GetStatusValue(has_status) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return false;
        }
    }
}