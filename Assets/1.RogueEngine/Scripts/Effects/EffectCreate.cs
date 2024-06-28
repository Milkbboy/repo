using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effects that creates a new card from a CardData
    /// Use for discover effects
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Create", order = 10)]
    public class EffectCreate : EffectData
    {
        public PileType create_pile;   //Better to not select Board here, for placing a card on board or in secret area, would suggest instead EffectSummon, or EffectPlay as chain after Create

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, CardData target)
        {
            Card ncard = Card.Create(target, 1, caster);
            logic.BattleData.last_summoned = ncard.uid;

            if(create_pile == PileType.Deck)
                caster.cards_deck.Add(ncard);

            if (create_pile == PileType.Discard)
                caster.cards_discard.Add(ncard);

            if (create_pile == PileType.Void)
                caster.cards_void.Add(ncard);

            if (create_pile == PileType.Hand)
                caster.cards_hand.Add(ncard);

            if (create_pile == PileType.Power)
                caster.cards_power.Add(ncard);

            if (create_pile == PileType.Temp)
                caster.cards_temp.Add(ncard);
        }
    }
}