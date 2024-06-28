using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Shuffle Deck
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Shuffle", order = 10)]
    public class EffectShuffle : EffectData
    {
        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            logic.ShuffleDeck(target.cards_deck);
        }
    }
}