using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Compares cards or players custom stats
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Trait", order = 10)]
    public class ConditionTrait : ConditionData
    {
        [Header("Card stat is")]
        public TraitData trait;
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            return CompareInt(target.GetTraitValue(trait.id), oper, value);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareInt(target.GetTraitValue(trait.id), oper, value);
        }
    }
}