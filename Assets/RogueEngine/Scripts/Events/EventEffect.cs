using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Effect", order = 10)]
    public class EventEffect : EventData
    {
        [Header("Conditions")]
        public ConditionData[] conditions_trigger;

        [Header("Target")]
        public EventTarget target;

        [Header("Effects")]
        public EffectData[] effects;
        public int value;

        [Header("Chain Event")]
        public EventData chain_event;

        [Header("Text")]
        [TextArea(5, 8)]
        public string text;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            foreach (ConditionData condition in conditions_trigger)
            {
                if (!condition.IsMapEventConditionMet(world, this, champion))
                    return false;
            }
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion triggerer)
        {
            if (target == EventTarget.ChampionActive)
            {
                foreach (EffectData effect in effects)
                    effect.DoMapEventEffect(logic, this, triggerer);
            }

            if (target == EventTarget.ChampionAll)
            {
                foreach (Champion champion in logic.WorldData.champions)
                {
                    foreach (EffectData effect in effects)
                        effect.DoMapEventEffect(logic, this, triggerer);
                }
            }

            if (!string.IsNullOrEmpty(text))
                logic.TriggerTextEvent(triggerer, text);

            if (chain_event != null)
                logic.TriggerEventNext(triggerer, chain_event);
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
    
    public enum EventTarget
    {
        None = 0,
        ChampionActive = 10,
        ChampionAll = 15,
    }
}
