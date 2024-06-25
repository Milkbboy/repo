using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Add this to an ability to prevent it from being cast more than once per turn
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/OncePerTurn", order = 10)]
    public class ConditionOnce : ConditionData
    {
        public override bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card)
        {
            return !data.ability_played.Contains(ability.id);
        }

    }
}