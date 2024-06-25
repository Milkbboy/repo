using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.AI;

namespace RogueEngine
{
    /// <summary>
    /// Condition that checks if the character is in front
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CharacterFront", order = 10)]
    public class ConditionCharacterFront : ConditionData
    {
        [Header("Character is in front")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            BattleCharacter front = data.GetFrontCharacter(target.IsEnemy());
            return CompareBool(front.uid == target.uid, oper); //Is Player
        }
    }
}