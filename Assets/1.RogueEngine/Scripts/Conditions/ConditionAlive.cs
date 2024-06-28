using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check if character is alive
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Alive", order = 10)]
    public class ConditionAlive : ConditionData
    {
        [Header("Target is alive")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareBool(!target.IsDead(), oper);
        }
    }
}