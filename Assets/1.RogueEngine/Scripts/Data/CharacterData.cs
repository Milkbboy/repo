using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Defines all card data
    /// </summary>

    [CreateAssetMenu(fileName = "character", menuName = "TcgEngine/CharacterData", order = 5)]
    public class CharacterData : ScriptableObject
    {
        public string id;

        [Header("Display")]
        public string title;
        public Sprite art_full;
        public Sprite art_portrait;
        public GameObject prefab;

        [Header("Stats")]
        public int hp;
        public int speed;
        public int hand;     
        public int energy;

        [Header("Level Up")]
        public int level_max = 1;
        public int level_up_hp;
        public float level_up_speed;
        public float level_up_hand;
        public float level_up_energy;

        [Header("Behavior")]
        public BehaviorData behavior;

        [Header("Traits")]
        public TraitData[] traits;
        public TraitStat[] stats;

        [Header("Abilities")]
        public AbilityData[] abilities;

        [Header("Cards")]
        public CardData[] cards;

        [Header("Reward")]
        public int reward_gold;
        public int reward_xp;

        [Header("Description")]
        [TextArea(5, 10)]
        public string desc;

        [Header("FX")]
        public GameObject spawn_fx;
        public GameObject death_fx;
        public AudioClip spawn_audio;
        public AudioClip death_audio;

        public static List<CharacterData> character_list = new List<CharacterData>();

        public virtual int GetHP(int level) { return Mathf.RoundToInt(hp + (level-1) * level_up_hp); }
        public virtual int GetSpeed(int level) { return Mathf.RoundToInt(speed + (level - 1) * level_up_speed); }
        public virtual int GetHand(int level) { return Mathf.RoundToInt(hand + (level - 1) * level_up_hand); }
        public virtual int GetEnergy(int level) { return Mathf.RoundToInt(energy + (level - 1) * level_up_energy); }

        public static void Load(string folder = "")
        {
            if (character_list.Count == 0)
                character_list.AddRange(Resources.LoadAll<CharacterData>(folder));
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

        public static CharacterData Get(string id)
        {
            foreach (CharacterData champ in character_list)
            {
                if (champ.id == id)
                    return champ;
            }
            return null;
        }

        public static List<CharacterData> GetAll()
        {
            return character_list;
        }
    }
}