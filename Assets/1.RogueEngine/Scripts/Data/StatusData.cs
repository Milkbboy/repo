using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{

    public enum StatusEffect
    {
        None = 0,

        ManaCostBonus = 2,       //Cost increase/reduction
        SpeedBonus = 7,      //Speed bonuss
        HandBonus = 8,      //Hand size bonus

        AttackPower = 10,     //Damage bonus for all cards
        MagicPower = 12,     //Damage bonus for all cards
        Armor = 15,         //Shield per turn

        Stunned = 20,          //Cant do any actions for X turns
        Sleep = 22,          //Cant do any actions until attacked

        Courageous = 30,    //Deal 50% more damage
        Fearful =31,        //Deal 50% less damage
        Vulnerable = 32,          //Receive 50% more damage
        Evasive = 33,       //Receive 50% less damage

        Poisoned = 40,     //Lose hp each start of turn, ignoring shield
        Burned = 42,         //Lose hp each start of turn, shield protects

        StatusResistance = 50,   //Immune to negative status
        BurnHeal = 52,          //Burn damage heal this character
        Thorn = 55,         //When attacked, deal damage to attacker

        Keep = 60,         //Card isn't discarded from hand at end of turn

    }

    public enum StatusDuration
    {
        Persistant = 0,
        AutoReduce = 10,
        OneTurn = 20,
    }

    /// <summary>
    /// Defines all status effects data
    /// Status are effects that can be gained or lost with abilities, and that will affect gameplay
    /// Status can have a duration
    /// </summary>

    [CreateAssetMenu(fileName = "status", menuName = "TcgEngine/StatusData", order = 7)]
    public class StatusData : ScriptableObject
    {
        public string id;
        public StatusEffect effect;
        public StatusDuration duration;
        public bool is_negative;       //Negative effects wont be applied to characters with SpellResistance

        [Header("Display")]
        public string title;
        public Sprite icon;

        [TextArea(3, 5)]
        public string desc;

        [Header("FX")]
        public GameObject status_fx;
        public string animation;

        public static List<StatusData> status_list = new List<StatusData>();

        public string GetTitle()
        {
            return title;
        }

        public string GetDesc()
        {
            return GetDesc(1);
        }

        public string GetDesc(int value)
        {
            string des = desc.Replace("<value>", value.ToString());
            return des;
        }

        public static void Load(string folder = "")
        {
            if (status_list.Count == 0)
                status_list.AddRange(Resources.LoadAll<StatusData>(folder));
        }

        public static StatusData Get(string id)
        {
            foreach (StatusData status in GetAll())
            {
                if (status.id == id)
                    return status;
            }
            return null;
        }

        public static StatusData Get(StatusEffect effect)
        {
            foreach (StatusData status in GetAll())
            {
                if (status.effect == effect)
                    return status;
            }
            return null;
        }

        public static List<StatusData> GetAll()
        {
            return status_list;
        }
    }
}