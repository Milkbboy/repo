using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Reward", order = 10)]
    public class EventReward : EventData
    {
        public RarityData rarity;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            logic.GainCardRewards(rarity);
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
