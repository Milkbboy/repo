using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect to transform a card into another card
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/TransformCard", order = 10)]
    public class EffectTransformCard : EffectData
    {
        public CardData transform_to;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            logic.TransformCard(target, transform_to);
        }
    }
}