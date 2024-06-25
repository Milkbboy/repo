using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check the owner of the target match the owner of the caster (champion)
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardOwner", order = 10)]
    public class ConditionOwner : ConditionData
    {
        [Header("Target owner is caster owner")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            bool same_owner = caster.uid == target.owner_uid;
            return CompareBool(same_owner, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            bool same_owner = caster.uid == target.uid;
            return CompareBool(same_owner, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return false;
        }
    }
}