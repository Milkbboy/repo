using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Compares basic card or player stats such as attack/hp/mana
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Stat", order = 10)]
    public class ConditionStat : ConditionData
    {
        [Header("Card stat is")]
        public EffectStatType type;
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsMapEventConditionMet(World data, EventEffect evt, Champion champion)
        {
            if (type == EffectStatType.HP)
            {
                return CompareInt(champion.hp, oper, value);
            }

            if (type == EffectStatType.Level)
            {
                return CompareInt(champion.level, oper, value);
            }

            if (type == EffectStatType.XP)
            {
                return CompareInt(champion.xp, oper, value);
            }

            if (type == EffectStatType.Gold)
            {
                Player player = data.GetPlayer(champion.player_id);
                return CompareInt(player.gold, oper, value);
            }

            return false;
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            if (type == EffectStatType.Mana)
            {
                return CompareInt(target.GetMana(), oper, value);
            }

            return false;
        }

        public override bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectStatType.HP)
            {
                return CompareInt(target.hp, oper, value);
            }

            if (type == EffectStatType.Mana)
            {
                return CompareInt(target.mana, oper, value);
            }

            if (type == EffectStatType.Shield)
            {
                return CompareInt(target.shield, oper, value);
            }

            return false;
        }
    }
}