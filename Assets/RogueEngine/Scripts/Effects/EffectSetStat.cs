using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that sets basic stats (hp/attack/mana) to a specific value
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SetStat", order = 10)]
    public class EffectSetStat : EffectData
    {
        public EffectStatType type;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectStatType.HP)
            {
                target.hp = ability.GetValue(caster, card);
            }

            if (type == EffectStatType.Mana)
            {
                target.mana = ability.GetValue(caster, card);
                target.mana = Mathf.Max(target.mana, 0);
            }
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            if (type == EffectStatType.Mana)
                target.mana = ability.GetValue(caster, card);
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectStatType.HP)
                target.hp = ability.GetValue(caster, card);
            if (type == EffectStatType.Mana)
                target.mana = ability.GetValue(caster, card);
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            if (type == EffectStatType.Mana)
                target.mana = ability.GetValue(caster, card);
        }

    }
}