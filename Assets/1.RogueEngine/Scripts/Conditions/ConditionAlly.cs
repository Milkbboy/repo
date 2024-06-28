using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check the owner is in same team as target
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardAlly", order = 10)]
    public class ConditionAlly : ConditionData
    {
        [Header("Target is allied to caster")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            BattleCharacter target_owner = data.GetCharacter(target.owner_uid);
            bool same_owner = caster.IsEnemy() == target_owner.IsEnemy();
            return CompareBool(same_owner, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            bool same_owner = caster.IsEnemy() == target.IsEnemy();
            return CompareBool(same_owner, oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            bool same_owner = caster.IsEnemy() == target.enemy;
            return CompareBool(same_owner, oper);
        }
    }
}