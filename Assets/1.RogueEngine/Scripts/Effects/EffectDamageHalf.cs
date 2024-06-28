using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that damages a percentage of remaining HP
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/DamageHalf", order = 10)]
    public class EffectDamageHalf : EffectData
    {
        public float percentage = 0.5f;

        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            int damage = Mathf.RoundToInt(champion.GetHP() * percentage);
            champion.damage += damage;
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            int damage = Mathf.RoundToInt(target.GetHP() * percentage);
            logic.DamageCharacter(caster, target, damage);
        }

    }
}