using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect to discard cards from hand
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Discard", order = 10)]
    public class EffectDiscard : EffectData
    {
        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            logic.DiscardCard(target);
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            Card tcard = GameTool.Pick1Random(target.cards_hand);
            logic.DiscardCard(tcard);
        }
    }
}