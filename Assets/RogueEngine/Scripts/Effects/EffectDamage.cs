using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Damage", order = 10)]
    public class EffectDamage : EffectData
    {
        public bool ignore_shield;
        public bool shield_only;

        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            champion.damage += evt.value;
        }

        public override void DoMapEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            champion.damage += ability.GetValue();
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            int damage = ability.GetValue(caster, card);

            if (shield_only)
                logic.DamageShield(target, damage);
            else
                logic.DamageCharacter(caster, target, damage, ignore_shield);
        }

    }
}