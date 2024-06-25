using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace RogueEngine.Gameplay
{
    /// <summary>
    /// Execute and resolves game rules and logic
    /// </summary>

    public class BattleLogic
    {
        public UnityAction onBattleStart;
        public UnityAction<int> onBattleEnd;

        public UnityAction onTurnStart;
        public UnityAction onTurnPlay;
        public UnityAction onTurnEnd;

        public UnityAction<BattleCharacter, Slot> onCharacterMoved;
        public UnityAction<BattleCharacter, int> onCharacterDamaged;
        public UnityAction<BattleCharacter> onCharacterTransformed;

        public UnityAction<Card, Slot> onCardPlayed;
        public UnityAction<Card> onCardSummoned;
        public UnityAction<Card> onCardTransformed;
        public UnityAction<Card> onCardDiscarded;
        public UnityAction<int> onCardDrawn;

        public UnityAction<Card> onItemUsed;
        public UnityAction<int> onRollValue;

        public UnityAction<AbilityData, Card> onAbilityStart;
        public UnityAction<AbilityData, Card, Card> onAbilityTargetCard;  //Ability, Caster, Target
        public UnityAction<AbilityData, Card, BattleCharacter> onAbilityTargetCharacter;  //Ability, Caster, Target
        public UnityAction<AbilityData, Card, Slot> onAbilityTargetSlot;  //Ability, Caster, Target
        public UnityAction<AbilityData, Card> onAbilityEnd;

        public UnityAction<Card, Card> onSecretTrigger;    //Secret, Triggerer
        public UnityAction<Card, Card> onSecretResolve;    //Secret, Triggerer

        public UnityAction onSelectorStart;
        public UnityAction onSelectorSelect;
        public UnityAction refreshWorld;
        public UnityAction refreshBattle;

        private World world_data;
        private Battle battle_data;

        private ResolveQueue resolve_queue;

        private System.Random random = new System.Random();
        private List<Card> cards_to_clear = new List<Card>();

        public BattleLogic(bool is_instant = false)
        {
            resolve_queue = new ResolveQueue(is_instant);
        }

        public BattleLogic(World world)
        {
            world_data = world;
            battle_data = null; //Not assigned yet
            resolve_queue = new ResolveQueue(false);
        }

        public virtual void SetData(World game)
        {
            world_data = game;
            battle_data = game.battle;
            resolve_queue.SetData(game.battle);
        }

        public virtual void SetData(Battle game)
        {
            battle_data = game;
            resolve_queue.SetData(game);
        }

        public virtual void Update(float delta)
        {
            resolve_queue.Update(delta);
        }

        //----- Turn Phases ----------

        public virtual void StartBattle(World world, EventBattle battle)
        {
            //Champions
            foreach (Champion champion in world_data.champions)
            {
                BattleCharacter character = BattleCharacter.Create(champion);
                character.slot = GetChampionStartPos(champion);
                battle_data.characters.Add(character);
                SetChampionCards(champion, character);
            }

            //Allies
            foreach (CharacterAlly ally in world_data.allies)
            {
                BattleCharacter character = BattleCharacter.Create(ally.CharacterData, ally.level);
                character.uid = ally.uid;
                character.slot = GetAllyStartPos(ally);
                character.player_id = ally.player_id;
                battle_data.characters.Add(character);
                SetCharacterCards(character);
            }

            //Enemies
            int index = 0;
            foreach (CharacterData edata in battle.enemies)
            {
                 AddEnemy(edata, index, battle.enemies_level);
                 index++;
            }

            foreach (ExtraBattleEnemy extra in battle.extra_enemies)
            {
                if (world_data.champions.Count >= extra.champions_min)
                {
                    index = 0;
                    foreach (CharacterData edata in extra.enemies)
                    {
                        AddEnemy(edata, index, battle.enemies_level);
                        index++;
                    }
                }
            }

            UpdateOngoing();

            //Start state
            onBattleStart?.Invoke();
            RefreshBattle();

            foreach (BattleCharacter character in battle_data.characters)
            {
                TriggerCharacterAbilityType(AbilityTrigger.OnPlay, character);
            }

            resolve_queue.AddCallback(StartTurn);
            resolve_queue.ResolveAll(2f);
        }

        protected virtual void AddEnemy(CharacterData enemy, int index, int level)
        {
            if (enemy != null)
            {
                BattleCharacter character = BattleCharacter.Create(enemy, level);
                character.slot = GetEnemyStartPos(index);
                battle_data.characters.Add(character);

                ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
                character.SetScenarioDifficulty(scenario);

                SetCharacterCards(character);
            }
        }

        public virtual void SetChampionCards(Champion champion, BattleCharacter character)
        {
            character.cards_deck.Clear();
            character.cards_discard.Clear();
            character.cards_hand.Clear();

            foreach (ChampionCard ccard in champion.cards)
            {
                CardData icard = CardData.Get(ccard.card_id);
                if (icard != null)
                {
                    Card card = Card.Create(icard, ccard.level, character, ccard.uid);
                    character.cards_deck.Add(card);
                }
            }

            //Add Equipment items first
            foreach (ChampionItem item in champion.inventory)
            {
                CardData iitem = CardData.Get(item.card_id);
                if (iitem != null && iitem.item_type == ItemType.ItemPassive)
                {
                    Card card = Card.Create(iitem, 1, character, GameTool.GenerateRandomID());
                    character.cards_item.Add(card); //Add to consumable items, outside of deck
                }
                if (iitem != null && iitem.item_type == ItemType.ItemCard)
                {
                    Card card = Card.Create(iitem, 1, character, GameTool.GenerateRandomID());
                    character.cards_deck.Add(card); //Add to deck
                }
            }

            //Then add consumables
            foreach (ChampionItem item in champion.inventory)
            {
                CardData iitem = CardData.Get(item.card_id);
                if (iitem != null && iitem.item_type == ItemType.ItemConsumable)
                {
                    Card card = Card.Create(iitem, 1, character, GameTool.GenerateRandomID());
                    character.cards_item.Add(card); //Add to consumable items, outside of deck
                }
            }

            ShuffleDeck(character.cards_deck);
        }

        public virtual void SetCharacterCards(BattleCharacter character)
        {
            character.cards_deck.Clear();
            character.cards_discard.Clear();
            character.cards_hand.Clear();

            foreach (CardData icard in character.CharacterData.cards)
            {
                Card card = Card.Create(icard, character.level, character);
                character.cards_deck.Add(card);
            }

            if(!character.IsEnemy())
                ShuffleDeck(character.cards_deck);

            DrawEnemyHand(character);
        }

        public virtual void StartTurn()
        {
            ClearTurnData();
            CheckForWinner();

            if (battle_data.phase == BattlePhase.Ended)
                return;

            battle_data.phase = BattlePhase.StartTurn;
            battle_data.turn_timer = GameplayData.Get().turn_duration;
            battle_data.turn_count += 1;

            UpdateOngoing();
            CalculateInitiatives();

            BattleCharacter character = battle_data.GetFirstInitiative();
            if (character == null)
                return;

            battle_data.active_character = character.uid;

            //Refresh 
            character.Refresh();

            //Mana / Shield
            int bonus = character.HasStatus(StatusEffect.Keep) ? character.mana : 0;
            character.mana = character.GetEnergy() + character.delayed_energy + bonus;
            character.delayed_energy = 0;
            character.shield = character.GetStatusValue(StatusEffect.Armor) + character.delayed_shield;
            character.delayed_shield = 0;
            character.RemoveStatus(StatusEffect.Keep);

            //Turn timer and history
            character.history_list.Clear();

            // poison damage
            if (character.HasStatus(StatusEffect.Poisoned))
            {
                int val = character.GetStatusValue(StatusEffect.Poisoned);
                DamageCharacter(character, val, true);
            }

            //Burn damage
            if (character.HasStatus(StatusEffect.Burned))
            {
                int val = character.GetStatusValue(StatusEffect.Burned);
                if (character.HasStatus(StatusEffect.BurnHeal))
                    HealCharacter(character, val);
                else
                    DamageCharacter(character, val);
            }

            //Ongoing Abilities
            UpdateOngoing();
            DrawHand(character);

            onTurnStart?.Invoke();
            RefreshBattle();

            if (battle_data.turn_count == 1)
                TriggerCharacterAbilityType(AbilityTrigger.BattleStart, character);

            TriggerCharacterAbilityType(AbilityTrigger.StartOfTurn, character);

            resolve_queue.AddCallback(StartMainPhase);
            resolve_queue.ResolveAll(0.2f);
        }

        public virtual void StartMainPhase()
        {
            if (battle_data.phase == BattlePhase.Ended)
                return;

            BattleCharacter character = battle_data.GetActiveCharacter();
            bool can_play = character.CanPlayTurn(); //Check before reducing status
            character.ReduceStatusValues();
            UpdateOngoing();

            battle_data.phase = BattlePhase.Main;
            onTurnPlay?.Invoke();
            RefreshBattle();

            if (!can_play)
            {
                character.RemoveStatus(StatusEffect.Stunned);
                EndTurn();
            }
        }

        public virtual void EndTurn()
        {
            if (battle_data.phase != BattlePhase.Main)
                return;

            battle_data.selector = SelectorType.None;
            battle_data.phase = BattlePhase.EndTurn;

            BattleCharacter character = battle_data.GetActiveCharacter();
            RemoveFromInitiativeCurrent(character);

            DrawEnemyHand(character);

            //Remove once status
            character.RemoveOnceStatus();

            foreach (Card card in character.cards_hand)
                card.RemoveOnceStatus();

            battle_data.active_character = "";
            onTurnEnd?.Invoke();
            RefreshBattle();

            resolve_queue.AddCallback(StartTurn);
            resolve_queue.ResolveAll(0.2f);
        }

        //End game with winner
        public virtual void EndBattle(int result)
        {
            if (battle_data.phase != BattlePhase.Ended)
            {
                battle_data.phase = BattlePhase.Ended;
                battle_data.selector = SelectorType.None;
                battle_data.active_character = "";
                battle_data.win_result = result;
                resolve_queue.Clear();
                onBattleEnd?.Invoke(result);
            }
        }

        //Progress to the next step/phase 
        public virtual void NextStep()
        {
            if (battle_data.phase == BattlePhase.Ended)
                return;

            if (battle_data.selector != SelectorType.None)
            {
                CancelSelection();
            }
            else
            {
                EndTurn();
            }
        }

        //Check if a player is winning the game, if so end the game
        //Change or edit this function for a new win condition
        protected virtual void CheckForWinner()
        {
            int champions_alive = 0;
            int enemies_alive = 0;

            foreach (BattleCharacter player in battle_data.characters)
            {
                if (!player.IsDead() && !player.IsEnemy())
                    champions_alive++;
            }

            foreach (BattleCharacter player in battle_data.characters)
            {
                if (!player.IsDead() && player.IsEnemy())
                    enemies_alive++;
            }

            if (champions_alive == 0)
            {
                EndBattle(-1); //Defeat
            }
            else if (enemies_alive == 0)
            {
                EndBattle(1); ; //Player win
            }
        }

        protected virtual void ClearTurnData()
        {
            battle_data.selector = SelectorType.None;
            resolve_queue.Clear();
            battle_data.last_played = null;
            battle_data.last_destroyed = null;
            battle_data.last_summoned = null;
            battle_data.last_targeted = null;
            battle_data.ability_triggerer = null;
            battle_data.ability_played.Clear();
        }

        //--- Setup ------

        public virtual Slot GetChampionStartPos(Champion champion)
        {
            int nb_champ = world_data.CountCharacters();
            int offset = Slot.x_max - nb_champ;
            offset = Mathf.Clamp(offset, 0, 2);
            return new Slot(champion.position + offset, false);
        }

        public virtual Slot GetAllyStartPos(CharacterAlly ally)
        {
            int nb_champ = world_data.CountCharacters();
            int offset = Slot.x_max - nb_champ;
            offset = Mathf.Clamp(offset, 0, 2);
            return new Slot(ally.position + offset, false);
        }

        public virtual Slot GetEnemyStartPos(int index)
        {
            if (index == 0)
                return new Slot(1, true);
            if (index == 1)
                return new Slot(2, true);
            if (index == 2)
                return new Slot(3, true);
            if (index == 3)
                return new Slot(4, true);
            return new Slot(4, true);
        }

        //-----------------

        //When this function is called, initiative of current turn is "locked" but next turn is re-calculated
        public virtual void CalculateInitiatives()
        {
            if (battle_data.CountAlive() == 0)
                return; //No more characters

            //Sort character by speed
            battle_data.characters.Sort((BattleCharacter a, BattleCharacter b) => { return b.GetSpeed().CompareTo(a.GetSpeed()); });

            //Add current turn
            if (battle_data.initiatives.Count == 0)
            {
                foreach (BattleCharacter character in battle_data.characters)
                {
                    if(!character.IsDead())
                        battle_data.initiatives.Add(character.uid);
                }
            }

            //Add initiative of next turn
            battle_data.initiatives_next.Clear();
            foreach (BattleCharacter character in battle_data.characters)
            {
                if (!character.IsDead())
                    battle_data.initiatives_next.Add(character.uid);
            }
        }

        public virtual void RemoveFromInitiativeCurrent(BattleCharacter character)
        {
            for (int i = battle_data.initiatives.Count - 1; i >= 0; i--)
            {
                if (battle_data.initiatives[i] == character.uid)
                    battle_data.initiatives.RemoveAt(i);
            }
        }

        public virtual void RemoveFromInitiativeNext(BattleCharacter character)
        {
            for (int i = battle_data.initiatives_next.Count - 1; i >= 0; i--)
            {
                if (battle_data.initiatives_next[i] == character.uid)
                    battle_data.initiatives_next.RemoveAt(i);
            }
        }

        //---- Gameplay Actions --------------

        public virtual void PlayCard(Card card, Slot target, bool skip_cost = false)
        {
            if (battle_data.CanPlayCard(card, target, skip_cost))
            {
                BattleCharacter owner = battle_data.GetCharacter(card.owner_uid);

                //Cost
                if (!skip_cost)
                    owner.PayMana(card);

                //Play card
                owner.RemoveCardFromAllGroups(card);
                card.Clear();

                CardData icard = card.CardData;
                if (icard.card_type == CardType.Power)
                {
                    owner.cards_power.Add(card);
                }
                else
                {
                    owner.cards_discard.Add(card);
                }

                //History
                owner.AddHistory(GameAction.PlayCard, card);
                owner.turn++;

                //Update ongoing effects
                battle_data.play_target = target;
                battle_data.last_played = card.uid;
                UpdateOngoing();

                //Trigger abilities
                TriggerCardAbilityType(AbilityTrigger.OnPlay, card);
                TriggerCharacterAbilityType(AbilityTrigger.OnPlayOther, owner, card);

                onCardPlayed?.Invoke(card, target);
                RefreshBattle();

                resolve_queue.AddCard(card, AfterPlayCardResolved);
                resolve_queue.ResolveAll(0.3f);
            }
        }

        public virtual void MoveCharacter(BattleCharacter character, Slot slot)
        {
            if (battle_data.CanMoveCharacter(character, slot))
            {
                character.slot = slot;
                UpdateOngoing();

                onCharacterMoved?.Invoke(character, slot);
                RefreshBattle();

                resolve_queue.ResolveAll(0.3f);
            }
        }

        public virtual void UseItem(BattleCharacter character, Card item)
        {
            if (battle_data.CanUseItem(character, item))
            {
                Champion champion = world_data.GetChampion(character.uid);
                if (champion != null)
                {
                    champion.RemoveItem(item.card_id);

                    ChampionItem citem = champion.GetItem(item.card_id);
                    if(citem == null || citem.quantity <= 0)
                        character.cards_item.Remove(item);

                    TriggerCardAbilityType(AbilityTrigger.OnPlay, item);

                    onItemUsed?.Invoke(item);
                    RefreshWorld();

                    resolve_queue.ResolveAll(0.3f);
                }
            }
        }

        public virtual void ShuffleDeck(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card temp = cards[i];
                int randomIndex = random.Next(i, cards.Count);
                cards[i] = cards[randomIndex];
                cards[randomIndex] = temp;
            }
        }

        public virtual void ShuffleDiscardToDeck(BattleCharacter character)
        {
            ShuffleDeck(character.cards_discard);
            character.cards_deck.AddRange(character.cards_discard);
            character.cards_discard.Clear();
        }

        public virtual void DrawHand(BattleCharacter character)
        {
            if (character.IsEnemy())
                return; //Enemies dont draw

            //Discard hand
            for (int i = character.cards_hand.Count - 1; i >= 0; i--)
            {
                bool keep = character.cards_hand[i].HasStatus(StatusEffect.Keep);
                character.cards_hand[i].RemoveStatus(StatusEffect.Keep);
                if (!keep)
                {
                    DiscardCard(character.cards_hand[i]);
                }
            }

            //ReCards draw
            DrawCard(character, character.GetHand() + character.delayed_hand);
            character.delayed_hand = 0;

        }

        public virtual void DrawCard(BattleCharacter character, int nb = 1)
        {
            //Shuffle discard
            if (character.cards_deck.Count < nb)
                ShuffleDiscardToDeck(character);

            for (int i = 0; i < nb; i++)
            {
                if (character.cards_deck.Count > 0 && character.cards_hand.Count < GameplayData.Get().cards_max)
                {
                    Card card = character.cards_deck[0];
                    character.cards_deck.RemoveAt(0);
                    character.cards_hand.Add(card);
                    TriggerCardAbilityType(AbilityTrigger.OnDraw, card);
                    TriggerCharacterAbilityType(AbilityTrigger.OnDrawOther, character, card);
                }
            }

            onCardDrawn?.Invoke(nb);
        }

        public virtual void DrawEnemyHand(BattleCharacter character)
        {
            //Enemies add all cards to hand, since they have no hand size
            if (character.IsEnemy())
            {
                character.cards_hand.AddRange(character.cards_deck);
                character.cards_hand.AddRange(character.cards_discard);
                character.cards_deck.Clear();
                character.cards_discard.Clear();
            }
        }

        //Put a card from deck into discard
        public virtual void DrawDiscardCard(BattleCharacter owner, int nb = 1)
        {
            for (int i = 0; i < nb; i++)
            {
                if (owner.cards_deck.Count > 0)
                {
                    Card card = owner.cards_deck[0];
                    owner.cards_deck.RemoveAt(0);
                    owner.cards_discard.Add(card);
                }
            }
        }

        public virtual BattleCharacter SummonCharacter(int player_id, CharacterAlly ally, Slot slot)
        {
            return SummonCharacter(player_id, ally.CharacterData, slot, ally.level, ally.uid);
        }

        public virtual BattleCharacter SummonCharacter(int player_id, CharacterData character, Slot slot, int level)
        {
            string uid = GameTool.GenerateRandomID();
            return SummonCharacter(player_id, character, slot, level, uid);
        }

        public virtual BattleCharacter SummonCharacter(int player_id, CharacterData character, Slot slot, int level, string uid)
        {
            BattleCharacter summon = BattleCharacter.Create(character, uid, level);
            summon.player_id = player_id;
            summon.slot = slot;
            battle_data.characters.Add(summon);

            foreach (CardData icard in summon.CharacterData.cards)
            {
                Card card = Card.Create(icard, level, summon);
                summon.cards_deck.Add(card);
            }

            SetCharacterCards(summon);
            CalculateInitiatives();

            TriggerCharacterAbilityType(AbilityTrigger.OnPlay, summon);

            return summon;
        }

        //Summon copy of an exiting card into hand
        public virtual Card SummonCopyHand(BattleCharacter owner, Card copy)
        {
            CardData icard = copy.CardData;
            return SummonCardHand(owner, icard, copy.level);
        }

        //Create a new card and send it to your hand
        public virtual Card SummonCardHand(BattleCharacter owner, CardData card, int level)
        {
            Card acard = Card.Create(card, level, owner);
            owner.cards_hand.Add(acard);
            battle_data.last_summoned = acard.uid;
            onCardSummoned?.Invoke(acard);
            return acard;
        }

        //Transform card into another one
        public virtual Card TransformCard(Card card, CardData transform_to)
        {
            card.SetCard(transform_to, card.level);

            onCardTransformed?.Invoke(card);

            return card;
        }

        public virtual BattleCharacter TransformCharacter(BattleCharacter character, CharacterData transform_to)
        {
            character.SetCharacter(transform_to);
            character.damage = 0;

            ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
            character.SetScenarioDifficulty(scenario);

            SetCharacterCards(character);

            onCharacterTransformed?.Invoke(character);

            return character;
        }

        //Send card into discard
        public virtual void DiscardCard(Card card)
        {
            if (card == null)
                return;

            if (battle_data.IsInDiscard(card))
                return; //Already discarded

            CardData icard = card.CardData;
            BattleCharacter player = battle_data.GetCharacter(card.owner_uid);

            //Remove card from board and add to discard
            player.RemoveCardFromAllGroups(card);
            player.cards_discard.Add(card);
            card.RemoveStatus(StatusEffect.Keep);
            battle_data.last_destroyed = card.uid;

            cards_to_clear.Add(card); //Will be Clear() in the next UpdateOngoing, so that simultaneous damage effects work
            onCardDiscarded?.Invoke(card);
        }
        
        //Change owner of a card
        public virtual void ChangeOwner(Card card, BattleCharacter owner)
        {
            if (card.owner_uid != owner.uid)
            {
                BattleCharacter powner = battle_data.GetCharacter(card.owner_uid);
                powner.RemoveCardFromAllGroups(card);
                powner.cards_all.Remove(card.uid);
                owner.cards_all[card.uid] = card;
                card.player_id = owner.player_id;
                card.owner_uid = owner.character_id;
            }
        }


        public virtual void ResurrectCharacter(BattleCharacter target, int hp)
        {
            if (target == null)
                return;

            target.damage = target.GetHPMax();
            target.is_dead = false;
            HealCharacter(target, hp);
            CalculateInitiatives();
        }

        //Heal a card
        public virtual void HealCharacter(BattleCharacter target, int value)
        {
            if (target == null)
                return;

            target.damage -= value;
            target.damage = Mathf.Max(target.damage, 0);
        }

        //Generic damage that doesnt come from another card
        public virtual void DamageCharacter(BattleCharacter attacker, BattleCharacter target, int value, bool ignore_shield = false)
        {
            if(attacker == null || target == null)
                return;

            float factor_self = 1f;
            float factor_other = 1f;

            if (attacker.HasStatus(StatusEffect.Courageous))
                factor_self += 0.5f;
            if (attacker.HasStatus(StatusEffect.Fearful))
                factor_self -= 0.5f;

            if (target.HasStatus(StatusEffect.Vulnerable))
                factor_other += 0.5f;
            if (target.HasStatus(StatusEffect.Evasive))
                factor_other -= 0.5f;

            //Deal Damage
            int damage = Mathf.RoundToInt(value * factor_self * factor_other);
            DamageCharacter(target, damage, ignore_shield);

            //Deal Damage Back
            if (target.HasStatus(StatusEffect.Thorn))
            {
                int val = target.GetStatusValue(StatusEffect.Thorn);
                DamageCharacter(attacker, val);
            }
        }

        public virtual void DamageCharacter(BattleCharacter target, int value, bool ignore_shield = false)
        {
            if (target == null)
                return;

            int damage = value;

            if (!ignore_shield)
            {
                int damage_shield = Mathf.Min(damage, target.GetShield());
                target.shield -= damage_shield;
                damage -= damage_shield;
                target.damage += damage;
            }
            else
            {
                target.damage += damage;
            }

            target.RemoveStatus(StatusEffect.Sleep);

            if (target.GetHP() <= 0)
                KillCharacter(target);

            onCharacterDamaged?.Invoke(target, value);

            TriggerCharacterAbilityType(AbilityTrigger.OnDamaged, target);
        }

        public virtual void DamageShield(BattleCharacter target, int value)
        {
            if (target == null)
                return;

            target.shield -= value;
            target.shield = Mathf.Max(target.shield, 0);

            //target.shield_auto -= value;
            //target.shield_auto = Mathf.Max(target.shield_auto, 0);
        }

        public virtual void KillCharacter(BattleCharacter character)
        {
            if (!character.IsDead())
            {
                character.damage = character.GetHPMax();
                character.is_dead = true;
                RemoveFromInitiativeCurrent(character);
                RemoveFromInitiativeNext(character);

                TriggerCharacterAbilityType(AbilityTrigger.OnDeath, character);
                TriggerAnyCharacterAbilityType(AbilityTrigger.OnDeathOther);
            }
        }

        public int RollRandomValue(int dice)
        {
            return RollRandomValue(1, dice + 1);
        }

        public virtual int RollRandomValue(int min, int max)
        {
            battle_data.rolled_value = random.Next(min, max);
            onRollValue?.Invoke(battle_data.rolled_value);
            resolve_queue.SetDelay(1f);
            return battle_data.rolled_value;
        }

        //--- Abilities --

        public virtual void TriggerAnyCharacterAbilityType(AbilityTrigger type, Card triggerer = null)
        {
            foreach (BattleCharacter character in battle_data.characters)
            {
                TriggerCharacterAbilityType(type, character, triggerer);
            }
        }

        public virtual void TriggerCharacterAbilityType(AbilityTrigger type, BattleCharacter caster, Card triggerer = null)
        {
            if (!caster.CanTriggerAbilities())
                return;

            foreach (AbilityData iability in caster.GetAbilities())
            {
                if (iability && iability.trigger == type)
                {
                    TriggerAbility(iability, caster, triggerer, triggerer);
                }
            }

            foreach (Card acard in caster.cards_power)
            {
                foreach (AbilityData iability in acard.GetAbilities())
                {
                    if (iability && iability.trigger == type)
                    {
                        TriggerAbility(iability, caster, acard, triggerer);
                    }
                }
            }

            foreach (Card acard in caster.cards_item)
            {
                if (acard.CardData.item_type == ItemType.ItemPassive)
                {
                    foreach (AbilityData iability in acard.GetAbilities())
                    {
                        if (iability && iability.trigger == type)
                        {
                            TriggerAbility(iability, caster, acard, triggerer);
                        }
                    }
                }
            }
        }

        public virtual void TriggerCardAbilityType(AbilityTrigger type, Card card)
        {
            BattleCharacter caster = battle_data.GetCharacter(card.owner_uid);
            foreach (AbilityData iability in card.GetAbilities())
            {
                if (iability && iability.trigger == type)
                {
                    TriggerAbility(iability, caster, card);
                }
            }
        }

        public virtual void TriggerAbility(AbilityData iability, BattleCharacter caster, Card card, Card triggerer = null)
        {
            if (iability.AreTriggerConditionsMet(battle_data, caster, card, triggerer))
            {
                resolve_queue.AddAbility(iability, caster, card, ResolveAbility);
            }
        }

        //Resolve a card ability, may stop to ask for target
        protected virtual void ResolveAbility(AbilityData iability, BattleCharacter caster, Card card)
        {
            //Debug.Log("Trigger Ability " + iability.id + " : " + card.card_id);

            onAbilityStart?.Invoke(iability, card);
            battle_data.ability_triggerer = card != null ? card.uid : "";
            battle_data.ability_triggerer = card != null ? card.uid : "";
            battle_data.ability_played.Add(iability.id);

            bool is_selector = ResolveCardAbilitySelector(iability, caster, card);
            if (is_selector)
                return; //Wait for player to select

            ResolveCardAbilityPlayTarget(iability, caster, card);
            ResolveCardAbilityCharacters(iability, caster, card);
            ResolveCardAbilityCards(iability, caster, card);
            ResolveCardAbilitySlots(iability, caster, card);
            ResolveCardAbilityCardData(iability, caster, card);
            ResolveCardAbilityNoTarget(iability, caster, card);
            AfterAbilityResolved(iability, caster, card);
        }

        protected virtual bool ResolveCardAbilitySelector(AbilityData iability, BattleCharacter caster, Card card)
        {
            if (iability.target == AbilityTarget.SelectTarget)
            {
                //Wait for target
                GoToSelectTarget(iability, caster, card);
                return true;
            }
            else if (iability.target == AbilityTarget.CardSelector)
            {
                GoToSelectorCard(iability, caster, card);
                return true;
            }
            else if (iability.target == AbilityTarget.ChoiceSelector)
            {
                GoToSelectorChoice(iability, caster, card);
                return true;
            }
            return false;
        }

        protected virtual void ResolveCardAbilityPlayTarget(AbilityData iability, BattleCharacter caster, Card card)
        {
            if (card != null && iability.IsPlayTarget())
            {
                Slot slot = battle_data.play_target;
                BattleCharacter target = battle_data.GetSlotCharacter(slot);
                if (target != null)
                {
                    if (iability.AreTargetConditionsMet(battle_data, caster, card, target))
                    {
                        battle_data.last_targeted = target.uid;
                        ResolveEffectTarget(iability, caster, card, target);
                    }
                }
                else
                {
                    if (iability.AreTargetConditionsMet(battle_data, caster, card, slot))
                        ResolveEffectTarget(iability, caster, card, slot);
                }
            }
        }

        protected virtual void ResolveCardAbilityCharacters(AbilityData iability, BattleCharacter caster, Card card)
        {
            //Get Player Targets based on conditions
            List<BattleCharacter> targets = iability.GetCharacterTargets(battle_data, caster, card, battle_data.GetCharacterListSwap());

            //Resolve effects
            foreach (BattleCharacter target in targets)
            {
                ResolveEffectTarget(iability, caster, card, target);
            }
        }

        protected virtual void ResolveCardAbilityCards(AbilityData iability, BattleCharacter caster, Card card)
        {
            //Get Cards Targets based on conditions
            List<Card> targets = iability.GetCardTargets(battle_data, caster, card, battle_data.GetCardListSwap());

            //Resolve effects
            foreach (Card target in targets)
            {
                ResolveEffectTarget(iability, caster, card, target);
            }
        }

        protected virtual void ResolveCardAbilitySlots(AbilityData iability, BattleCharacter caster, Card card)
        {
            //Get Slot Targets based on conditions
            List<Slot> targets = iability.GetSlotTargets(battle_data, caster, card, battle_data.GetSlotListSwap());

            //Resolve effects
            foreach (Slot target in targets)
            {
                ResolveEffectTarget(iability, caster, card, target);
            }
        }

        protected virtual void ResolveCardAbilityCardData(AbilityData iability, BattleCharacter caster, Card card)
        {
            //Get Cards Targets based on conditions
            List<CardData> targets = iability.GetCardDataTargets(battle_data, caster, card, battle_data.GetCardDataListSwap());

            //Resolve effects
            foreach (CardData target in targets)
            {
                ResolveEffectTarget(iability, caster, card, target);
            }
        }

        protected virtual void ResolveCardAbilityNoTarget(AbilityData iability, BattleCharacter caster, Card card)
        {
            if (iability.target == AbilityTarget.None)
                iability.DoEffects(this, caster, card);
        }

        protected virtual void ResolveEffectTarget(AbilityData iability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            iability.DoEffects(this, caster, card, target);

            onAbilityTargetCharacter?.Invoke(iability, card, target);
        }

        protected virtual void ResolveEffectTarget(AbilityData iability, BattleCharacter caster, Card card, Card target)
        {
            iability.DoEffects(this, caster, card, target);

            onAbilityTargetCard?.Invoke(iability, card, target);
        }

        protected virtual void ResolveEffectTarget(AbilityData iability, BattleCharacter caster, Card card, Slot target)
        {
            iability.DoEffects(this, caster, card, target);

            onAbilityTargetSlot?.Invoke(iability, card, target);
        }

        protected virtual void ResolveEffectTarget(AbilityData iability, BattleCharacter caster, Card card, CardData target)
        {
            iability.DoEffects(this, caster, card, target);
        }

        protected virtual void AfterAbilityResolved(AbilityData iability, BattleCharacter caster, Card card)
        {
            //Recalculate and clear
            UpdateOngoing();

            //Chain ability
            if (iability.target != AbilityTarget.ChoiceSelector && battle_data.phase != BattlePhase.Ended)
            {
                foreach (AbilityData chain_ability in iability.chain_abilities)
                {
                    if (chain_ability != null)
                    {
                        TriggerAbility(chain_ability, caster, card);
                    }
                }
            }

            if (resolve_queue.Count() == 0)
                CheckForWinner();

            onAbilityEnd?.Invoke(iability, card);
            resolve_queue.ResolveAll(0.5f);
            RefreshBattle();
        }

        protected virtual void AfterPlayCardResolved(Card card)
        {
            //Recalculate and clear
            UpdateOngoing();
            CheckForWinner();
            RefreshBattle();

            resolve_queue.ResolveAll(0.1f);
        }

        //This function is called often to update status/stats affected by ongoing abilities
        //It basically first reset the bonus to 0 (CleanOngoing) and then recalculate it to make sure it it still present
        //Only cards in hand and on board are updated in this way
        public virtual void UpdateOngoing()
        {
            Profiler.BeginSample("Update Ongoing");
            for (int p = 0; p < battle_data.characters.Count; p++)
            {
                BattleCharacter character = battle_data.characters[p];
                character.ClearOngoing();

                for (int c = 0; c < character.cards_hand.Count; c++)
                    character.cards_hand[c].ClearOngoing();
            }

            for (int p = 0; p < battle_data.characters.Count; p++)
            {
                BattleCharacter character = battle_data.characters[p];
                UpdateOngoingAbilities(character);

                foreach (Card card in character.cards_power)
                    UpdateOngoingAbilities(character, card);

                foreach (Card card in character.cards_item)
                    UpdateOngoingAbilities(character, card);

                foreach (Card card in character.cards_hand)
                    UpdateOngoingAbilities(character, card, true);
            }

            //Stats bonus
            for (int p = 0; p < battle_data.characters.Count; p++)
            {
                BattleCharacter character = battle_data.characters[p];
                foreach (CardStatus status in character.status)
                    AddOngoingStatusBonus(character, status);
                foreach (CardStatus status in character.ongoing_status)
                    AddOngoingStatusBonus(character, status);

                foreach (Card card in character.cards_hand)
                {
                    foreach (CardStatus status in card.status)
                        AddOngoingStatusBonus(card, status);
                    foreach (CardStatus status in card.ongoing_status)
                        AddOngoingStatusBonus(card, status);
                }
            }

            //Kill stuff with 0 hp
            for (int p = 0; p < battle_data.characters.Count; p++)
            {
                BattleCharacter character = battle_data.characters[p];
                if (!character.IsDead() && character.GetHP() <= 0)
                    KillCharacter(character);
            }

            //Clear cards
            for (int c = 0; c < cards_to_clear.Count; c++)
                cards_to_clear[c].Clear();
            cards_to_clear.Clear();

            Profiler.EndSample();
        }

        protected virtual void UpdateOngoingAbilities(BattleCharacter character)
        {
            if (character == null)
                return;

            List<AbilityData> abilities = character.GetAbilities();
            for (int a = 0; a < abilities.Count; a++)
            {
                AbilityData ability = abilities[a];
                if (ability != null && ability.trigger == AbilityTrigger.Ongoing && ability.AreTriggerConditionsMet(battle_data, character, null))
                {
                    if (ability.target == AbilityTarget.CharacterSelf)
                    {
                        if (ability.AreTargetConditionsMet(battle_data, character, null, character))
                        {
                            ability.DoOngoingEffects(this, character, null, character);
                        }
                    }

                    if (ability.target == AbilityTarget.AllCharacters)
                    {
                        for (int tp = 0; tp < battle_data.characters.Count; tp++)
                        {
                            BattleCharacter ocharacter = battle_data.characters[tp];
                            if (ability.AreTargetConditionsMet(battle_data, character, null, ocharacter))
                            {
                                ability.DoOngoingEffects(this, character, null, ocharacter);
                            }
                        }
                    }

                    if (ability.target == AbilityTarget.AllCardsAllPiles || ability.target == AbilityTarget.AllCardsHand)
                    {
                        for (int tp = 0; tp < battle_data.characters.Count; tp++)
                        {
                            //Looping on all cards is very slow, since there are no ongoing effects that works out of board/hand we loop on those only
                            BattleCharacter tcharacter = battle_data.characters[tp];

                            //Hand Cards
                            for (int tc = 0; tc < tcharacter.cards_hand.Count; tc++)
                            {
                                Card tcard = tcharacter.cards_hand[tc];
                                if (ability.AreTargetConditionsMet(battle_data, character, null, tcard))
                                {
                                    ability.DoOngoingEffects(this, character, null, tcard);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void UpdateOngoingAbilities(BattleCharacter charact, Card card, bool self_only = false)
        {
            if (card == null)
                return;

            List<AbilityData> abilities = card.GetAbilities();
            for (int a = 0; a < abilities.Count; a++)
            {
                AbilityData ability = abilities[a];
                if (ability != null && ability.trigger == AbilityTrigger.Ongoing && ability.AreTriggerConditionsMet(battle_data, charact, card))
                {
                    if (ability.target == AbilityTarget.CharacterSelf && !self_only)
                    {
                        if (ability.AreTargetConditionsMet(battle_data, charact, card, charact))
                        {
                            ability.DoOngoingEffects(this, charact, card, charact);
                        }
                    }

                    if (ability.target == AbilityTarget.CardSelf)
                    {
                        if (ability.AreTargetConditionsMet(battle_data, charact, card, card))
                        {
                            ability.DoOngoingEffects(this, charact, card, card);
                        }
                    }

                    if (ability.target == AbilityTarget.AllCharacters)
                    {
                        for (int tc = 0; tc < battle_data.characters.Count; tc++)
                        {
                            BattleCharacter tcharacter = battle_data.characters[tc];
                            if (ability.AreTargetConditionsMet(battle_data, charact, card, tcharacter))
                            {
                                ability.DoOngoingEffects(this, charact, card, tcharacter);
                            }
                        }
                    }

                    if (ability.target == AbilityTarget.AllCardsAllPiles || ability.target == AbilityTarget.AllCardsHand)
                    {
                        for (int tc = 0; tc < battle_data.characters.Count; tc++)
                        {
                            //Looping on all cards is very slow, since there are no ongoing effects that works out of board/hand we loop on those only
                            BattleCharacter tcharacter = battle_data.characters[tc];

                            //Hand Cards
                            if (ability.target == AbilityTarget.AllCardsAllPiles || ability.target == AbilityTarget.AllCardsHand)
                            {
                                for (int t = 0; t < tcharacter.cards_hand.Count; t++)
                                {
                                    Card tcard = tcharacter.cards_hand[t];
                                    if (ability.AreTargetConditionsMet(battle_data, charact, card, tcard))
                                    {
                                        ability.DoOngoingEffects(this, charact, card, tcard);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void AddOngoingStatusBonus(BattleCharacter character, CardStatus status)
        {
            if (status.effect == StatusEffect.SpeedBonus)
                character.speed_ongoing += status.value;
            if (status.effect == StatusEffect.HandBonus)
                character.hand_ongoing += status.value;
        }

        protected virtual void AddOngoingStatusBonus(Card card, CardStatus status)
        {
            if (status.effect == StatusEffect.ManaCostBonus)
                card.mana_ongoing += status.value;
        }

        //---- Resolve Selector -----

        public virtual void SelectCard(Card target)
        {
            if (battle_data.selector == SelectorType.None)
                return;

            BattleCharacter caster = battle_data.GetCharacter(battle_data.selector_caster_uid);
            Card card = battle_data.GetCard(battle_data.selector_card_uid);
            AbilityData ability = AbilityData.Get(battle_data.selector_ability_id);

            if (caster == null || target == null || ability == null)
                return;

            if (battle_data.selector == SelectorType.SelectTarget)
            {
                if (!ability.CanTarget(battle_data, caster, card, target))
                    return; //Can't target that target

                battle_data.selector = SelectorType.None;
                battle_data.last_targeted = target.uid;
                ResolveEffectTarget(ability, caster, card, target);
                AfterAbilityResolved(ability, caster, card);
                resolve_queue.ResolveAll();
            }

            if (battle_data.selector == SelectorType.SelectorCard)
            {
                if (!ability.IsCardSelectionValid(battle_data, caster, card, target, battle_data.GetCardListSwap()))
                    return; //Supports conditions and filters

                battle_data.selector = SelectorType.None;
                battle_data.last_targeted = target.uid;
                ResolveEffectTarget(ability, caster, card, target);
                AfterAbilityResolved(ability, caster, card);
                resolve_queue.ResolveAll();
            }
        }

        public virtual void SelectCharacter(BattleCharacter target)
        {
            if (battle_data.selector == SelectorType.None)
                return;

            BattleCharacter caster = battle_data.GetCharacter(battle_data.selector_caster_uid);
            Card card = battle_data.GetCard(battle_data.selector_card_uid);
            AbilityData ability = AbilityData.Get(battle_data.selector_ability_id);

            if (caster == null || target == null || ability == null)
                return;

            if (battle_data.selector == SelectorType.SelectTarget)
            {
                if (!ability.CanTarget(battle_data, caster, card, target))
                    return; //Can't target that target

                battle_data.selector = SelectorType.None;
                battle_data.last_targeted = target.uid;
                ResolveEffectTarget(ability, caster, card, target);
                AfterAbilityResolved(ability, caster, card);
                resolve_queue.ResolveAll();
            }
        }

        public virtual void SelectSlot(Slot target)
        {
            if (battle_data.selector == SelectorType.None)
                return;

            BattleCharacter caster = battle_data.GetCharacter(battle_data.selector_caster_uid);
            Card card = battle_data.GetCard(battle_data.selector_card_uid);
            AbilityData ability = AbilityData.Get(battle_data.selector_ability_id);

            if (caster == null || ability == null || !target.IsValid())
                return;

            if (battle_data.selector == SelectorType.SelectTarget)
            {
                if(!ability.CanTarget(battle_data, caster, card, target))
                    return; //Conditions not met

                battle_data.selector = SelectorType.None;
                ResolveEffectTarget(ability, caster, card, target);
                AfterAbilityResolved(ability, caster, card);
                resolve_queue.ResolveAll();
            }
        }

        public virtual void SelectChoice(int choice)
        {
            if (battle_data.selector == SelectorType.None)
                return;

            BattleCharacter caster = battle_data.GetCharacter(battle_data.selector_caster_uid);
            Card card = battle_data.GetCard(battle_data.selector_card_uid);
            AbilityData ability = AbilityData.Get(battle_data.selector_ability_id);

            if (caster == null || ability == null || choice < 0)
                return;

            if (battle_data.selector == SelectorType.SelectorChoice && ability.target == AbilityTarget.ChoiceSelector)
            {
                if (choice >= 0 && choice < ability.chain_abilities.Length)
                {
                    AbilityData achoice = ability.chain_abilities[choice];
                    if (achoice != null && achoice.AreTriggerConditionsMet(battle_data, caster, card))
                    {
                        battle_data.selector = SelectorType.None;
                        AfterAbilityResolved(ability, caster, card);
                        ResolveAbility(achoice, caster, card);
                        resolve_queue.ResolveAll();
                    }
                }
            }
        }

        public virtual void CancelSelection()
        {
            if (battle_data.selector != SelectorType.None)
            {
                //End selection
                battle_data.selector = SelectorType.None;
                onSelectorSelect?.Invoke();
                RefreshBattle();
            }
        }

        //-----Trigger Selector-----

        protected virtual void GoToSelectTarget(AbilityData iability, BattleCharacter caster, Card card)
        {
            battle_data.selector = SelectorType.SelectTarget;
            battle_data.selector_player_id = caster.player_id;
            battle_data.selector_ability_id = iability.id;
            battle_data.selector_caster_uid = caster.uid;
            battle_data.selector_card_uid = card != null ? card.uid : "";
            onSelectorStart?.Invoke();
            RefreshBattle();
        }

        protected virtual void GoToSelectorCard(AbilityData iability, BattleCharacter caster, Card card)
        {
            battle_data.selector = SelectorType.SelectorCard;
            battle_data.selector_player_id = caster.player_id;
            battle_data.selector_ability_id = iability.id;
            battle_data.selector_caster_uid = caster.uid;
            battle_data.selector_card_uid = card != null ? card.uid : "";
            onSelectorStart?.Invoke();
            RefreshBattle();
        }

        protected virtual void GoToSelectorChoice(AbilityData iability, BattleCharacter caster, Card card)
        {
            battle_data.selector = SelectorType.SelectorChoice;
            battle_data.selector_player_id = caster.player_id;
            battle_data.selector_ability_id = iability.id;
            battle_data.selector_caster_uid = caster.uid;
            battle_data.selector_card_uid = card != null ? card.uid : "";
            onSelectorStart?.Invoke();
            RefreshBattle();
        }

        //-------------

        public virtual void RefreshWorld()
        {
            refreshWorld?.Invoke();
        }

        public virtual void RefreshBattle()
        {
            refreshBattle?.Invoke();
        }

        public virtual void ClearResolve()
        {
            resolve_queue.Clear();
        }

        public virtual bool IsResolving()
        {
            return resolve_queue.IsResolving();
        }

        public virtual bool IsGameStarted()
        {
            return battle_data.HasStarted();
        }

        public virtual bool IsGameEnded()
        {
            return battle_data.HasEnded();
        }

        public virtual Player GetPlayer(int player_id)
        {
            return world_data.GetPlayer(player_id);
        }

        public virtual Battle GetBattleData()
        {
            return battle_data;
        }

        public virtual World GetWorldData()
        {
            return world_data;
        }

        public System.Random GetRandom()
        {
            return random;
        }

        public World WorldData { get { return world_data; } }
        public Battle BattleData { get { return battle_data; } }
        public ResolveQueue ResolveQueue { get { return resolve_queue; } }
    }
}