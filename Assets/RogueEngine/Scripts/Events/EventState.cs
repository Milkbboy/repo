using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/State", order = 10)]
    public class EventState : EventData
    {
        public WorldState state;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            logic.WorldData.state = state;
            logic.WorldData.ResetActionCompleted();
        }

        public static new EventState Get(string id)
        {
            foreach (EventData evt in GetAll())
            {
                if (evt.id == id && evt is EventState)
                    return evt as EventState;
            }
            return null;
        }

    }
}
