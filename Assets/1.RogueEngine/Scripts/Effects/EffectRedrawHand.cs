using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/RedrawHand", order = 10)]
    public class EffectRedrawHand : EffectData
    {
        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            int nb_cards = target.cards_hand.Count + ability.GetValue(caster, card);        //Store number of cards in hand
            target.cards_discard.AddRange(target.cards_hand); //Add hand cards to your deck
            target.cards_hand.Clear();                     //Empty hand array
            //logic.ShuffleDeck(target.cards_deck);          //Shuffle Deck
            logic.DrawCard(target, nb_cards);  //Redraw Hand
        }
    }
}
