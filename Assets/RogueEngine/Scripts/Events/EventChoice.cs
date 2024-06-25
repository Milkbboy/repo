using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Choice", order = 10)]
    public class EventChoice : EventData
    {
        [Header("Text")]
        [TextArea(5,8)]
        public string text;

        [Header("Choices")]
        public ChoiceElement[] choices;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            foreach (ChoiceElement choice in choices)
            {
                if (choice.effect.AreEventsConditionMet(world, champion))
                    return true;
            }
            return false; //None of the choice can be selected
        }

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            logic.WorldData.state = WorldState.EventChoice; //Wait for choice selected
        }

        public override string GetText()
        {
            return text;
        }

        public static new EventChoice Get(string id)
        {
            foreach (EventData evt in GetAll())
            {
                if (evt.id == id && evt is EventChoice)
                    return evt as EventChoice;
            }
            return null;
        }
    }

    [System.Serializable]
    public class ChoiceElement
    {
        public string text;
        public string subtext;
        public EventData effect;
    }
}
