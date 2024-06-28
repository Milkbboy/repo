using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "tip", menuName = "TcgEngine/TipData", order = 4)]
    public class TipData : ScriptableObject
    {
        public EffectData[] effects;

        [Header("Text")]
        public string title;
        [TextArea(5, 7)]
        public string desc;

        public static List<TipData> tip_list = new List<TipData>();

        public bool HasEffect(EffectData effect)
        {
            foreach (EffectData eff in effects)
            {
                if (eff == effect)
                    return true;
            }
            return false;
        }

        public static void Load(string folder = "")
        {
            if (tip_list.Count == 0)
                tip_list.AddRange(Resources.LoadAll<TipData>(folder));
        }

        public static TipData Get(EffectData effect)
        {
            foreach (TipData team in GetAll())
            {
                if (team.HasEffect(effect))
                    return team;
            }
            return null;
        }

        public static List<TipData> GetAll()
        {
            return tip_list;
        }

    }


}
