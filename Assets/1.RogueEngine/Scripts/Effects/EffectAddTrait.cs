using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that adds card/player custom stats or traits
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddTrait", order = 10)]
    public class EffectAddTrait : EffectData
    {
        public TraitData trait;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            target.AddTrait(trait.id, ability.GetValue(caster, card));
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            target.AddTrait(trait.id, ability.GetValue(caster, card));
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            target.AddOngoingTrait(trait.id, ability.GetValue(caster, card));
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            target.AddOngoingTrait(trait.id, ability.GetValue(caster, card));
        }
    }
}