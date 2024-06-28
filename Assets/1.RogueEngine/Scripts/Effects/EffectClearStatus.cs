using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that removes a status,
    /// Will remove all status if the public field is empty
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ClearStatus", order = 10)]
    public class EffectClearStatus : EffectData
    {
        public StatusData status;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            if (status != null)
                target.RemoveStatus(status.effect);
            else
                target.status.Clear();
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            if (status != null)
                target.RemoveStatus(status.effect);
            else
                target.status.Clear();
        }
    }
}