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

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Heal", order = 10)]
    public class EffectHeal : EffectData
    {
        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            champion.damage -= evt.value;
            champion.damage = Mathf.Max(champion.damage, 0);
        }

        public override void DoMapEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            champion.damage -= ability.GetValue();
            champion.damage = Mathf.Max(champion.damage, 0);
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            logic.HealCharacter(target, ability.GetValue(caster, card));
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            BattleCharacter character = logic.BattleData.GetCharacter(target.owner_uid);
            logic.HealCharacter(character, ability.GetValue(caster, card));
        }

    }
}