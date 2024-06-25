using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect to play a card from your hand for free
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Play", order = 10)]
    public class EffectPlay : EffectData
    {
        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            Battle game = logic.GetBattleData();

            caster.RemoveCardFromAllGroups(target);
            caster.cards_hand.Add(target);

            Slot slot = Slot.GetRandom(logic.GetRandom());
            logic.PlayCard(target, slot, true);
        }
    }
}