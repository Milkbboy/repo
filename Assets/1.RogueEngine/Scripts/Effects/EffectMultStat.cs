using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/MultStat", order = 10)]
    public class EffectMultStat : EffectData
    {
        public EffectStatType type;
        public float value;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (type == EffectStatType.HP)
            {
                target.hp = Mathf.RoundToInt(target.hp * value);
            }

            if (type == EffectStatType.Mana)
            {
                target.mana = Mathf.RoundToInt(target.mana * value);
            }

            if (type == EffectStatType.Speed)
            {
                target.speed = Mathf.RoundToInt(target.speed * value);
            }

            if (type == EffectStatType.Hand)
            {
                target.hand = Mathf.RoundToInt(target.hand * value);
            }

            if (type == EffectStatType.Energy)
            {
                target.energy = Mathf.RoundToInt(target.energy * value);
            }

            if (type == EffectStatType.Shield)
            {
                target.shield = Mathf.RoundToInt(target.shield * value);
            }

        }

    }
}