using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Defines all rarities data (common, uncommon, rare, mythic)
    /// </summary>

    [CreateAssetMenu(fileName = "RarityData", menuName = "TcgEngine/RarityData", order = 1)]
    public class RarityData : ScriptableObject
    {
        public string id;
        public string title;
        public Sprite icon;

        [Header("Reward probability")]
        public float probability = 1f;

        public static List<RarityData> rarity_list = new List<RarityData>();

        public static void Load(string folder = "")
        {
            if (rarity_list.Count == 0)
                rarity_list.AddRange(Resources.LoadAll<RarityData>(folder));
        }

        public static RarityData Get(string id)
        {
            foreach (RarityData rarity in GetAll())
            {
                if (rarity.id == id)
                    return rarity;
            }
            return null;
        }

        public static List<RarityData> GetAll()
        {
            return rarity_list;
        }

        public static float GetTotalProbability()
        {
            float total = 0f;
            foreach (RarityData rarity in GetAll())
            {
                total += rarity.probability;
            }
            return total;
        }
    }
}