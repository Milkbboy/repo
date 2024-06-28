using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    //Effect to Summon an entirely new card (not in anyones deck)
    //And places it on the board

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SummonCard", order = 10)]
    public class EffectSummonCard : EffectData
    {
        public CardData summon;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            logic.SummonCardHand(target, summon, card.level); //Summon in hand instead of board when target a player
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, CardData target)
        {
            logic.SummonCardHand(caster, target, card.level);   //Summon in hand instead of board when target a carddata
        }
    }
}