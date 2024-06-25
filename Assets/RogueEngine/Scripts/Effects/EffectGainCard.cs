using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Gain item in adventure map
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/GainCard", order = 10)]
    public class EffectGainCard : EffectData
    {
        public CardData card;
        public int level = 1;

        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            champion.AddCard(card, level);
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, BattleCharacter target)
        {
            Card ncard = Card.Create(this.card, level, target);
            target.AddCard(target.cards_hand, ncard);
        }
    }
}