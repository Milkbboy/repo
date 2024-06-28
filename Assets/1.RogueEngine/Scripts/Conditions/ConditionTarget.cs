using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.AI;

namespace RogueEngine
{
    /// <summary>
    /// Condition that compares the target category of an ability to the actual target (card, player or slot)
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Player", order = 10)]
    public class ConditionTarget : ConditionData
    {
        [Header("Target is of type")]
        public ConditionTargetType type;
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            return CompareBool(type == ConditionTargetType.Card, oper); //Is Card
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareBool(type == ConditionTargetType.Character, oper); //Is Player
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return CompareBool(type == ConditionTargetType.Slot, oper); //Is Player
        }
    }

    public enum ConditionTargetType
    {
        None = 0,
        Card = 10,
        Character = 20,
        Slot = 30,
    }
}