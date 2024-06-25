using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Battle", menuName = "TcgEngine/MapEvent/Text", order = 10)]
    public class EventText : EventData
    {
        [Header("Text")]
        [TextArea(5, 8)]
        public string text;

        [Header("Chain Event")]
        public EventData chain_event;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            if (!string.IsNullOrEmpty(text))
            {
                logic.TriggerTextEvent(champion, text);
                if (chain_event != null)
                    logic.TriggerEventNext(champion, chain_event);
            }
        }
    }
}
