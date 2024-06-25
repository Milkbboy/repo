using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect to upgrade a card until the end of battle
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/UpgradeCard", order = 10)]
    public class EffectUpgradeCard : EffectData
    {
        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            target.level += ability.GetValue(caster, card);
            target.level = Mathf.Clamp(target.level, 1, target.CardData.level_max);
            target.mana += target.CardData.upgrade_mana * ability.GetValue(caster, card);
        }
    }

}