using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Upgrade", order = 10)]
    public class EventUpgrade : EventData
    {
        public int free_upgrades = 1;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            logic.WorldData.state = WorldState.Upgrade;
            logic.WorldData.ResetActionCompleted();

            foreach(Champion champ in logic.WorldData.champions)
                champ.free_upgrades = free_upgrades;
        }

    }
}
