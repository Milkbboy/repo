using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check if character/card is on champion side or enemy side
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Side", order = 10)]
    public class ConditionSide : ConditionData
    {
        [Header("Target is allied with champions/players")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            BattleCharacter target_owner = data.GetCharacter(target.owner_uid);
            return CompareBool(!target_owner.IsEnemy(), oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            return CompareBool(!target.IsEnemy(), oper);
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            return CompareBool(!target.enemy, oper);
        }
    }
}