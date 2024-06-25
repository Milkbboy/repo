using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    //Sends the target card to a pile of your choice (deck/discard/hand)
    //Dont use to send to board since it needs a slot, use EffectPlay instead to send to board
    //Also dont send to discard from the board because it wont trigger OnKill effects, use EffectDestroy instead

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SendPile", order = 10)]
    public class EffectSendPile : EffectData
    {
        public PileType pile;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            Battle data = logic.GetBattleData();
            BattleCharacter player = data.GetCharacter(target.owner_uid);

            if (pile == PileType.Deck)
            {
                player.RemoveCardFromAllGroups(target);
                player.cards_deck.Add(target);
            }

            if (pile == PileType.Power)
            {
                player.RemoveCardFromAllGroups(target);
                player.cards_power.Add(target);
            }

            if (pile == PileType.Hand)
            {
                player.RemoveCardFromAllGroups(target);
                player.cards_hand.Add(target);
            }

            if (pile == PileType.Discard)
            {
                player.RemoveCardFromAllGroups(target);
                player.cards_discard.Add(target);
            }

            if (pile == PileType.Void)
            {
                player.RemoveCardFromAllGroups(target);
                player.cards_void.Add(target);
            }

            if (pile == PileType.Temp)
            {
                player.RemoveCardFromAllGroups(target);
                player.cards_temp.Add(target);
            }
        }
    }

    public enum PileType
    {
        None = 0,
        Hand = 20,
        Power = 25,
        Deck = 30,
        Discard = 40,
        Void = 50,
        Temp = 90,
    }

}
