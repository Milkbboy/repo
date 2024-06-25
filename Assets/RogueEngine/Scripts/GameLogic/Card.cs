using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Represent the current state of a card during the game (data only)

    [System.Serializable]
    public class Card
    {
        public string card_id;
        public string uid;

        public int player_id;       //Player id
        public string owner_uid;    //Character uid
        public int level;

        public int mana = 0;
        public int mana_ongoing = 0;

        public List<CardTrait> traits = new List<CardTrait>();
        public List<CardTrait> ongoing_traits = new List<CardTrait>();

        public List<CardStatus> status = new List<CardStatus>();
        public List<CardStatus> ongoing_status = new List<CardStatus>();

        public List<string> abilities = new List<string>();
        public List<string> abilities_ongoing = new List<string>();

        [System.NonSerialized] private int hash = 0;
        [System.NonSerialized] private CardData data = null;
        [System.NonSerialized] private List<AbilityData> abilities_data = null;

        public Card(string card_id, string uid, int player_id) { this.card_id = card_id; this.uid = uid; this.player_id = player_id; }

        public virtual void ClearOngoing() { ongoing_status.Clear(); ongoing_traits.Clear(); mana_ongoing = 0; }
        public virtual void Clear()
        {
            ClearOngoing(); status.Clear(); 
            SetCard(CardData, level); //Reset to initial stats
        }

        public virtual int GetMana() { return Mathf.Max(mana + mana_ongoing, 0); }

        public virtual bool IsEnemy()
        {
            return player_id == 0; //Enemies have player_id 0, players have player_id 1,2,3,4
        }

        //------ Set Stats ----

        public virtual void SetCard(CardData icard, int lvl)
        {
            data = icard;
            level = lvl;
            card_id = icard.id;
            mana = icard.GetMana(lvl);
            SetTraits(icard);
            SetAbilities(icard);
        }

        public void SetTraits(CardData icard)
        {
            traits.Clear();
            foreach (TraitData trait in icard.traits)
                SetTrait(trait.id, 0);
            if (icard.stats != null)
            {
                foreach (TraitStat stat in icard.stats)
                    SetTrait(stat.trait.id, stat.value);
            }
        }

        public void SetAbilities(CardData icard)
        {
            abilities.Clear();
            abilities_ongoing.Clear();
            if (abilities_data != null)
                abilities_data.Clear();
            foreach (AbilityData ability in icard.abilities)
                AddAbility(ability);
        }
        
        //------ Custom Traits/Stats ---------

        public void SetTrait(string id, int value)
        {
            CardTrait trait = GetTrait(id);
            if (trait != null)
            {
                trait.value = value;
            }
            else
            {
                trait = new CardTrait(id, value);
                traits.Add(trait);
            }
        }

        public void AddTrait(string id, int value)
        {
            CardTrait trait = GetTrait(id);
            if (trait != null)
                trait.value += value;
            else
                SetTrait(id, value);
        }

        public void AddOngoingTrait(string id, int value)
        {
            CardTrait trait = GetOngoingTrait(id);
            if (trait != null)
            {
                trait.value += value;
            }
            else
            {
                trait = new CardTrait(id, value);
                ongoing_traits.Add(trait);
            }
        }

        public void RemoveTrait(string id)
        {
            for (int i = traits.Count - 1; i >= 0; i--)
            {
                if (traits[i].id == id)
                    traits.RemoveAt(i);
            }
        }

        public CardTrait GetTrait(string id)
        {
            foreach (CardTrait trait in traits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        public CardTrait GetOngoingTrait(string id)
        {
            foreach (CardTrait trait in ongoing_traits)
            {
                if (trait.id == id)
                    return trait;
            }
            return null;
        }

        public int GetTraitValue(TraitData trait)
        {
            if (trait != null)
                return GetTraitValue(trait.id);
            return 0;
        }

        public virtual int GetTraitValue(string id)
        {
            int val = 0;
            CardTrait stat1 = GetTrait(id);
            CardTrait stat2 = GetOngoingTrait(id);
            if (stat1 != null)
                val += stat1.value;
            if (stat2 != null)
                val += stat2.value;
            return val;
        }

        public bool HasTrait(TraitData trait)
        {
            if (trait != null)
                return HasTrait(trait.id);
            return false;
        }

        public bool HasTrait(string id)
        {
            return GetTrait(id) != null || GetOngoingTrait(id) != null;
        }

        public List<CardTrait> GetAllTraits()
        {
            List<CardTrait> all_traits = new List<CardTrait>();
            all_traits.AddRange(traits);
            all_traits.AddRange(ongoing_traits);
            return all_traits;
        }
        
        //Alternate names since traits/stats are stored in same var
        public void SetStat(string id, int value) => SetTrait(id, value);
        public void AddStat(string id, int value) => AddTrait(id, value);
        public void AddOngoingStat(string id, int value) => AddOngoingTrait(id, value);
        public void RemoveStat(string id) => RemoveTrait(id);
        public int GetStatValue(TraitData trait) => GetTraitValue(trait);
        public int GetStatValue(string id) => GetTraitValue(id);
        public bool HasStat(TraitData trait) => HasTrait(trait);
        public bool HasStat(string id) => HasTrait(id);
        public List<CardTrait> GetAllStats() => GetAllTraits();

        //------  Status Effects ---------


        public void AddStatus(StatusData sdata, int value)
        {
            if (sdata != null && sdata.effect != StatusEffect.None)
            {
                CardStatus status = GetStatus(sdata.id);
                if (status == null)
                {
                    status = new CardStatus(sdata.id, sdata.effect, value);
                    this.status.Add(status);
                }
                else
                {
                    status.value += value;
                }
            }
        }

        public void AddOngoingStatus(StatusData sdata, int value)
        {
            if (sdata != null && sdata.effect != StatusEffect.None)
            {
                CardStatus status = GetOngoingStatus(sdata.id);
                if (status == null)
                {
                    status = new CardStatus(sdata.id, sdata.effect, value);
                    ongoing_status.Add(status);
                }
                else
                {
                    status.value += value;
                }
            }
        }

        public void RemoveStatus(StatusEffect type)
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (status[i].effect == type)
                    status.RemoveAt(i);
            }
        }

        public List<CardStatus> GetAllStatus()
        {
            List<CardStatus> all_status = new List<CardStatus>();
            all_status.AddRange(status);
            all_status.AddRange(ongoing_status);
            return all_status;
        }

        public bool HasStatus(StatusEffect effect)
        {
            return GetStatus(effect) != null || GetOngoingStatus(effect) != null;
        }

        public CardStatus GetStatus(string id)
        {
            foreach (CardStatus status in status)
            {
                if (status.id == id)
                    return status;
            }
            return null;
        }

        public CardStatus GetStatus(StatusEffect effect)
        {
            foreach (CardStatus status in status)
            {
                if (status.effect == effect)
                    return status;
            }
            return null;
        }

        public CardStatus GetOngoingStatus(string id)
        {
            foreach (CardStatus status in ongoing_status)
            {
                if (status.id == id)
                    return status;
            }
            return null;
        }

        public CardStatus GetOngoingStatus(StatusEffect effect)
        {
            foreach (CardStatus status in ongoing_status)
            {
                if (status.effect == effect)
                    return status;
            }
            return null;
        }

        public virtual int GetStatusValue(StatusEffect effect)
        {
            int value = 0;
            foreach (CardStatus status in status)
            {
                if (status.effect == effect)
                    value += status.value;
            }
            foreach (CardStatus status in ongoing_status)
            {
                if (status.effect == effect)
                    value += status.value;
            }
            return value;
        }

        public virtual void ReduceStatusValues()
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (status[i].StatusData.duration == StatusDuration.AutoReduce)
                {
                    status[i].value -= 1;
                    if (status[i].value <= 0)
                        status.RemoveAt(i);
                }
            }
        }

        public virtual void RemoveOnceStatus()
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                CardStatus astatus = status[i];
                if (astatus != null && astatus.StatusData.duration == StatusDuration.OneTurn)
                    status.RemoveAt(i);
            }
        }

        //----- Abilities ------------

        public void AddAbility(AbilityData ability)
        {
            abilities.Add(ability.id);
            if (abilities_data != null)
                abilities_data.Add(ability);
        }

        public void RemoveAbility(AbilityData ability)
        {
            abilities.Remove(ability.id);
            if (abilities_data != null)
                abilities_data.Remove(ability);
        }

        public void AddOngoingAbility(AbilityData ability)
        {
            if (!abilities_ongoing.Contains(ability.id) && !abilities.Contains(ability.id))
            {
                abilities_ongoing.Add(ability.id);
                if (abilities_data != null)
                    abilities_data.Add(ability);
            }
        }

        public void ClearOngoingAbility()
        {
            if (abilities_data != null)
            {
                for (int i = abilities_data.Count - 1; i >= 0; i--)
                {
                    AbilityData ability = abilities_data[i];
                    if (abilities_ongoing.Contains(ability.id))
                        abilities_data.RemoveAt(i);
                }
            }

            abilities_ongoing.Clear();
        }

        public AbilityData GetAbility(AbilityTrigger trigger)
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.trigger == trigger)
                    return iability;
            }
            return null;
        }

        public bool HasAbility(AbilityData ability)
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.id == ability.id)
                    return true;
            }
            return false;
        }

        public bool HasAbility(AbilityTrigger trigger)
        {
            AbilityData iability = GetAbility(trigger);
            if (iability != null)
                return true;
            return false;
        }

        public bool HasAbility(AbilityTrigger trigger, AbilityTarget target)
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.trigger == trigger && iability.target == target)
                    return true;
            }
            return false;
        }

        public bool HasEffect<T>(AbilityTrigger trigger) where T : EffectData
        {
            foreach (AbilityData iability in GetAbilities())
            {
                if (iability.trigger == trigger && iability.HasEffect<T>())
                    return true;
            }
            return false;
        }

        public bool AreAbilityConditionsMet(AbilityTrigger ability_trigger, Battle data, BattleCharacter caster, Card card)
        {
            foreach (AbilityData ability in GetAbilities())
            {
                if (ability && ability.trigger == ability_trigger && ability.AreTriggerConditionsMet(data, caster, card))
                    return true;
            }
            return false;
        }

        public List<AbilityData> GetAbilities()
        {
            //Load abilities data, important to do this here since this array will be null after being sent through networking (cant serialize it)
            if (abilities_data == null)
            {
                abilities_data = new List<AbilityData>(abilities.Count + abilities_ongoing.Count);
                for (int i = 0; i < abilities.Count; i++)
                    abilities_data.Add(AbilityData.Get(abilities[i]));
                for (int i = 0; i < abilities_ongoing.Count; i++)
                    abilities_data.Add(AbilityData.Get(abilities_ongoing[i]));
            }

            //Return
            return abilities_data;
        }

        public int GetAbilityValue(BattleCharacter character)
        {
            int highest = 0;
            foreach (AbilityData ability in GetAbilities())
            {
                int val = ability.GetValue(character, this);
                if (val > highest)
                    highest = val;
            }
            return highest;
        }

        //-------

        public CardData CardData 
        { 
            get { 
                if(data == null || data.id != card_id)
                    data = CardData.Get(card_id); //Optimization, store for future use
                return data;
            } 
        }

        public CardData Data => CardData; //Alternate name

        public int Hash
        {
            get {
                if (hash == 0)
                    hash = Mathf.Abs(uid.GetHashCode()); //Optimization, store for future use
                return hash;
            }
        }

        public static Card Create(CardData icard, int level, BattleCharacter player)
        {
            return Create(icard, level, player, GameTool.GenerateRandomID(11, 15));
        }

        public static Card Create(CardData icard, int level, BattleCharacter owner, string uid)
        {
            Card card = new Card(icard.id, uid, owner.player_id);
            card.owner_uid = owner.uid;
            card.SetCard(icard, Mathf.Min(level, icard.level_max));
            owner.cards_all[card.uid] = card;
            return card;
        }

        public static Card CloneNew(Card source)
        {
            Card card = new Card(source.card_id, source.uid, source.player_id);
            Clone(source, card);
            return card;
        }

        //Clone all card variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Card source, Card dest)
        {
            dest.card_id = source.card_id;
            dest.uid = source.uid;
            dest.player_id = source.player_id;
            dest.owner_uid = source.owner_uid;
            dest.level = source.level;
            dest.mana = source.mana;
            dest.mana_ongoing = source.mana_ongoing;

            CardTrait.CloneList(source.traits, dest.traits);
            CardTrait.CloneList(source.ongoing_traits, dest.ongoing_traits);
            CardStatus.CloneList(source.status, dest.status);
            CardStatus.CloneList(source.ongoing_status, dest.ongoing_status);
            GameTool.CloneList(source.abilities, dest.abilities); 
            GameTool.CloneList(source.abilities_ongoing, dest.abilities_ongoing); 
            GameTool.CloneListRefNull(source.abilities_data, ref dest.abilities_data); //No need to deep copy since AbilityData doesn't change dynamically, its just a reference
        }

        //Clone a var that could be null
        public static void CloneNull(Card source, ref Card dest)
        {
            //Source is null
            if (source == null)
            {
                dest = null;
                return;
            }

            //Dest is null
            if (dest == null)
            {
                dest = CloneNew(source);
                return;
            }

            //Both arent null, just clone
            Clone(source, dest);
        }

        //Clone dictionary completely
        public static void CloneDict(Dictionary<string, Card> source, Dictionary<string, Card> dest)
        {
            foreach (KeyValuePair<string, Card> pair in source)
            {
                bool valid = dest.TryGetValue(pair.Key, out Card val);
                if (valid)
                    Clone(pair.Value, val);
                else
                    dest[pair.Key] = CloneNew(pair.Value);
            }
        }

        //Clone list by keeping references from ref_dict
        public static void CloneListRef(Dictionary<string, Card> ref_dict, List<Card> source, List<Card> dest)
        {
            for (int i = 0; i < source.Count; i++)
            {
                Card scard = source[i];
                bool valid = ref_dict.TryGetValue(scard.uid, out Card rcard);
                if (valid)
                {
                    if (i < dest.Count)
                        dest[i] = rcard;
                    else
                        dest.Add(rcard);
                }
            }

            if(dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
    }

    [System.Serializable]
    public class CardStatus
    {
        public string id;
        public StatusEffect effect;
        public int value;

        [System.NonSerialized]
        private StatusData data = null;

        public CardStatus(string id, StatusEffect type, int value)
        {
            this.id = id;
            this.effect = type;
            this.value = value;
        }

        public StatusData StatusData { 
            get
            {
                if (data == null || data.effect != effect)
                    data = StatusData.Get(id);
                return data;
            }
        }

        public StatusData Data => StatusData; //Alternate name

        public static CardStatus CloneNew(CardStatus copy)
        {
            CardStatus status = new CardStatus(copy.id, copy.effect, copy.value);
            return status;
        }

        public static void Clone(CardStatus source, CardStatus dest)
        {
            dest.id = source.id;
            dest.effect = source.effect;
            dest.value = source.value;
        }

        public static void CloneList(List<CardStatus> source, List<CardStatus> dest)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < dest.Count)
                    Clone(source[i], dest[i]);
                else
                    dest.Add(CloneNew(source[i]));
            }

            if (dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
    }

    [System.Serializable]
    public class CardTrait
    {
        public string id;
        public int value;

        [System.NonSerialized]
        private TraitData data = null;

        public CardTrait(string id, int value)
        {
            this.id = id;
            this.value = value;
        }

        public CardTrait(TraitData trait, int value)
        {
            this.id = trait.id;
            this.value = value;
        }

        public TraitData TraitData
        {
            get
            {
                if (data == null || data.id != id)
                    data = TraitData.Get(id);
                return data;
            }
        }

        public TraitData Data => TraitData; //Alternate name


        public static CardTrait CloneNew(CardTrait copy)
        {
            CardTrait status = new CardTrait(copy.id, copy.value);
            return status;
        }

        public static void Clone(CardTrait source, CardTrait dest)
        {
            dest.id = source.id;
            dest.value = source.value;
        }

        public static void CloneList(List<CardTrait> source, List<CardTrait> dest)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < dest.Count)
                    Clone(source[i], dest[i]);
                else
                    dest.Add(CloneNew(source[i]));
            }

            if (dest.Count > source.Count)
                dest.RemoveRange(source.Count, dest.Count - source.Count);
        }
    }
}
