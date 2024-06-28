using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Contains all battle state data that is sync across network

    [System.Serializable]
    public class Battle
    {
        //Battle state
        public string battle_id;
        public string active_character;
        public int turn_count = 0;
        public float turn_timer = 0f;

        public BattlePhase phase = BattlePhase.None;
        public int win_result = 0;

        public List<BattleCharacter> characters = new List<BattleCharacter>();
        public List<string> initiatives = new List<string>();
        public List<string> initiatives_next = new List<string>();

        //Selector
        public SelectorType selector = SelectorType.None;
        public int selector_player_id = 0;
        public string selector_ability_id;
        public string selector_caster_uid;
        public string selector_card_uid;

        //Other reference values
        public string marked_character;
        public string last_played;
        public string last_destroyed;
        public string last_summoned;
        public string last_targeted;
        public string ability_triggerer;
        public Slot play_target;
        public int rolled_value;

        public HashSet<string> ability_played = new HashSet<string>();

        [System.NonSerialized] private ListSwap<BattleCharacter> character_list = new ListSwap<BattleCharacter>();
        [System.NonSerialized] private ListSwap<Card> card_list = new ListSwap<Card>();
        [System.NonSerialized] private ListSwap<Slot> slot_list = new ListSwap<Slot>();
        [System.NonSerialized] private ListSwap<CardData> card_data_list = new ListSwap<CardData>();

        [System.NonSerialized]
        public System.Random rand = new System.Random();

        public Battle() { }
        public Battle(string id) { battle_id = id; }

        public virtual bool IsAllyTurn()
        {
            BattleCharacter character = GetActiveCharacter();
            return character != null && !character.IsEnemy();
        }

        public virtual bool IsEnemyTurn()
        {
            BattleCharacter character = GetActiveCharacter();
            return character != null && character.IsEnemy();
        }

        public virtual bool IsPlayerTurn(int player_id)
        {
            BattleCharacter character = GetActiveCharacter();
            return character != null && character.player_id == player_id && IsCharacterTurn(character);
        }

        public virtual bool IsPlayerActionTurn(int player_id)
        {
            BattleCharacter character = GetActiveCharacter();
            return character != null && character.player_id == player_id && IsCharacterActionTurn(character);
        }

        public virtual bool IsPlayerSelectorTurn(int player_id)
        {
            BattleCharacter character = GetActiveCharacter();
            return character != null && character.player_id == player_id && IsCharacterSelectorTurn(character);
        }

        //Check if its player's turn
        public virtual bool IsCharacterTurn(BattleCharacter champ)
        {
            return IsCharacterActionTurn(champ) || IsCharacterSelectorTurn(champ);
        }

        public virtual bool IsCharacterActionTurn(BattleCharacter champ)
        {
            return champ != null && active_character == champ.uid
                && phase == BattlePhase.Main && selector == SelectorType.None;
        }

        public virtual bool IsCharacterSelectorTurn(BattleCharacter champ)
        {
            return champ != null && active_character == champ.uid && selector != SelectorType.None;
        }

        //Slow function
        public virtual bool CanPlayCardAnyTarget(Card card, bool skip_cost = false)
        {
            foreach (Slot slot in Slot.GetAll())
            {
                if (CanPlayCard(card, slot, skip_cost))
                    return true;
            }
            return false;
        }

        //Check if a card is allowed to be played on slot
        public virtual bool CanPlayCard(Card card, Slot target, bool skip_cost = false)
        {
            if (card == null)
                return false;

            BattleCharacter character = GetCharacter(card.owner_uid);
            if (!skip_cost && !character.CanPayMana(card))
                return false; //Cant pay mana
            if (!character.HasCard(character.cards_hand, card))
                return false; // Card not in hand
            if (character.IsDead())
                return false;

            foreach (AbilityData ability in card.GetAbilities())
            {
                if (!ability.AreTriggerConditionsMet(this, character, card))
                    return false;
            }

            if (card.CardData.IsRequireTarget())
            {
                return IsPlayTargetValid(card, target); //Check play target on slot
            }

            if (card.CardData.card_type == CardType.Skill)
            {
                return CanAnyPlayAbilityTrigger(character, card);
            }
            return true;
        }

        public virtual bool CanMoveCharacter(BattleCharacter character, Slot target)
        {
            if (character == null || character.IsDead())
                return false; //Dead

            if (target.enemy != character.IsEnemy())
                return false; //Wrong side

            BattleCharacter scharacter = GetSlotCharacter(target);
            if (scharacter != null)
                return false; //Already occupied
            return true;
        }

        public virtual bool CanUseItem(BattleCharacter caster, Card item)
        {
            if (caster == null || item == null)
                return false;

            BattleCharacter champion = GetCharacter(item.owner_uid);
            if (!champion.HasCard(champion.cards_item, item))
                return false; // Card not in items

            if (item.CardData.item_type != ItemType.ItemConsumable)
                return false; //Must be consumable

            return true;
        }

        //For choice selector
        public virtual bool CanSelectAbility(BattleCharacter caster, Card card, AbilityData ability)
        {
            if (ability == null || caster == null)
                return false; //This card cant cast

            //if (!caster.CanPayAbility(ability))
            //    return false; //Cant pay for ability

            if (!ability.AreTriggerConditionsMet(this, caster, card))
                return false; //Conditions not met

            return true;
        }

        public virtual bool CanAnyPlayAbilityTrigger(BattleCharacter caster, Card card)
        {
            if (card == null)
                return false;

            foreach (AbilityData ability in card.GetAbilities())
            {
                if (ability.trigger == AbilityTrigger.OnPlay && ability.AreTriggerConditionsMet(this, caster, card))
                    return true;
            }
            return false;
        }

        //Check if Card play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card card, BattleCharacter target)
        {
            if (card == null || target == null)
                return false;

            foreach (AbilityData ability in card.GetAbilities())
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.IsPlayTarget())
                {
                    if (!ability.CanPlayTarget(this, card, target))
                        return false;
                }
            }
            return true;
        }

        //Check if Slot play target is valid, play target is the target when a spell requires to drag directly onto another card
        public virtual bool IsPlayTargetValid(Card card, Slot target)
        {
            if (card == null || target == null)
                return false;

            BattleCharacter slot_champ = GetSlotCharacter(target);
            if (slot_champ != null)
            {
                if (IsPlayTargetValid(card, slot_champ))
                    return true; //Slot has card, check play target on that card
            }

            foreach (AbilityData ability in card.GetAbilities())
            {
                if (ability && ability.trigger == AbilityTrigger.OnPlay && ability.IsPlayTarget())
                {
                    if (!ability.CanPlayTarget(this, card, target))
                        return false;
                }
            }
            return true;
        }


        public List<Card> GetValidPlayCards(BattleCharacter character, bool skip_cost = false)
        {
            ListSwap<Card> card_swap = GetCardListSwap();
            List<Card> valid_cards = card_swap.Get();
            foreach (Card tcard in character.cards_hand)
            {
                if (CanPlayCardAnyTarget(tcard, skip_cost))
                {
                    valid_cards.Add(tcard);
                }
            }

            return valid_cards;
        }

        public Card GetIntentCard(BattleCharacter character, int turn)
        {
            List<Card> valid_cards = GetValidPlayCards(character, true);
            Card card = character.CharacterData.behavior.SelectPlayCard(this, character, valid_cards, turn);
            if (card != null && card.CardData.intent != null)
            {
                return card;
            }
            return null;
        }

        public int CountAlive()
        {
            int count = 0;
            foreach (BattleCharacter character in characters)
            {
                if (!character.IsDead())
                {
                    count++;
                }
            }
            return count;
        }

        public int CountAliveTeam(bool enemy)
        {
            int count = 0;
            foreach (BattleCharacter character in characters)
            {
                if (!character.IsDead() && character.IsEnemy() == enemy)
                {
                    count++;
                }
            }
            return count;
        }

        public BattleCharacter GetFirstInitiative()
        {
            if (initiatives.Count > 0)
                return GetCharacter(initiatives[0]);
            return null;
        }

        //Character at the front is 1, then 2...
        public int GetOrderIndex(BattleCharacter character)
        {
            int count_in_front = 0;
            foreach (BattleCharacter acharacter in characters)
            {
                if (acharacter.slot.x < character.slot.x && acharacter.slot.enemy == character.slot.enemy)
                    count_in_front++;
            }
            return count_in_front + 1;
        }

        public BattleCharacter GetFrontCharacter(bool enemy_side)
        {
            for (int x = Slot.x_min; x <= Slot.x_max; x++)
            {
                BattleCharacter character = GetSlotCharacter(new Slot(x, enemy_side));
                if (character != null)
                    return character;
            }
            return null;
        }

        public BattleCharacter GetBackCharacter(bool enemy_side)
        {
            for (int x = Slot.x_max; x >= Slot.x_min; x--)
            {
                BattleCharacter character = GetSlotCharacter(new Slot(x, enemy_side));
                if (character != null)
                    return character;
            }
            return null;
        }

        public BattleCharacter GetActiveCharacter()
        {
            return GetCharacter(active_character);
        }
        
        public BattleCharacter GetCharacter(string uid)
        {
            foreach (BattleCharacter character in characters)
            {
                if (character.uid == uid)
                    return character;
            }
            return null;
        }

        public BattleCharacter GetChampion(int player_id)
        {
            foreach (BattleCharacter champion in characters)
            {
                if (champion.player_id == player_id)
                    return champion;
            }
            return null;
        }

        public Card GetCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                Card acard = champion.GetCard(card_uid);
                if (acard != null)
                    return acard;
            }
            return null;
        }

        public Card GetHandCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                foreach (Card card in champion.cards_hand)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetPowerCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                foreach (Card card in champion.cards_power)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetDeckCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                foreach (Card card in champion.cards_deck)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetDiscardCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                foreach (Card card in champion.cards_discard)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetVoidCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                foreach (Card card in champion.cards_void)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public Card GetTempCard(string card_uid)
        {
            foreach (BattleCharacter champion in characters)
            {
                foreach (Card card in champion.cards_temp)
                {
                    if (card != null && card.uid == card_uid)
                        return card;
                }
            }
            return null;
        }

        public BattleCharacter GetSlotCharacter(Slot slot)
        {
            foreach (BattleCharacter bcharacter in characters)
            {
                if (bcharacter != null && !bcharacter.IsDead())
                {
                    if(bcharacter.slot == slot)
                        return bcharacter;
                }
            }
            return null;
        }

        public bool IsInHand(Card card)
        {
            return card != null && GetHandCard(card.uid) != null;
        }

        public bool IsInPower(Card card)
        {
            return card != null && GetPowerCard(card.uid) != null;
        }

        public bool IsInDeck(Card card)
        {
            return card != null && GetDeckCard(card.uid) != null;
        }

        public bool IsInDiscard(Card card)
        {
            return card != null && GetDiscardCard(card.uid) != null;
        }

        public bool IsInVoid(Card card)
        {
            return card != null && GetVoidCard(card.uid) != null;
        }

        public bool IsInTemp(Card card)
        {
            return card != null && GetTempCard(card.uid) != null;
        }

        public bool HasStarted()
        {
            return phase != BattlePhase.None;
        }

        public bool HasEnded()
        {
            return phase == BattlePhase.Ended;
        }

        public bool IsActive()
        {
            return phase != BattlePhase.None && phase != BattlePhase.Ended;
        }

        public ListSwap<BattleCharacter> GetCharacterListSwap()
        {
            if (character_list == null)
                character_list = new ListSwap<BattleCharacter>();
            return character_list; //Used for optimiztion, allows to get an empty array already existing instead of instantiating a new one
        }

        public ListSwap<Card> GetCardListSwap()
        {
            if (card_list == null)
                card_list = new ListSwap<Card>();
            return card_list; //Used for optimiztion, allows to get an empty array already existing instead of instantiating a new one
        }

        public ListSwap<Slot> GetSlotListSwap()
        {
            if (slot_list == null)
                slot_list = new ListSwap<Slot>();
            return slot_list; //Used for optimiztion, allows to get an empty array already existing instead of instantiating a new one
        }

        public ListSwap<CardData> GetCardDataListSwap()
        {
            if (card_data_list == null)
                card_data_list = new ListSwap<CardData>();
            return card_data_list; //Used for optimiztion, allows to get an empty array already existing instead of instantiating a new one
        }

        //Same as clone, but also instantiates the variable (much slower)
        public static Battle CloneNew(Battle source)
        {
            Battle game = new Battle();
            Clone(source, game);
            return game;
        }

        //Clone all variables into another var, used mostly by the AI when building a prediction tree
        public static void Clone(Battle source, Battle dest)
        {
            dest.battle_id = source.battle_id;
            dest.active_character = source.active_character;
            dest.turn_count = source.turn_count;
            dest.turn_timer = source.turn_timer;
            dest.phase = source.phase;

            BattleCharacter.CloneList(source.characters, dest.characters);

            dest.selector = source.selector;
            dest.selector_player_id = source.selector_player_id;
            dest.selector_caster_uid = source.selector_caster_uid;
            dest.selector_card_uid = source.selector_card_uid;
            dest.selector_ability_id = source.selector_ability_id;

            dest.last_destroyed = source.last_destroyed;
            dest.last_played = source.last_played;
            dest.last_summoned = source.last_summoned;
            dest.last_targeted = source.last_targeted;
            dest.ability_triggerer = source.ability_triggerer;
            dest.rolled_value = source.rolled_value;

            //Some values are commented for optimization, you can uncomment if you want more accurate slower AI
            CloneHash(source.ability_played, dest.ability_played);
        }

        public static void CloneList(List<string> source, List<string> dest)
        {
            dest.Clear();
            foreach (string str in source)
                dest.Add(str);
        }

        public static void CloneHash(HashSet<string> source, HashSet<string> dest)
        {
            dest.Clear();
            foreach (string str in source)
                dest.Add(str);
        }
    }

    [System.Serializable]
    public enum BattlePhase
    {
        None = 0,
        StartTurn = 10, //Start of turn resolution
        Main = 20,      //Main play phase
        EndTurn = 30,   //End of turn resolutions
        Ended = 50,
    }

    [System.Serializable]
    public enum SelectorType
    {
        None = 0,
        SelectTarget = 10,
        SelectorCard = 20,
        SelectorChoice = 30,
    }
}