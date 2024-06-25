using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Remove and item in adventure map
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/RemoveItem", order = 10)]
    public class EffectRemoveItem : EffectData
    {
        public CardData item;

        public override void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            champion.RemoveItem(item);
        }

    }
}