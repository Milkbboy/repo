using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Defines all possible intent (notice) on cards
    /// </summary>
    
    [CreateAssetMenu(fileName = "IntentData", menuName = "TcgEngine/IntentData", order = 7)]
    public class IntentData : ScriptableObject
    {
        public string id;

        [Header("Intent")]
        public bool show_value;
        public int priority;

        [Header("Display")]
        public string title;
        public Sprite icon;
        [TextArea(3, 5)]
        public string text;

        public static List<IntentData> intent_list = new List<IntentData>();

        public static void Load(string folder = "")
        {
            if(intent_list.Count == 0)
                intent_list.AddRange(Resources.LoadAll<IntentData>(folder));
        }

        public string GetTitle()
        {
            return title;
        }

        public string GetText()
        {
            return text;
        }

        public static IntentData Get(string id)
        {
            foreach (IntentData intent in GetAll())
            {
                if (intent.id == id)
                    return intent;
            }
            return null;
        }

        public static List<IntentData> GetAll()
        {
            return intent_list;
        }
    }
}