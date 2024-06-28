using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    //Effect to Summon an entirely new card (not in anyones deck)
    //And places it on the board

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SummonCharacter", order = 10)]
    public class EffectSummonCharacter : EffectData
    {
        public CharacterData summon;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Slot target)
        {
            logic.SummonCharacter(caster.player_id, summon, target, card.level); 
        }
    }
}