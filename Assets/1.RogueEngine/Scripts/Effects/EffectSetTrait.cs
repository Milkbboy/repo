using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that sets custom stats to a specific value
    /// </summary>
    
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SetTrait", order = 10)]
    public class EffectSetTrait : EffectData
    {
        public TraitData trait;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            target.SetTrait(trait.id, ability.GetValue(caster, card));
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            target.SetTrait(trait.id, ability.GetValue(caster, card));
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            target.SetTrait(trait.id, ability.GetValue(caster, card));
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            target.SetTrait(trait.id, ability.GetValue(caster, card));
        }
    }
}