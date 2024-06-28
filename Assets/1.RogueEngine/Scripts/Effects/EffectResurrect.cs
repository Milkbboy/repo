using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effects that heals a card or player (hp)
    /// It cannot restore more than the original hp, use AddStats to go beyond original
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Resurrect", order = 10)]
    public class EffectResurrect : EffectData
    {
        public float percentage;

        public override void DoMapEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            int value = Mathf.RoundToInt(percentage * target.GetHPMax());
            target.damage = target.GetHPMax() - value;
            target.damage = Mathf.Max(target.damage, 0);
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            int value = Mathf.RoundToInt(percentage * target.GetHPMax());
            logic.ResurrectCharacter(target, value);
        }

    }
}