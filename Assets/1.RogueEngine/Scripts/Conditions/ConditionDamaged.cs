using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that checks if a character is damaged
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Damaged", order = 10)]
    public class ConditionDamaged : ConditionData
    {
        [Header("Damage is")]
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareInt(target.damage, oper, value);
        }

    }
}