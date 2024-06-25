using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "ScenarioData", menuName = "TcgEngine/ScenarioData", order = 1)]
    public class ScenarioData : ScriptableObject
    {
        public string id;
        public string title;

        [Header("Description")]
        [TextArea(5, 7)]
        public string desc;

        [Header("Gameplay")]
        public int champions = 1;
        public int start_gold;
        public int start_xp;
        public CardData[] start_cards;
        public CardData[] start_items;

        [Header("Difficulty")]
        public float enemy_hp_percentage = 1f;
        public int enemy_power_add;
        public int enemy_armor_add;

        [Header("Maps")]
        public MapData[] maps;

        public static List<ScenarioData> scenario_list = new List<ScenarioData>();

        public static void Load(string folder = "")
        {
            if (scenario_list.Count == 0)
                scenario_list.AddRange(Resources.LoadAll<ScenarioData>(folder));
        }

        public static ScenarioData Get(string id)
        {
            foreach (ScenarioData scenario in GetAll())
            {
                if (scenario.id == id)
                    return scenario;
            }
            return null;
        }

        public static List<ScenarioData> GetAll()
        {
            return scenario_list;
        }
    }

}