using RogueEngine.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Defines all ability data
    /// </summary>

    [CreateAssetMenu(fileName = "ability", menuName = "TcgEngine/AbilityData", order = 5)]
    public class AbilityData : ScriptableObject
    {
        public string id;

        [Header("Trigger")]
        public AbilityTrigger trigger;             //WHEN does the ability trigger?
        public ConditionData[] conditions_trigger; //Condition checked on the card triggering the ability (usually the caster)

        [Header("Target")]
        public AbilityTarget target;               //WHO is targeted?
        public ConditionData[] conditions_target;  //Condition checked on the target to know if its a valid taget
        public FilterData[] filters_target;  //Condition checked on the target to know if its a valid taget

        [Header("Effect")]
        public EffectData[] effects;              //WHAT this does?
        public StatusData[] status;               //Status added by this ability  
        public int value;   

        [Header("Upgrade")]
        public float upgrade_value;            //Add this value for each level after 1
        public StatusEffect status_bonus;     //Add the value of this status to ability value

        [Header("Chain/Choices")]
        public AbilityData[] chain_abilities;    //Abilities that will be triggered after this one

        [Header("FX")]
        public GameObject board_fx;
        public GameObject caster_fx;
        public GameObject target_fx;
        public AudioClip cast_audio;
        public AudioClip target_audio;
        public string caster_anim;
        public string target_anim;

        [Header("Text")]
        public string title;
        [TextArea(5, 7)]
        public string desc;
        //public TipData[] tips;

        public static List<AbilityData> ability_list = new List<AbilityData>();                             //Faster access in loops
        public static Dictionary<string, AbilityData> ability_dict = new Dictionary<string, AbilityData>(); //Faster access in Get(id)

        public static void Load(string folder = "")
        {
            if (ability_list.Count == 0)
            {
                ability_list.AddRange(Resources.LoadAll<AbilityData>(folder));

                foreach (AbilityData ability in ability_list)
                    ability_dict.Add(ability.id, ability);
            }
        }

        public string GetTitle()
        {
            return title;
        }

        public string GetDesc()
        {
            return desc;
        }

        public string GetDesc(ChampionData card)
        {
            string dsc = desc;
            dsc = dsc.Replace("<name>", card.title);
            dsc = dsc.Replace("<value>", GetValue().ToString());
            return dsc;
        }

        public string GetDesc(CharacterData card)
        {
            string dsc = desc;
            dsc = dsc.Replace("<name>", card.title);
            dsc = dsc.Replace("<value>", GetValue().ToString());
            return dsc;
        }

        public string GetDesc(CardData card)
        {
            string dsc = desc;
            dsc = dsc.Replace("<name>", card.title);
            dsc = dsc.Replace("<value>", GetValue().ToString());
            return dsc;
        }

        public string GetDesc(BattleCharacter character, Card card)
        {
            string dsc = desc;
            dsc = dsc.Replace("<name>", card.CardData.title);
            dsc = dsc.Replace("<value>", GetValue(character, card).ToString());
            return dsc;
        }

        public int GetValue()
        {
            return value;
        }

        public int GetValue(BattleCharacter character, Card card)
        {
            int level = card != null ? card.level : 1;
            int value = GetValue(level);
            int bonus = GetBonusValue(character, card);
            return value + bonus;
        }

        public int GetValue(int level)
        {
            return value + Mathf.RoundToInt((level - 1) * upgrade_value);
        }

        public int GetBonusValue(BattleCharacter character, Card card)
        {
            return character.GetStatusValue(status_bonus);
        }

        //Event condition for map ongoing abilities
        public bool AreMapTriggerConditionsMet(World world, Champion champion, ChampionItem item)
        {
            foreach (ConditionData condition in conditions_trigger)
            {
                if (!condition.IsMapTriggerConditionMet(world, this, champion, item))
                    return false;
            }
            return true;
        }

        public bool AreMapTargetConditionsMet(World world, Champion champion, ChampionItem item, Champion target)
        {
            foreach (ConditionData condition in conditions_target)
            {
                if (!condition.IsMapTargetConditionMet(world, this, champion, item, target))
                    return false;
            }
            return true;
        }

        //Generic condition for the ability to trigger
        public bool AreTriggerConditionsMet(Battle data, BattleCharacter caster, Card card)
        {
            foreach (ConditionData cond in conditions_trigger)
            {
                if (cond != null)
                {
                    if (!cond.IsTriggerConditionMet(data, this, caster, card))
                        return false;
                }
            }
            return true;
        }

        //Some abilities are caused by another card (PlayOther), otherwise most of the time the triggerer is the caster, check condition on triggerer
        public bool AreTriggerConditionsMet(Battle data, BattleCharacter caster, Card card, Card trigger_card)
        {
            foreach (ConditionData cond in conditions_trigger)
            {
                if (cond != null)
                {
                    if (!cond.IsTriggerConditionMet(data, this, caster, card))
                        return false;
                    if (trigger_card == null && !cond.IsTargetConditionMet(data, this, caster, card, caster))
                        return false;
                    if (trigger_card != null && !cond.IsTargetConditionMet(data, this, caster, card, trigger_card))
                        return false;
                }
            }
            return true;
        }

        //Check if the card target is valid
        public bool AreTargetConditionsMet(Battle data, BattleCharacter caster, Card card, Card target_card)
        {
            foreach (ConditionData cond in conditions_target)
            {
                if (cond != null && !cond.IsTargetConditionMet(data, this, caster, card, target_card))
                    return false;
            }
            return true;
        }

        //Check if the player target is valid
        public bool AreTargetConditionsMet(Battle data, BattleCharacter caster, Card card, BattleCharacter target_player)
        {
            foreach (ConditionData cond in conditions_target)
            {
                if (cond != null && !cond.IsTargetConditionMet(data, this, caster, card, target_player))
                    return false;
            }
            return true;
        }

        //Check if the slot target is valid
        public bool AreTargetConditionsMet(Battle data, BattleCharacter caster, Card card, Slot target_slot)
        {
            foreach (ConditionData cond in conditions_target)
            {
                if (cond != null && !cond.IsTargetConditionMet(data, this, caster, card, target_slot))
                    return false;
            }
            return true;
        }

        //Check if the card data target is valid
        public bool AreTargetConditionsMet(Battle data, BattleCharacter caster, Card card, CardData target_card)
        {
            foreach (ConditionData cond in conditions_target)
            {
                if (cond != null && !cond.IsTargetConditionMet(data, this, caster, card, target_card))
                    return false;
            }
            return true;
        }

        //CanTarget is similar to AreTargetConditionsMet but only applies to targets on the board, with extra board-only conditions
        public bool CanTarget(Battle data, BattleCharacter caster, Card card, Card target)
        {
            bool condition_match = AreTargetConditionsMet(data, caster, card, target);
            return condition_match;
        }

        //Can target check additional restrictions and is usually for SelectTarget or PlayTarget abilities
        public bool CanTarget(Battle data, BattleCharacter caster, Card card, BattleCharacter target)
        {
            bool condition_match = AreTargetConditionsMet(data, caster, card, target);
            return condition_match && !target.IsDead();
        }

        public bool CanTarget(Battle data, BattleCharacter caster, Card card, Slot target)
        {
            return AreTargetConditionsMet(data, caster, card, target); //No additional conditions for slots
        }

        //Check if destination array has the target after being filtered, used to support filters in CardSelector
        public bool IsCardSelectionValid(Battle data, BattleCharacter caster, Card card, Card target, ListSwap<Card> card_array = null)
        {
            List<Card> targets = GetCardTargets(data, caster, card, card_array);
            return targets.Contains(target); //Card is still in array after filtering
        }

        public void DoEffects(BattleLogic logic, BattleCharacter caster, Card card)
        {
            foreach(EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, card);
        }

        public void DoEffects(BattleLogic logic, BattleCharacter caster, Card card, Card target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, card, target);
            foreach(StatusData stat in status)
                target.AddStatus(stat, GetValue(caster, card));
        }

        public void DoEffects(BattleLogic logic, BattleCharacter caster, Card card, BattleCharacter target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, card, target);
            foreach (StatusData stat in status)
                target.AddStatus(stat, GetValue(caster, card));
        }

        public void DoEffects(BattleLogic logic, BattleCharacter caster, Card card, Slot target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, card, target);
        }

        public void DoEffects(BattleLogic logic, BattleCharacter caster, Card card, CardData target)
        {
            foreach (EffectData effect in effects)
                effect?.DoEffect(logic, this, caster, card, target);
        }

        public void DoOngoingEffects(BattleLogic logic, BattleCharacter caster, Card card, Card target)
        {
            foreach (EffectData effect in effects)
                effect?.DoOngoingEffect(logic, this, caster, card, target);
            foreach (StatusData stat in status)
                target.AddOngoingStatus(stat, GetValue(caster, card));
        }

        public void DoOngoingEffects(BattleLogic logic, BattleCharacter caster, Card card, BattleCharacter target)
        {
            foreach (EffectData effect in effects)
                effect?.DoOngoingEffect(logic, this, caster, card, target);
            foreach (StatusData stat in status)
                target.AddOngoingStatus(stat, GetValue(caster, card));
        }

        public void DoMapEffects(WorldLogic logic, Champion caster, ChampionItem item, Champion target)
        {
            foreach (EffectData effect in effects)
                effect?.DoMapEffect(logic, this, caster, item, target);
        }

        public void DoMapOngoingEffects(WorldLogic logic, Champion caster, ChampionItem item, Champion target)
        {
            foreach (EffectData effect in effects)
                effect?.DoMapOngoingEffect(logic, this, caster, item, target);
        }

        public bool IsAttack()
        {
            return HasEffect<EffectDamage>();
        }

        public bool HasEffect<T>() where T : EffectData
        {
            foreach (EffectData eff in effects)
            {
                if (eff != null && eff is T)
                    return true;
            }
            return false;
        }

        public bool HasStatus(StatusEffect type)
        {
            foreach (StatusData sta in status)
            {
                if (sta != null && sta.effect == type)
                    return true;
            }
            return false;
        }

        public void AddValidCards(Battle data, BattleCharacter caster, Card card, List<Card> source, List<Card> targets)
        {
            foreach (Card tcard in source)
            {
                if (AreTargetConditionsMet(data, caster, card, tcard))
                    targets.Add(tcard);
            }
        }

        //Return cards targets,  memory_array is used for optimization and avoid allocating new memory
        public List<Card> GetCardTargets(Battle data, BattleCharacter caster, Card card, ListSwap<Card> memory_array = null)
        {
            if (memory_array == null)
                memory_array = new ListSwap<Card>(); //Slow operation

            List<Card> targets = memory_array.Get();

            if (target == AbilityTarget.CardSelf)
            {
                if (card != null && AreTargetConditionsMet(data, caster, card, card))
                    targets.Add(card);
            }

            if (target == AbilityTarget.AllCardsHand)
            {
                foreach (BattleCharacter character in data.characters)
                {
                    foreach (Card tcard in character.cards_hand)
                    {
                        if (AreTargetConditionsMet(data, caster, card, tcard))
                            targets.Add(tcard);
                    }
                }
            }

            if (target == AbilityTarget.AllCardsAllPiles || target == AbilityTarget.CardSelector)
            {
                foreach (BattleCharacter character in data.characters)
                {
                    AddValidCards(data, caster, card, character.cards_deck, targets);
                    AddValidCards(data, caster, card, character.cards_hand, targets);
                    AddValidCards(data, caster, card, character.cards_power, targets);
                    AddValidCards(data, caster, card, character.cards_discard, targets);
                    AddValidCards(data, caster, card, character.cards_void, targets);
                    AddValidCards(data, caster, card, character.cards_temp, targets);
                }
            }

            if (target == AbilityTarget.LastPlayed)
            {
                Card target = data.GetCard(data.last_played);
                if (target != null && AreTargetConditionsMet(data, caster, card, target))
                    targets.Add(target);
            }

            if (target == AbilityTarget.LastDestroyed)
            {
                Card target = data.GetCard(data.last_destroyed);
                if (target != null && AreTargetConditionsMet(data, caster, card, target))
                    targets.Add(target);
            }

            if (target == AbilityTarget.LastTargeted)
            {
                Card target = data.GetCard(data.last_targeted);
                if (target != null && AreTargetConditionsMet(data, caster, card, target))
                    targets.Add(target);
            }

            if (target == AbilityTarget.LastSummoned)
            {
                Card target = data.GetCard(data.last_summoned);
                if (target != null && AreTargetConditionsMet(data, caster, card, target))
                    targets.Add(target);
            }

            if (target == AbilityTarget.AbilityTriggerer)
            {
                Card target = data.GetCard(data.ability_triggerer);
                if (target != null && AreTargetConditionsMet(data, caster, card, target))
                    targets.Add(target);
            }

            //Filter targets
            if (filters_target != null && targets.Count > 0)
            {
                foreach (FilterData filter in filters_target)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, card, targets, memory_array.GetOther(targets));
                }
            }

            return targets;
        }

        //Return player targets,  memory_array is used for optimization and avoid allocating new memory
        public List<BattleCharacter> GetCharacterTargets(Battle data, BattleCharacter caster, Card card, ListSwap<BattleCharacter> memory_array = null)
        {
            if (memory_array == null)
                memory_array = new ListSwap<BattleCharacter>(); //Slow operation

            List<BattleCharacter> targets = memory_array.Get();

            if (target == AbilityTarget.CharacterSelf)
            {
                if(AreTargetConditionsMet(data, caster, card, caster))
                    targets.Add(caster);
            }
            else if (target == AbilityTarget.AllCharacters)
            {
                foreach (BattleCharacter character in data.characters)
                {
                    if (AreTargetConditionsMet(data, caster, card, character))
                        targets.Add(character);
                }
            }
            else if (target == AbilityTarget.SelectTarget)
            {
                foreach (BattleCharacter character in data.characters)
                {
                    if (AreTargetConditionsMet(data, caster, card, character))
                        targets.Add(character);
                }
            }
            else if (target == AbilityTarget.LastTargeted)
            {
                BattleCharacter target = data.GetCharacter(data.last_targeted);
                if (target != null && AreTargetConditionsMet(data, caster, card, target))
                    targets.Add(target);
            }

            //Filter targets
            if (filters_target != null && targets.Count > 0)
            {
                foreach (FilterData filter in filters_target)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, card, targets, memory_array.GetOther(targets));
                }
            }

            return targets;
        }

        public List<Champion> GetWorldCharacterTargets(World data, Champion caster, ChampionItem item, ListSwap<Champion> memory_array = null)
        {
            if (memory_array == null)
                memory_array = new ListSwap<Champion>(); //Slow operation

            List<Champion> targets = memory_array.Get();

            if (target == AbilityTarget.CharacterSelf)
            {
                if (AreMapTargetConditionsMet(data, caster, item, caster))
                    targets.Add(caster);
            }
            else if (target == AbilityTarget.AllCharacters)
            {
                foreach (Champion character in data.champions)
                {
                    if (AreMapTargetConditionsMet(data, caster, item, character))
                        targets.Add(character);
                }
            }

            return targets;
        }

        //Return slot targets,  memory_array is used for optimization and avoid allocating new memory
        public List<Slot> GetSlotTargets(Battle data, BattleCharacter caster, Card card, ListSwap<Slot> memory_array = null)
        {
            if (memory_array == null)
                memory_array = new ListSwap<Slot>(); //Slow operation

            List<Slot> targets = memory_array.Get();

            if (target == AbilityTarget.AllSlots)
            {
                List<Slot> slots = Slot.GetAll();
                foreach (Slot slot in slots)
                {
                    if (AreTargetConditionsMet(data, caster, card, slot))
                        targets.Add(slot);
                }
            }

            //Filter targets
            if (filters_target != null && targets.Count > 0)
            {
                foreach (FilterData filter in filters_target)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, card, targets, memory_array.GetOther(targets));
                }
            }

            return targets;
        }

        public List<CardData> GetCardDataTargets(Battle data, BattleCharacter caster, Card card, ListSwap<CardData> memory_array = null)
        {
            if (memory_array == null)
                memory_array = new ListSwap<CardData>(); //Slow operation

            List<CardData> targets = memory_array.Get();

            if (target == AbilityTarget.AllCardData)
            {
                foreach (CardData tcard in CardData.GetAll())
                {
                    if (AreTargetConditionsMet(data, caster, card, tcard))
                        targets.Add(tcard);
                }
            }

            //Filter targets
            if (filters_target != null && targets.Count > 0)
            {
                foreach (FilterData filter in filters_target)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, this, caster, card, targets, memory_array.GetOther(targets));
                }
            }

            return targets;
        }

        public bool CanPlayTarget(Battle data, Card card, BattleCharacter character)
        {
            if (target == AbilityTarget.PlayTarget)
            {
                BattleCharacter owner = data.GetCharacter(card.owner_uid);
                return CanTarget(data, owner, card, character);
            }
            else
            {
                return CanPlayTarget(data, card, character.slot);
            }
        }

        public bool CanPlayTarget(Battle data, Card card, Slot selected)
        {
            BattleCharacter owner = data.GetCharacter(card.owner_uid);

            if (target == AbilityTarget.PlayTarget)
            {
                return CanTarget(data, owner, card, selected);
            }

            return false;
        }
        
        // Check if there is any valid target, if not, AI wont try to cast activated ability
        public bool HasValidSelectTarget(Battle game_data, BattleCharacter caster, Card card)
        {
            if (target == AbilityTarget.SelectTarget)
            {
                if (HasValidBoardTarget(game_data, caster, card))
                    return true;
                if (HasValidSlotTarget(game_data, caster, card))
                    return true;
                return false;
            }

            if (target == AbilityTarget.CardSelector)
            {
                if (HasValidCardTarget(game_data, caster, card))
                    return true;
                return false;
            }

            if (target == AbilityTarget.ChoiceSelector)
            {
                foreach (AbilityData choice in chain_abilities)
                {
                    if(choice.AreTriggerConditionsMet(game_data, caster, card))
                        return true;
                }
                return false;
            }

            return true; //Not selecting, valid
        }

        public bool HasValidBoardTarget(Battle game_data, BattleCharacter caster, Card card)
        {
            for (int p = 0; p < game_data.characters.Count; p++)
            {
                BattleCharacter champ = game_data.characters[p];
                if (CanTarget(game_data, caster, card, champ))
                    return true;
            }
            return false;
        }

        public bool HasValidCardTarget(Battle game_data, BattleCharacter caster, Card card)
        {
            for (int p = 0; p < game_data.characters.Count; p++)
            {
                BattleCharacter champ = game_data.characters[p];
                bool v1 = HasValidCardTarget(game_data, caster, card, champ.cards_deck);
                bool v2 = HasValidCardTarget(game_data, caster, card, champ.cards_discard);
                bool v3 = HasValidCardTarget(game_data, caster, card, champ.cards_hand);
                bool v4 = HasValidCardTarget(game_data, caster, card, champ.cards_temp);
                if (v1 || v2 || v3 || v4 )
                    return true;
            }
            return false;
        }

        public bool HasValidCardTarget(Battle game_data, BattleCharacter caster, Card card, List<Card> list)
        {
            for (int c = 0; c < list.Count; c++)
            {
                Card tcard = list[c];
                if (AreTargetConditionsMet(game_data, caster, card, tcard))
                    return true;
            }
            return false;
        }

        public bool HasValidSlotTarget(Battle game_data, BattleCharacter caster, Card card)
        {
            foreach (Slot slot in Slot.GetAll())
            {
                if (CanTarget(game_data, caster, card, slot))
                    return true;
            }
            return false;
        }

        public bool IsSelector()
        {
            return target == AbilityTarget.SelectTarget || target == AbilityTarget.CardSelector || target == AbilityTarget.ChoiceSelector;
        }

        public bool IsPlayTarget()
        {
            return target == AbilityTarget.PlayTarget;
        }

        public static AbilityData Get(string id)
        {
            if (id == null)
                return null;
            bool success = ability_dict.TryGetValue(id, out AbilityData ability);
            if (success)
                return ability;
            return null;
        }

        public static List<AbilityData> GetAll()
        {
            return ability_list;
        }
    }


    public enum AbilityTrigger
    {
        None = 0,

        Ongoing = 2,  //Always active (does not work with all effects)

        OnPlay = 10,  //When playeds
        OnPlayOther = 12,  //When playeds

        StartOfTurn = 20,       //Every turn
        EndOfTurn = 22,         //Every turn

        OnDraw = 30,   //When card is drawn into hand
        OnDrawOther = 31,   //When card is drawn into hand

        OnDeath = 40,       //When character is dying
        OnDeathOther = 41,    //When any character is dying
        OnDamaged = 42,     //When character is damaged

        BattleStart = 50,
        BattleEnd = 52,
    }

    public enum AbilityTarget
    {
        None = 0,

        CardSelf = 1,
        CharacterSelf = 4,
        AllCharacters = 7,

        AllCardsHand = 11,
        AllCardsAllPiles = 12,
        AllSlots = 15,
        AllCardData = 17,       //For card Create effects only

        PlayTarget = 20,        //The target selected at the same time the spell was played (spell only)  
        
        AbilityTriggerer = 25,   //The card that triggered the trap

        SelectTarget = 30,        //Select a card, player or slot on board
        CardSelector = 40,          //Card selector menu
        ChoiceSelector = 50,        //Choice selector menu

        LastPlayed = 70,            //Last card that was played 
        LastTargeted = 72,          //Last card that was targeted with an ability
        LastDestroyed = 74,            //Last card that was killed
        LastSummoned = 77,            //Last card that was summoned or created

    }

}
