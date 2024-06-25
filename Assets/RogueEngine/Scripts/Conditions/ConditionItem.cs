using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.AI;

namespace RogueEngine
{
    /// <summary>
    /// Condition that compares the target category of an ability to the actual target (card, player or slot)
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Item", order = 10)]
    public class ConditionItem: ConditionData
    {
        [Header("Champion has Item")]
        public CardData item;

        public override bool IsMapEventConditionMet(World data, EventEffect evt, Champion champion)
        {
            return champion.HasItem(item);
        }
    }
}