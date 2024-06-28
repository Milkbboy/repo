using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect to transform a card into another card
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/TransformCharacter", order = 10)]
    public class EffectTransformCharacter : EffectData
    {
        public CharacterData transform_to;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            logic.TransformCharacter(target, transform_to);
        }
    }
}