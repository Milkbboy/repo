using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana, but only at the start of their next turn
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatDelayed", order = 10)]
    public class EffectAddStatDelayed : EffectData
    {
        public EffectDelayedStatType type;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectDelayedStatType.Shield)
            {
                target.delayed_shield += ability.GetValue(caster, card);
                target.delayed_shield = Mathf.Max(target.delayed_shield, 0);
            }

            if (type == EffectDelayedStatType.Mana)
            {
                target.delayed_energy += ability.GetValue(caster, card);
            }
            
            if (type == EffectDelayedStatType.Card)
            {
                target.delayed_hand += ability.GetValue(caster, card);
            }
        }

    }

    public enum EffectDelayedStatType
    {
        None = 0,

        Mana = 5,
        Shield = 12,

        Card = 24,
    }
}