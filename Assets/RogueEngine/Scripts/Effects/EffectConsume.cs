using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Consume item and remove it from inventory
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ConsumeItem", order = 10)]
    public class EffectConsume : EffectData
    {

        public override void DoMapEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            champion.RemoveItem(item.card_id);
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            Champion champion = logic.WorldData.GetChampion(caster.uid);
            if (champion != null)
            {
                champion.RemoveItem(target.card_id);

                ChampionItem citem = champion.GetItem(target.card_id);
                if (citem == null || citem.quantity <= 0)
                    caster.cards_item.Remove(target);
            }

        }

    }
}