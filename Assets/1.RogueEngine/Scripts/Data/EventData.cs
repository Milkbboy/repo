using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    public class EventData : ScriptableObject
    {
        public string id;
        public int depth_min = 0;
        public int depth_max = 0;
        public Sprite icon;

        public static List<EventData> evt_list = new List<EventData>();

        public virtual bool AreEventsConditionMet(World world, Champion triggerer)
        {
            return true;
        }

        public virtual void DoEvent(WorldLogic logic, Champion triggerer)
        {

        }

        public virtual string GetText()
        {
            return null;
        }

        public virtual bool HasText()
        {
            return !string.IsNullOrEmpty(GetText());
        }

        public T Get<T>() where T : EventData
        {
            return this as T;
        }

        public static void Load(string folder = "")
        {
            if (evt_list.Count == 0)
                evt_list.AddRange(Resources.LoadAll<EventData>(folder));
        }

        public static EventData Get(string id)
        {
            foreach (EventData evt in GetAll())
            {
                if (evt.id == id)
                    return evt;
            }
            return null;
        }

        public static List<EventData> GetAll()
        {
            return evt_list;
        }
    }

}
