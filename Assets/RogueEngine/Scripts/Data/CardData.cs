using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RogueEngine
{
    
    public enum CardType
    {
        None = 0,
        Skill = 20,     //One time effect, then discarded
        Power = 30,    //Stays in play for the rest of battle
    }

    public enum ItemType
    {
        None = 0,               //Not in inventory
        ItemQuest = 20,         //In your inventory, but not used in battles
        ItemConsumable = 22,    //In your inventory, and can be consumed during battles
        ItemCard = 24,          //In your inventory, and added to your deck as a card during battles
        ItemPassive = 26,       //In your inventory, and gives ongoing bonus during battles
    }

    /// <summary>
    /// Defines all card data
    /// </summary>

    [CreateAssetMenu(fileName = "card", menuName = "TcgEngine/CardData", order = 5)]
    public class CardData : ScriptableObject
    {
        public string id;

        [Header("Display")]
        public string title;
        public Sprite art_icon;
        public Sprite art_full;

        [Header("Stats")]
        public CardType card_type;
        public ItemType item_type;
        public TeamData team;
        public RarityData rarity;
        public int mana;

        [Header("Traits")]
        public TraitData[] traits;
        public TraitStat[] stats;

        [Header("Abilities")]
        public AbilityData[] abilities;

        [Header("Upgrades")]
        public int level_max = 1;
        public int upgrade_mana;

        [Header("Shop")]
        public int cost = 100;

        [Header("Intent")]
        public IntentData intent;

        [Header("Card Text")]
        [TextArea(3, 5)]
        public string text;

        [TextArea(3, 5)]
        public string upg_text;

        [Header("FX")]
        public GameObject spawn_fx;
        public AudioClip spawn_audio;
        public string caster_anim;
        public string target_anim;

        [Header("Availability")]
        public CardAvailability availability;

        public static List<CardData> card_list = new List<CardData>();                              //Faster access in loops
        public static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();    //Faster access in Get(id)

        public static void Load(string folder = "")
        {
            if (card_list.Count == 0)
            {
                card_list.AddRange(Resources.LoadAll<CardData>(folder));

                foreach (CardData card in card_list)
                    card_dict.Add(card.id, card);
            }
        }

        public Sprite GetIconArt()
        {
            return art_icon;
        }

        public Sprite GetFullArt()
        {
            return art_full;
        }

        public string GetTitle()
        {
            return title;
        }

        public string GetText(int level = 1, int value_size = 42)
        {
            string dsc = text;
            if (!string.IsNullOrWhiteSpace(upg_text) && level > 1)
                dsc = upg_text;

            dsc = dsc.Replace("<name>", title);
            dsc = dsc.Replace("<level>", level.ToString());
            dsc = dsc.Replace("<value>", "<size=" + value_size + ">" + GetValue(0, level).ToString() + "</size>");
            dsc = dsc.Replace("<value0>", "<size=" + value_size + ">" + GetValue(0, level).ToString() + "</size>");
            dsc = dsc.Replace("<value1>", "<size=" + value_size + ">" + GetValue(1, level).ToString() + "</size>");
            dsc = dsc.Replace("<value2>", "<size=" + value_size + ">" + GetValue(2, level).ToString() + "</size>");
            dsc = dsc.Replace("<size>", "<size=" + value_size + ">");
            return dsc;
        }

        public string GetText(BattleCharacter caster, Card card, int value_size = 42)
        {
            string dsc = text;
            if (!string.IsNullOrWhiteSpace(upg_text) && card.level > 1)
                dsc = upg_text;

            dsc = dsc.Replace("<name>", title);
            dsc = dsc.Replace("<level>", card.level.ToString());
            dsc = dsc.Replace("<value>", "<size=" + value_size + ">" + GetValue(0, caster, card).ToString() + "</size>");
            dsc = dsc.Replace("<value0>", "<size=" + value_size + ">" + GetValue(0, caster, card).ToString() + "</size>");
            dsc = dsc.Replace("<value1>", "<size=" + value_size + ">" + GetValue(1, caster, card).ToString() + "</size>");
            dsc = dsc.Replace("<value2>", "<size=" + value_size + ">" + GetValue(2, caster, card).ToString() + "</size>");
            dsc = dsc.Replace("<size>", "<size=" + value_size + ">");
            return dsc;
        }

        public int GetValue(int ability_index, int level)
        {
            if (ability_index >= 0 && ability_index < abilities.Length)
            {
                AbilityData ability = abilities[ability_index];
                return ability.GetValue(level);
            }
            return 0;
        }

        public int GetValue(int ability_index, BattleCharacter caster, Card card)
        {
            if (ability_index >= 0 && ability_index < abilities.Length)
            {
                AbilityData ability = abilities[ability_index];
                return ability.GetValue(caster, card);
            }
            return 0;
        }

        public int GetUpgradeCost(int level)
        {
            if (level_max > 3)
                return level > 3 ? 100 : 50;
            else if(level_max == 3)
                return level > 2 ? 100 : 50;
            else
                return 100;
        }

        public int GetMana(int level)
        {
            return mana + (level - 1) * upgrade_mana;
        }

        public string GetPlayTypeId()
        {
            if (card_type == CardType.Skill)
                return "spell";
            if (card_type == CardType.Power)
                return "power";
            return "";
        }

        public string GetTraitText()
        {
            string txt = "";
            foreach (TraitData trait in traits)
            {
                if (trait != null && !string.IsNullOrEmpty(trait.title))
                {
                    txt += trait.GetTitle() + ", ";
                }
            }
            if (txt.Length > 2)
                txt = txt.Substring(0, txt.Length - 2);
            return txt;
        }

        public string GetTips()
        {
            string txt = "";
            foreach (AbilityData ability in abilities)
            {
                foreach (EffectData effect in ability.effects)
                {
                    TipData tip = TipData.Get(effect);
                    if (tip != null && !string.IsNullOrWhiteSpace(tip.desc))
                        txt += "<b>" + tip.title + ":</b> " + tip.desc + "\n";
                }
                foreach (StatusData status in ability.status)
                {
                    if (status != null && !string.IsNullOrWhiteSpace(status.desc))
                        txt += "<b>" + status.title + ":</b> " + status.desc + "\n";
                }
            }
            return txt;
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

        public bool IsItem()
        {
            return item_type != ItemType.None;
        }

        public bool IsCard()
        {
            return card_type != CardType.None;
        }

        public bool IsBoardCard()
        {
            return card_type == CardType.Power;
        }

        public bool IsRequireTarget()
        {
            return card_type == CardType.Skill && HasPlayTargetAbility();
        }

        public bool IsRequireTargetSpell(AbilityTarget target)
        {
            return card_type == CardType.Skill && HasPlayTargetAbility(target);
        }

        public bool HasTrait(string trait)
        {
            foreach (TraitData t in traits)
            {
                if (t.id == trait)
                    return true;
            }
            return false;
        }

        public bool HasTrait(TraitData trait)
        {
            if(trait != null)
                return HasTrait(trait.id);
            return false;
        }

        public bool HasStat(string trait)
        {
            if (stats == null)
                return false;

            foreach (TraitStat stat in stats)
            {
                if (stat.trait.id == trait)
                    return true;
            }
            return false;
        }

        public bool HasStat(TraitData trait)
        {
            if(trait != null)
                return HasStat(trait.id);
            return false;
        }

        public int GetStat(string trait_id)
        {
            if (stats == null)
                return 0;

            foreach (TraitStat stat in stats)
            {
                if (stat.trait.id == trait_id)
                    return stat.value;
            }
            return 0;
        }

        public int GetStat(TraitData trait)
        {
            if(trait != null)
                return GetStat(trait.id);
            return 0;
        }

        public bool HasAbility(AbilityData tability)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.id == tability.id)
                    return true;
            }
            return false;
        }

        public bool HasAbility(AbilityTrigger trigger)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == trigger)
                    return true;
            }
            return false;
        }

        public bool HasAbility(AbilityTrigger trigger, AbilityTarget target)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == trigger && ability.target == target)
                    return true;
            }
            return false;
        }

        public bool HasEffect<T>(AbilityTrigger trigger) where T : EffectData
        {
            foreach (AbilityData iability in abilities)
            {
                if (iability.trigger == trigger && iability.HasEffect<T>())
                    return true;
            }
            return false;
        }

        public bool HasPlayTargetAbility()
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.IsPlayTarget())
                    return true;
            }
            return false;
        }

        public bool HasPlayTargetAbility(AbilityTarget target)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == target)
                    return true;
            }
            return false;
        }

        public AbilityData GetAbility(AbilityTrigger trigger)
        {
            foreach (AbilityData ability in abilities)
            {
                if (ability && ability.trigger == trigger)
                    return ability;
            }
            return null;
        }

        public bool IsAvailable(UserData udata)
        {
            return availability == CardAvailability.Available || (udata != null && udata.IsCardUnlocked(this));
        }

        public bool IsUnlockable(UserData udata)
        {
            return availability == CardAvailability.Unlockable && udata != null && !udata.IsCardUnlocked(this);
        }

        public static CardData Get(string id)
        {
            if (id == null)
                return null;
            bool success = card_dict.TryGetValue(id, out CardData card);
            if (success)
                return card;
            return null;
        }

        public static List<CardData> GetRewardCards(UserData user, TeamData team, RarityData rarity)
        {
            List<CardData> list = new List<CardData>();
            foreach (CardData acard in GetAll())
            {
                bool is_team = team == null || acard.team == team;
                bool is_rarity = rarity == null || acard.rarity == rarity;
                if (is_team && is_rarity && acard.IsCard() && acard.IsAvailable(user))
                    list.Add(acard);
            }
            return list;
        }

        public static List<CardData> GetRewardItems(UserData user, TeamData team, RarityData rarity)
        {
            List<CardData> list = new List<CardData>();
            foreach (CardData acard in GetAll())
            {
                bool is_team = team == null || acard.team == team;
                bool is_rarity = rarity == null || acard.rarity == rarity;
                if (is_team && is_rarity && acard.IsItem() && acard.IsAvailable(user))
                    list.Add(acard);
            }
            return list;
        }

        public static List<CardData> GetRewardItems(UserData user, CardData[] item_list, RarityData rarity)
        {
            List<CardData> list = new List<CardData>();
            foreach (CardData item in item_list)
            {
                bool is_rarity = rarity == null || item.rarity == rarity;
                if (is_rarity && item.IsItem() && item.IsAvailable(user))
                    list.Add(item);
            }
            return list;
        }

        public static List<CardData> GetLockedCards(UserData user, TeamData team)
        {
            List<CardData> locked = new List<CardData>();
            foreach (CardData acard in GetAll())
            {
                bool is_team = team == null || acard.team == team;
                if (is_team && acard.IsCard() && acard.IsUnlockable(user))
                    locked.Add(acard);
            }
            return locked;
        }

        public static List<CardData> GetLockedItems(UserData user, TeamData team)
        {
            List<CardData> locked = new List<CardData>();
            foreach (CardData acard in GetAll())
            {
                bool is_team = team == null || acard.team == team;
                if (is_team && acard.IsItem() && acard.IsUnlockable(user))
                    locked.Add(acard);
            }
            return locked;
        }

        public static List<CardData> GetAll()
        {
            return card_list;
        }
    }

    public enum CardAvailability
    {
        Available = 0,
        Unlockable = 10,
        Unlisted = 20,
    }
}