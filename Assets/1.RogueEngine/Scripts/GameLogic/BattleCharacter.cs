using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Represent any character (champion, enemy, allies) during a battle.

    [System.Serializable]
    public class BattleCharacter
    {
        public string character_id;
        public int player_id;
        public string uid;
        public bool is_champion;
        public int level;

        public Slot slot;
        public int turn = 0;
        public int damage = 0;
        public int mana_spent = 0;
        public int initiative = 0;
        public bool is_dead = false;

        public int hp;
        public int shield;
        public int mana;

        public int speed;
        public int hand;
        public int energy;

        public int speed_ongoing = 0;
        public int hand_ongoing = 0;
        public int hp_ongoing = 0;

        public int delayed_shield = 0;
        public int delayed_energy = 0;
        public int delayed_hand = 0;

        public Dictionary<string, Card> cards_all = new Dictionary<string, Card>(); //Dictionnary for quick access to any card by UID
        public List<Card> cards_deck = new List<Card>();    //Cards in the player's deck
        public List<Card> cards_hand = new List<Card>();    //Cards in the player's hand
        public List<Card> cards_discard = new List<Card>(); //Cards in the player's discard
        public List<Card> cards_void = new List<Card>();   //Exhausted cards
        public List<Card> cards_power = new List<Card>();   //Active powers
        public List<Card> cards_item = new List<Card>();   //Inventory Items
        public List<Card> cards_temp = new List<Card>();    //Temporary cards that have just been created, not assigned to any zone yet

        public List<CardTrait> traits = new List<CardTrait>();              //Current persistant traits the cards has
        public List<CardTrait> ongoing_traits = new List<CardTrait>();      //Current ongoing traits the cards has

        public List<CardStatus> status = new List<CardStatus>();    //Current persistant (or with duration) traits the cards has
        public List<CardStatus> ongoing_status = new List<CardStatus>();    //Current ongoing traits the cards has

        public List<string> abilities = new List<string>();
        public List<string> abilities_ongoing = new List<string>();

        public List<ActionHistory> history_list = new List<ActionHistory>();  //History of actions performed by the player

        [System.NonSerialized] private CharacterData edata = null;
        [System.NonSerialized] private ChampionData cdata = null;
        [System.NonSerialized] private List<AbilityData> abilities_data = null;
        [System.NonSerialized] private int hash = 0;

        public BattleCharacter() { }
        public BattleCharacter(int id, string uid) { this.player_id = id; this.uid = uid; }

        public virtual void Refresh() { mana_spent = 0; }
        public virtual void ClearOngoing() { ongoing_status.Clear(); ongoing_traits.Clear(); speed_ongoing = 0; hand_ongoing = 0; hp_ongoing = 0; }

        //---- Stats ---------

        public virtual void SetCharacter(Champion champ)
        {
            cdata = champ.ChampionData;
            edata = null;
            character_id = champ.character_id;
            hp = champ.GetHPMax();
            damage = champ.GetDamage();
            speed = champ.GetSpeed();
            hand = champ.GetHand();
            energy = champ.GetEnergy();
            level = champ.level;
            is_champion = true;
            SetTraits(cdata);
            SetAbilities(cdata);
        }

        public virtual void SetCharacter(CharacterData charact)
        {
            cdata = null;
            edata = charact;
            character_id = charact.id;
            hp = charact.GetHP(level);
            speed = charact.GetSpeed(level);
            hand = charact.GetHand(level);
            energy = charact.GetEnergy(level);
            is_champion = false;
            SetTraits(charact);
            SetAbilities(charact);
        }

        public virtual void SetScenarioDifficulty(ScenarioData scenario)
        {
            if(scenario.enemy_hp_percentage > 0.001f)
                hp = Mathf.RoundToInt(hp * scenario.enemy_hp_percentage);
        }

        public void SetTraits(ChampionData champ)
        {
            traits.Clear();
            foreach (TraitData trait in champ.traits)
                SetTrait(trait.id, 0);
            foreach (TraitStat stat in champ.stats)
                SetTrait(stat.trait.id, stat.value);
        }

        public void SetTraits(CharacterData charact)
        {
            traits.Clear();
            foreach (TraitData trait in charact.traits)
                SetTrait(trait.id, 0);
            foreach (TraitStat stat in charact.stats)
                SetTrait(stat.trait.id, stat.value);
        }

        public void SetAbilities(ChampionData champ)
        {
            abilities.Clear();
            abilities_ongoing.Clear();
            if (abilities_data != null)
                abilities_data.Clear();
            foreach (AbilityData ability in champ.abilities)
                AddAbility(ability);
        }

        public void SetAbilities(CharacterData charact)
        {
            abilities.Clear();
            abilities_ongoing.Clear();
            if (abilities_data != null)
                abilities_data.Clear();
            foreach (AbilityData ability in charact.abilities)
                AddAbility(ability);
        }

        public virtual int GetHP() { return Mathf.Max(hp + hp_ongoing - damage, 0); }
        public virtual int GetHPMax() { return Mathf.Max(hp + hp_ongoing, 0); }
        public virtual int GetDamage() { return damage; }
        public virtual int GetShield() { return Mathf.Max(shield, 0); }
        public virtual int GetMana() { return Mathf.Max(mana - mana_spent, 0); }
        public virtual int GetManaMax() { return Mathf.Max(mana, 0); }

        public virtual int GetSpeed() { return Mathf.Max(speed + speed_ongoing, 1); }
        public virtual int GetHand() { return Mathf.Max(hand + hand_ongoing, 0); }
        public virtual int GetEnergy() { return Mathf.Max(energy, 0); }

        public virtual bool IsEnemy()
        {
            return player_id < 0; //Enemies have player_id -1, players have player_id 0,1,2,3
        }

        public virtual int GetImportance()
        {
            int val = is_champion ? 10 : 0;
            return val + level + GetHP() / 4;
        }

        //---- Cards ---------

        public void AddCard(List<Card> card_list, Card card)
        {
            card_list.Add(card);
        }

        public void RemoveCard(List<Card> card_list, Card card)
        {
            card_list.Remove(card);
        }

        public virtual void RemoveCardFromAllGroups(Card card)
        {
            cards_deck.Remove(card);
            cards_hand.Remove(card);
            cards_power.Remove(card);
            cards_discard.Remove(card);
            cards_item.Remove(card);
            cards_temp.Remove(card);
            cards_void.Remove(card);
        }
        
        public virtual Card GetRandomCard(List<Card> card_list, System.Random rand)
        {
            if (card_list.Count > 0)
                return card_list[rand.Next(0, card_list.Count)];
            return null;
        }

        public bool HasCard(List<Card> card_list, Card card)
        {
            return card_list.Contains(card);
        }

        public Card GetHandCard(string uid)
        {
            foreach (Card card in cards_hand)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetDeckCard(string uid)
        {
            foreach (Card card in cards_deck)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetDiscardCard(string uid)
        {
            foreach (Card card in cards_discard)
            {
                if (card.uid == uid)
                    return card;
            }
            return null;
        }

        public Card GetItem(string item_uid)
        {
            foreach (Card item in cards_item)
            {
                if (item.uid == item_uid)
                    return item;
            }
            return null;
        }

        public Card GetCard(string uid)
        {
            if (uid != null)
            {
                bool valid = cards_all.TryGetValue(uid, out Card card);
                if (valid)
                    return card;
            }
            return null;
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

        public List<CardTrait> GetAllTraits()
        {
            List<CardTrait> all_traits = new List<CardTrait>();
            all_traits.AddRange(traits);
            all_traits.AddRange(ongoing_traits);
            return all_traits;
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
            foreach (CardTrait trait in traits)
            {
                if (trait.id == id)
                    return true;
            }
            return false;
        }

        //---- Status ---------

        public void AddStatus(StatusData sdata, int value)
        {
            if (sdata != null && sdata.effect != StatusEffect.None)
            {
                if (sdata.is_negative && HasStatus(StatusEffect.StatusResistance))
                    return;

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
                if (sdata.is_negative && HasStatus(StatusEffect.StatusResistance))
                    return;

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

        public void ReduceStatus(StatusEffect effect, int val)
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (status[i].effect == effect)
                {
                    status[i].value -= val;

                    if (status[i].value <= 0)
                        status.RemoveAt(i);
                }
            }
        }

        public void RemoveStatus(StatusEffect effect)
        {
            for (int i = status.Count - 1; i >= 0; i--)
            {
                if (status[i].effect == effect)
                    status.RemoveAt(i);
            }
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

        public virtual int GetStatusValue(StatusEffect effect)
        {
            int value = 0;
            foreach (CardStatus status in status)
            {
                if(status.effect == effect)
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

        //---- History ---------

        public void AddHistory(ushort type, Card card)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = card.uid;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, Card card, Card target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = card.uid;
            order.target_uid = target.uid;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, BattleCharacter character, Slot target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.character_uid = character.uid;
            order.slot = target;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, Card card, BattleCharacter target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = target.uid;
            order.target_uid = target.uid;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, Card card, AbilityData ability)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = card.uid;
            order.ability_id = ability.id;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, Card card, AbilityData ability, Card target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = card.uid;
            order.ability_id = ability.id;
            order.target_uid = target.uid;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, Card card, AbilityData ability, BattleCharacter target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = card.uid;
            order.ability_id = ability.id;
            order.target_uid = target.uid;
            history_list.Add(order);
        }

        public void AddHistory(ushort type, Card card, AbilityData ability, Slot target)
        {
            ActionHistory order = new ActionHistory();
            order.type = type;
            order.card_uid = card.uid;
            order.ability_id = ability.id;
            order.slot = target;
            history_list.Add(order);
        }

        //---- Action Check ---------

        public virtual bool CanTriggerAbilities()
        {
            return !HasStatus(StatusEffect.Sleep) && !HasStatus(StatusEffect.Stunned);
        }

        public virtual bool CanPayMana(Card card)
        {
            return mana >= card.GetMana();
        }

        public virtual void PayMana(Card card)
        {
            mana -= card.GetMana();
        }

        public virtual bool IsAlly()
        {
            return player_id > 0; //0 is AI player
        }

        public virtual bool CanPlayTurn()
        {
            return !IsDead() && !HasStatus(StatusEffect.Sleep) && !HasStatus(StatusEffect.Stunned);
        }

        public virtual bool IsDead()
        {
            return is_dead;
        }

        //-----------------

        public GameObject GetPrefab()
        {
            if (is_champion)
                return ChampionData.prefab;
            else
                return CharacterData.prefab;
        }

        public ChampionData ChampionData
        {
            get
            {
                if (cdata == null || cdata.id != character_id)
                    cdata = ChampionData.Get(character_id); //Optimization, store for future use
                return cdata;
            }
        }

        public CharacterData CharacterData
        {
            get
            {
                if (edata == null || edata.id != character_id)
                    edata = CharacterData.Get(character_id); //Optimization, store for future use
                return edata;
            }
        }

        public int Hash
        {
            get
            {
                if (hash == 0)
                    hash = Mathf.Abs(uid.GetHashCode()); //Optimization, store for future use
                return hash;
            }
        }

        //--------------------

        public static BattleCharacter CloneNew(BattleCharacter source)
        {
            BattleCharacter character = new BattleCharacter();
            Clone(source, character);
            return character;
        }

        //Clone all player variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(BattleCharacter source, BattleCharacter dest)
        {
            dest.character_id = source.character_id;
            dest.uid = source.uid;
            dest.player_id = source.player_id;
            
            dest.damage = source.damage;
            dest.mana_spent = source.mana_spent;

            dest.hp = source.hp;
            dest.mana = source.mana;
            dest.shield = source.shield;

            dest.speed = source.speed;
            dest.energy = source.energy;
            dest.hand = source.hand;

            dest.speed_ongoing = source.speed_ongoing;
            dest.hand_ongoing = source.hand_ongoing;
            dest.hp_ongoing = source.hp_ongoing;

            dest.delayed_shield = source.delayed_shield;
            dest.delayed_energy = source.delayed_energy;
            dest.delayed_hand = source.delayed_hand;

            Card.CloneDict(source.cards_all, dest.cards_all);
            Card.CloneListRef(dest.cards_all, source.cards_hand, dest.cards_hand);
            Card.CloneListRef(dest.cards_all, source.cards_deck, dest.cards_deck);
            Card.CloneListRef(dest.cards_all, source.cards_discard, dest.cards_discard);
            Card.CloneListRef(dest.cards_all, source.cards_temp, dest.cards_temp);

            CardStatus.CloneList(source.status, dest.status);
            CardStatus.CloneList(source.ongoing_status, dest.ongoing_status);
        }

        public static void CloneList(List<BattleCharacter> source, List<BattleCharacter> dest)
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

        public static BattleCharacter Create(Champion champion)
        {
            BattleCharacter character = new BattleCharacter();
            character.character_id = champion.character_id;
            character.uid = champion.uid;
            character.player_id = champion.player_id;
            character.SetCharacter(champion);
            return character;
        }

        public static BattleCharacter Create(CharacterData charact, int level = 1)
        {
            BattleCharacter character = new BattleCharacter();
            character.character_id = charact.id;
            character.uid = GameTool.GenerateRandomID();
            character.player_id = -1; //Enemy player
            character.level = Mathf.Clamp(level, 1, charact.level_max);
            character.SetCharacter(charact);
            return character;
        }

        public static BattleCharacter Create(CharacterData charact, string uid, int level = 1)
        {
            BattleCharacter character = new BattleCharacter();
            character.character_id = charact.id;
            character.uid = uid;
            character.level = Mathf.Clamp(level, 1, charact.level_max);
            character.SetCharacter(charact);
            return character;
        }
    }

    [System.Serializable]
    public class ActionHistory
    {
        public ushort type;
        public string card_uid;
        public string character_uid;
        public string target_uid;
        public string ability_id;
        public Slot slot;
    }
}