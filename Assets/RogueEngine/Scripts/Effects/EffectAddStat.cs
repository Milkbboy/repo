using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStat", order = 10)]
    public class EffectAddStat : EffectData
    {
        public EffectStatType type;

        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            if (type == EffectStatType.HP)
            {
                champion.hp += evt.value;
            }

            if (type == EffectStatType.XP)
            {
                champion.xp += evt.value;
            }

            if (type == EffectStatType.Gold)
            {
                Player player = logic.WorldData.GetPlayer(champion.player_id);
                player.gold += evt.value;
            }
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectStatType.HP)
            {
                target.hp += ability.GetValue(caster, card);
            }

            if (type == EffectStatType.Mana)
            {
                target.mana += ability.GetValue(caster, card);
            }

            if (type == EffectStatType.Speed)
            {
                target.speed += ability.GetValue(caster, card);
            }

            if (type == EffectStatType.Hand)
            {
                target.hand += ability.GetValue(caster, card);
            }

            if (type == EffectStatType.Energy)
            {
                target.energy += ability.GetValue(caster, card);
            }

            if (type == EffectStatType.Shield)
            {
                target.shield += ability.GetValue(caster, card);
                target.shield = Mathf.Max(target.shield, 0);
            }
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            if (type == EffectStatType.Mana)
                target.mana += ability.GetValue(caster, card);
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectStatType.Speed)
                target.speed_ongoing += ability.GetValue(caster, card);
            if (type == EffectStatType.HP)
                target.hp_ongoing += ability.GetValue(caster, card);
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            if (type == EffectStatType.Mana)
                target.mana_ongoing += ability.GetValue(caster, card);
        }

    }

    public enum EffectStatType
    {
        None = 0,
        Mana = 5,

        HP = 10,
        Shield = 12,

        Speed = 20,
        Energy = 22,
        Hand =24,

        XP = 30,
        Level = 31,
        Gold = 32,


    }
}