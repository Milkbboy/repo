using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.AI;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/PlayTarget", order = 10)]
    public class ConditionPlayTarget : ConditionData
    {
        [Header("Card is PlayTarget")]
        public AbilityTarget play_target;
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            bool valid = (play_target == AbilityTarget.None) ? target.CardData.IsRequireTarget() : target.CardData.IsRequireTargetSpell(play_target);
            return CompareBool(valid, oper); //Is Card
        }

    }

}