using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Compare the turn count
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/TurnCount", order = 10)]
    public class ConditionTurnCount : ConditionData
    {
        public ConditionOperatorInt oper;
        public int turn = 1;

        public override bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card)
        {
            return CompareInt(data.turn_count, oper, turn);
        }
    }
}