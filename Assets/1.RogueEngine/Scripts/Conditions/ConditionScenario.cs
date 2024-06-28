using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Condition that check current scenario
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Scenario", order = 10)]
    public class ConditionScenario : ConditionData
    {
        [Header("Is Scenario")]
        public ScenarioData scenario;
        public ConditionOperatorBool oper;

        public override bool IsMapEventConditionMet(World data, EventEffect evt, Champion champion)
        {
            return CompareBool(scenario.id == data.scenario_id, oper);
        }

    }
}