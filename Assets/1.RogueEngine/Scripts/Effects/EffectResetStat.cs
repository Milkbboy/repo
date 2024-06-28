using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect to reset all stats to their original values
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ResetStat", order = 10)]
    public class EffectResetStat : EffectData
    {
        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            target.SetCard(target.CardData, target.level);
        }
    }
}