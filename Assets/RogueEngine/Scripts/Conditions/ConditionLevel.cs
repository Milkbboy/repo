using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check if the card played is level
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardLevel", order = 10)]
    public class ConditionLevel : ConditionData
    {
        [Header("Card is Level")]
        public ConditionOperatorInt oper;
        public int level;

        public override bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card)
        {
            return CompareInt(card.level, oper, level);
        }

    }
}