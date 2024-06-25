using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana, by the value of the dice roll
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatRoll", order = 10)]
    public class EffectAddStatRoll : EffectData
    {
        public EffectStatType type;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            Battle data = logic.GetBattleData();

            if (type == EffectStatType.HP)
            {
                target.hp += data.rolled_value;
            }

            if (type == EffectStatType.Mana)
            {
                target.mana += data.rolled_value;
                target.mana = Mathf.Max(target.mana, 0);
            }
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            Battle data = logic.GetBattleData();

            if (type == EffectStatType.Mana)
                target.mana += data.rolled_value;
        }
    }
}