using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Defines all champions data
    /// </summary>

    [CreateAssetMenu(fileName = "champion", menuName = "TcgEngine/ChampionData", order = 5)]
    public class ChampionData : ScriptableObject
    {
        public string id;

        [Header("Display")]
        public string title;
        public Sprite art_full;
        public Sprite art_portrait;
        public GameObject prefab;

        [Header("Stats")]
        public int hp;      //Reach 0 and die
        public int speed;   //Determines turn order
        public int hand;    //Number of cards in drawn in hand each turn
        public int energy;  //Number of mana to spend each turn

        [Header("Level Up")]
        public int level_up_hp;
        public float level_up_speed;
        public float level_up_hand;
        public float level_up_energy;

        [Header("Traits")]
        public TeamData team;
        public TraitData[] traits;
        public TraitStat[] stats;

        [Header("Abilities")]
        public AbilityData[] abilities;

        [Header("Cards")]
        public CardData[] start_cards;
        public CardData[] start_items;
        public TeamData[] reward_cards;

        [Header("Description")]
        [TextArea(5, 10)]
        public string desc;

        public static List<ChampionData> champ_list = new List<ChampionData>();

        public static void Load(string folder = "")
        {
            if (champ_list.Count == 0)
                champ_list.AddRange(Resources.LoadAll<ChampionData>(folder));
        }

        public Sprite GetFullArt()
        {
            return art_full;
        }

        public Sprite GetPortraitArt()
        {
            return art_portrait;
        }

        public GameObject GetPrefab()
        {
            return prefab;
        }

        public string GetTitle()
        {
            return title;
        }

        public string GetDesc()
        {
            return desc;
        }

        public string GetAbilitiesDesc()
        {
            string txt = "";
            foreach (AbilityData ability in abilities)
            {
                if (!string.IsNullOrWhiteSpace(ability.desc))
                    txt += "<b>" + ability.GetTitle() + ":</b> " + ability.GetDesc(this) + "\n";
            }
            return txt;
        }

        public List<CardData> GetRewardCards(Player player, RarityData rarity)
        {
            List<CardData> cards = new List<CardData>();
            foreach (TeamData team in reward_cards)
            {
                cards.AddRange(CardData.GetRewardCards(player.udata, team, rarity));
            }
            return cards;
        }

        public List<CardData> GetLockedCards(Player player)
        {
            List<CardData> cards = new List<CardData>();
            foreach (TeamData team in reward_cards)
            {
                cards.AddRange(CardData.GetLockedCards(player.udata, team));
            }
            return cards;
        }

        public static ChampionData Get(string id)
        {
            foreach (ChampionData champ in champ_list)
            {
                if (champ.id == id)
                    return champ;
            }
            return null;
        }

        public static List<ChampionData> GetAll()
        {
            return champ_list;
        }
    }
}