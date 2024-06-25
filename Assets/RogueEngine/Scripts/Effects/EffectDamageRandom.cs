using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// Adds a random value to the base ability.value
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/DamageRandom", order = 10)]
    public class EffectDamageRandom : EffectData
    {
        public int dice; //Dice set to 6 would add between 1-6 at random, so a ability.value of 4 would deal 5-10 damage.

        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            int seed = logic.WorldData.GetLocationSeed(755);
            System.Random rand = new System.Random(seed);
            int rval = rand.Next(1, dice);
            int damage = evt.value + rval;
            champion.damage += damage;
        }

        public override void DoMapEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            int seed = logic.WorldData.GetLocationSeed(755);
            System.Random rand = new System.Random(seed);
            int rval = rand.Next(1, dice);
            int damage = ability.GetValue() + rval;
            champion.damage += damage;
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            int seed = logic.WorldData.GetLocationSeed(384);
            System.Random rand = new System.Random(seed + caster.Hash + caster.turn);
            int rval = rand.Next(1, dice);
            int damage = ability.GetValue(caster, card) + rval;
            logic.DamageCharacter(caster, target, damage);
        }

    }
}