using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check current map
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Map", order = 10)]
    public class ConditionMap : ConditionData
    {
        [Header("Is Map")]
        public MapData map;
        public ConditionOperatorBool oper;

        public override bool IsMapEventConditionMet(World data, EventEffect evt, Champion champion)
        {
            return CompareBool(map.id == data.map_id, oper);
        }

    }
}