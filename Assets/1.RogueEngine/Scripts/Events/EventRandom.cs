using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Random", order = 10)]
    public class EventRandom : EventData
    {
        [Header("Events")]
        public EventData[] events;

        public override void DoEvent(WorldLogic logic, Champion triggerer)
        {
            System.Random rand = new System.Random(logic.WorldData.GetLocationSeed(3943));
            int index = rand.Next(events.Length);
            logic.TriggerEvent(triggerer, events[index]);
        }
    }
}
