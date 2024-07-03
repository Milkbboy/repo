using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    [CreateAssetMenu(fileName = "champion", menuName = "ERang/ChampionData")]
    public class Champion : ScriptableObject
    {
        [Header("Champion Info")]
        public string uid;
        public int hp;

        [Header("Cards")]
        public CardData[] start_cards;

        public static List<Champion> champ_list = new List<Champion>();
        public static Dictionary<string, Champion> champ_dict = new Dictionary<string, Champion>();

        public static void Load(string folder = "")
        {
            if (champ_list.Count == 0)
            {
                champ_list.AddRange(Resources.LoadAll<Champion>(folder));

                foreach (Champion champ in champ_list)
                {
                    champ_dict.Add(champ.uid, champ);

                    Debug.Log("Champion loaded: " + champ.uid);
                }
            }
        }
    }
}