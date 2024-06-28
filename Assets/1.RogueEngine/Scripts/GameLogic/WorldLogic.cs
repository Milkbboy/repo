using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RogueEngine.Gameplay
{

    public class WorldLogic 
    {

        public UnityAction onGameStart;
        public UnityAction onGameEnd;
        public UnityAction onRefreshWorld;

        public UnityAction<Champion, MapLocation> onMove;
        public UnityAction<Champion, EventData> onEventChoice;
        public UnityAction<Champion, CardData> onRewardChoice;
        public UnityAction<Champion, AbilityData> onMapAbility;

        private BattleLogic battle_logic;
        private World world_data;

        public Queue<EventQueueItem> event_queue = new Queue<EventQueueItem>();

        public WorldLogic(BattleLogic logic, World world) { this.battle_logic = logic; this.world_data = world; }

        public virtual void SetData(World world) { 
            this.world_data = world;
            battle_logic.SetData(world);
        }

        public virtual void CreateGame(ScenarioData scenario, bool online, string filename, string title)
        {
            if (world_data.state != WorldState.None && world_data.state != WorldState.Setup)
                return;

            world_data.state = WorldState.Setup;
            world_data.filename = filename;
            world_data.title = title;
            world_data.online = online;
            world_data.scenario_id = scenario.id;
            world_data.map_id = scenario.maps[0].id;
            world_data.map_location_id = 0;
            world_data.champions.Clear();

            RefreshWorld();
        }

        public virtual void CreateChampion(int player_id, ChampionData champion, int slot_x)
        {
            if (world_data.state != WorldState.Setup)
                return;

            ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
            Player player = world_data.GetPlayer(player_id);
            if (scenario != null && player != null && slot_x >= Slot.x_min && slot_x <= Slot.x_max)
            {
                Champion champ = Champion.Create(champion, player_id);
                champ.position = slot_x;

                //Starting cards/items
                foreach (CardData icard in champion.start_cards)
                {
                    champ.AddCard(icard);
                }
                foreach (CardData item in champion.start_items)
                {
                    champ.AddItem(item);
                }

                world_data.RemoveChampion(slot_x);
                world_data.AddChampion(champ);
                RefreshWorld();
            }
        }

        public virtual void StartGame(int seed)
        {
            if (world_data.state != WorldState.Setup)
                return; //Already started

            ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
            if (scenario == null || world_data.champions.Count == 0)
                return; //Scenario not set

            Debug.Log("Start Scenario: " + world_data.scenario_id);
            world_data.seed = seed;
            world_data.state = WorldState.Map;
            world_data.event_id = "";

            foreach (Player player in world_data.players)
            {
                player.gold = scenario.start_gold;
            }

            foreach (Champion champ in world_data.champions)
            {
                champ.xp = scenario.start_xp;

                foreach (CardData card in scenario.start_cards)
                {
                    if (card != null)
                        champ.AddCard(card);
                }
                foreach (CardData item in scenario.start_items)
                {
                    if (item != null)
                        champ.AddItem(item);
                }
            }

            GenerateMaps();
            UpdateOngoing();

            onGameStart?.Invoke();
            RefreshWorld();
        }

        public virtual void StartTest(WorldState test_state)
        {
            if (test_state == WorldState.None)
                return; //No test

            world_data.state = test_state;

            if (test_state == WorldState.Battle)
            {
                StartBattle(GameplayData.Get().test_battle);
            }

            //StartBattle(GameplayData.Get().test_battle);
            //GoToRewards();

            RefreshWorld();
        }

        public virtual void GenerateMaps()
        {
            System.Random rand = new System.Random(world_data.seed);
            ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
            foreach (MapData mapdata in scenario.maps)
            {
                Map map = new Map(mapdata, rand.Next());
                map.GenerateMap(world_data);
                world_data.maps.Add(map);
            }
        }

        public virtual void CheckIfDead()
        {
            if (world_data.AreAllChampionsDead())
                EndGame(false);
        }
        
        public virtual void EndGame(bool victory)
        {
            if (world_data.state == WorldState.Ended)
                return;

            world_data.state = WorldState.Ended;
            world_data.completed = victory;
            onGameEnd?.Invoke();
            RefreshWorld();
        }

        //-----------------------

        public virtual void Update(float delta)
        {
            if (world_data.state == WorldState.Map)
            {
                if (event_queue.Count > 0)
                {
                    EventQueueItem item = event_queue.Dequeue();
                    Champion champion = world_data.GetChampion(item.champion_uid);
                    EventData evt = EventData.Get(item.event_id);
                    TriggerEvent(champion, evt);
                    RefreshWorld();
                }
            }
        }

        public virtual void UpdateOngoing()
        {
            foreach (Champion champion in world_data.champions)
                champion.ClearOngoing();

            foreach (Champion champion in world_data.champions)
            {
                UpdateOngoingAbilities(champion);

                foreach (ChampionItem item in champion.inventory)
                {
                    if (item.CardData.item_type == ItemType.ItemPassive)
                    {
                        UpdateOngoingAbilities(champion, item);
                    }
                }
            }
        }

        protected virtual void UpdateOngoingAbilities(Champion character)
        {
            if (character == null)
                return;

            AbilityData[] abilities = character.ChampionData.abilities;
            for (int a = 0; a < abilities.Length; a++)
            {
                AbilityData ability = abilities[a];
                if (ability != null && ability.trigger == AbilityTrigger.Ongoing && ability.AreMapTriggerConditionsMet(world_data, character, null))
                {
                    if (ability.target == AbilityTarget.CharacterSelf)
                    {
                        if (ability.AreMapTargetConditionsMet(world_data, character, null, character))
                        {
                            ability.DoMapOngoingEffects(this, character, null, character);
                        }
                    }

                    if (ability.target == AbilityTarget.AllCharacters)
                    {
                        for (int tp = 0; tp < world_data.champions.Count; tp++)
                        {
                            Champion tcharacter = world_data.champions[tp];
                            if (ability.AreMapTargetConditionsMet(world_data, character, null, tcharacter))
                            {
                                ability.DoMapOngoingEffects(this, character, null, tcharacter);
                            }
                        }
                    }

                }
            }
        }

        protected virtual void UpdateOngoingAbilities(Champion character, ChampionItem item)
        {
            if (item == null)
                return;

            AbilityData[] abilities = item.CardData.abilities;
            for (int a = 0; a < abilities.Length; a++)
            {
                AbilityData ability = abilities[a];
                if (ability != null && ability.trigger == AbilityTrigger.Ongoing && ability.AreMapTriggerConditionsMet(world_data, character, item))
                {
                    if (ability.target == AbilityTarget.CharacterSelf)
                    {
                        if (ability.AreMapTargetConditionsMet(world_data, character, item, character))
                        {
                            ability.DoMapOngoingEffects(this, character, item, character);
                        }
                    }

                    if (ability.target == AbilityTarget.AllCharacters)
                    {
                        for (int tc = 0; tc < world_data.champions.Count; tc++)
                        {
                            Champion tcharacter = world_data.champions[tc];
                            if (ability.AreMapTargetConditionsMet(world_data, character, item, tcharacter))
                            {
                                ability.DoMapOngoingEffects(this, character, item, tcharacter);
                            }
                        }
                    }
                }
            }
        }

        public virtual void TriggerAbilityType(AbilityTrigger trigger)
        {
            foreach (Champion champion in world_data.champions)
            {
                TriggerAbilityType(trigger, champion);
            }
        }

        public virtual void TriggerAbilityType(AbilityTrigger trigger, Champion champion)
        {
            foreach (ChampionItem item in champion.inventory)
            {
                if (item.CardData.item_type == ItemType.ItemPassive)
                {
                    foreach (AbilityData ability in item.CardData.abilities)
                    {
                        if (ability.trigger == trigger && ability.AreMapTriggerConditionsMet(world_data, champion, item))
                            TriggerAbility(ability, champion, item);
                    }
                }
            }
        }

        public virtual void TriggerAbility(AbilityData ability, Champion champion, ChampionItem item)
        {
            if (champion == null || ability == null || item == null)
                return;

            if (ability.AreMapTriggerConditionsMet(world_data, champion, item))
            {
                List<Champion> targets = ability.GetWorldCharacterTargets(world_data, champion, item);

                //Resolve effects
                foreach (Champion target in targets)
                {
                    ability.DoMapEffects(this, champion, item, target);
                }

                onMapAbility?.Invoke(champion, ability);
                CheckIfDead();
            }
        }

        //-----------------------

        public virtual void Move(Champion champion, MapLocation loc)
        {
            if (world_data.CanMoveTo(loc))
            {
                world_data.state = WorldState.Map;
                world_data.map_location_id = loc.ID;
                world_data.AddExplored(loc);
                TriggerMapEvent(champion, loc);
                onMove?.Invoke(champion, loc);
                RefreshWorld();
            }
        }

        public virtual void SelectCardReward(Champion champion, string card_id)
        {
            if (world_data.state == WorldState.Reward && champion != null)
            {
                CardData icard = CardData.Get(card_id);
                if (icard != null && champion.reward.cards.Contains(card_id))
                {
                    champion.AddCard(icard);
                }

                champion.reward.cards.Clear();
                onRewardChoice?.Invoke(champion, icard);
                RefreshWorld();
            }

            if (world_data.state == WorldState.LevelUp && champion != null && champion.uid == world_data.event_champion)
            {
                CardData icard = CardData.Get(card_id);
                if (icard != null && champion.reward.cards.Contains(card_id))
                {
                    champion.AddCard(icard);
                }

                champion.reward.cards.Clear();
                onRewardChoice?.Invoke(champion, icard);
                RefreshWorld();
            }
        }

        public virtual void SelectItemReward(Champion champion, string item_id)
        {
            if (world_data.state == WorldState.Reward && champion != null)
            {
                CardData icard = CardData.Get(item_id);
                if (icard != null && champion.reward.items.Contains(item_id))
                {
                    champion.AddItem(icard);
                }

                champion.reward.items.Clear();
                onRewardChoice?.Invoke(champion, icard);
                RefreshWorld();
            }
        }

        public virtual void SelectEventChoice(Champion champion, string choice_id)
        {
            if (world_data.state == WorldState.EventChoice && champion != null)
            {
                EventChoice evt = EventChoice.Get(world_data.event_id);
                EventData choice = EventData.Get(choice_id);
                if (evt != null && choice != null)
                {
                    world_data.state = WorldState.Map;
                    TriggerEvent(champion, choice);
                    onEventChoice?.Invoke(champion, choice);
                    CheckIfDead();
                    RefreshWorld();
                }
            }
        }

        public virtual void UpgradeCard(Champion champion, ChampionCard card)
        {
            if (champion != null && card != null && world_data.state == WorldState.Upgrade)
            {
                Player player = world_data.GetPlayer(champion.player_id);
                int cost = world_data.GetUpgradeCost(card.CardData, card.level + 1);
                bool can_pay = player.gold >= cost || champion.free_upgrades > 0;
                if (can_pay && card.level < card.CardData.level_max)
                {
                    if (champion.free_upgrades > 0)
                        champion.free_upgrades--;
                    else
                        player.gold -= cost;

                    champion.UpgradeCard(card.uid); 
                    RefreshWorld();
                }
            }
        }

        public virtual void TrashCard(Champion champion, ChampionCard card)
        {
            if (champion != null && card != null && world_data.state == WorldState.Trash)
            {
                champion.RemoveCard(card.uid);
                CompleteAction(champion);
                RefreshWorld();
            }
        }

        public virtual void LevelUp(Champion champion)
        {
            if(champion != null && world_data.state == WorldState.Map)
            {
                if (champion.CanLevelUp())
                {
                    int xp_cost = GameplayData.Get().GetXpForNextLevel(champion.level);
                    champion.xp -= xp_cost;
                    champion.level++;
                    champion.hp += champion.ChampionData.level_up_hp;
                    champion.speed += champion.ChampionData.level_up_speed;
                    champion.hand += champion.ChampionData.level_up_hand;
                    champion.energy += champion.ChampionData.level_up_energy;
                    GainLevelUpRewards(champion); //Just gain regular card rewards when level up
                }
            }
        }

        public virtual void BuyCard(Champion champion, CardData card)
        {
            if (champion != null && card != null && world_data.state == WorldState.Shop)
            {
                Player player = world_data.GetPlayer(champion.player_id);
                int cost = world_data.GetBuyCost(card);
                if (player.gold >= cost && world_data.shop_cards.Contains(card.id))
                {
                    player.gold -= cost;
                    world_data.shop_cards.Remove(card.id);
                    champion.AddCard(card);
                    RefreshWorld();
                }
            }
        }

        public virtual void BuyItem(Champion champion, CardData item)
        {
            if (champion != null && item != null && world_data.state == WorldState.Shop)
            {
                Player player = world_data.GetPlayer(champion.player_id);
                int cost = world_data.GetBuyCost(item);
                if (player.gold >= cost && world_data.shop_items.Contains(item.id))
                {
                    player.gold -= cost;
                    world_data.shop_items.Remove(item.id);
                    champion.AddItem(item);
                    RefreshWorld();
                }
            }
        }

        public virtual void SellItem(Champion champion, CardData item)
        {
            if (champion != null && item != null && world_data.state == WorldState.Shop)
            {
                Player player = world_data.GetPlayer(champion.player_id);
                int cost = world_data.GetSellCost(item);
                if (champion.HasItem(item))
                {
                    player.gold += cost;
                    champion.RemoveItem(item);
                    RefreshWorld();
                }
            }
        }

        public virtual void UseItem(Champion champion, ChampionItem item)
        {
            if (champion != null && item != null && world_data.state == WorldState.Map)
            {
                champion.RemoveItem(item.card_id);
                //Not implemented

                RefreshWorld();
            }
        }

        public virtual void GainChampion(ChampionData character, int player_id)
        {
            Player player = world_data.GetPlayer(player_id);
            int slot_x = world_data.GetEmptySlotPos();
            if (player != null && slot_x > 0)
            {
                Champion champ = Champion.Create(character, player_id);
                champ.position = slot_x;

                //Starting cards/items
                foreach (CardData icard in character.start_cards)
                {
                    champ.AddCard(icard);
                }
                foreach (CardData item in character.start_items)
                {
                    champ.AddItem(item);
                }

                world_data.AddChampion(champ);
                RefreshWorld();
            }
        }

        public virtual void RemoveChampion(Champion character)
        {
            if (character != null)
            {
                world_data.RemoveChampion(character.uid);
            }
        }

        public virtual void GainAlly(CharacterData character, int player_id)
        {
            Player player = world_data.GetPlayer(player_id);
            int slot_x = world_data.GetEmptySlotPos();
            if (player != null && slot_x > 0)
            {
                CharacterAlly ally = new CharacterAlly(character, player_id);
                ally.position = slot_x;
                world_data.AddAlly(ally);
                RefreshWorld();
            }
        }

        public virtual void RemoveAlly(CharacterAlly character)
        {
            if (character != null)
            {
                world_data.RemoveAlly(character.uid);
            }
        }

        //----------------

        public virtual void TriggerMapEvent(Champion champion, MapLocation loc)
        {
            if (loc != null)
            {
                EventData evt = EventData.Get(loc.evt_id);
                TriggerEvent(champion, evt);
            }
        }

        public virtual void TriggerEvent(Champion champion, EventData evt)
        {
            if (evt == null || world_data.state == WorldState.Ended)
                return;

            if (evt != null && champion != null && evt.AreEventsConditionMet(world_data, champion))
            {
                world_data.event_id = evt.id;
                world_data.event_champion = champion.uid;
                UpdateOngoing();
                evt.DoEvent(this, champion);
                CheckIfDead();
            }
        }

        //Simple event that just shows text to player until the player clicks OK
        public virtual void TriggerTextEvent(Champion champion, string text)
        {
            if (world_data.state == WorldState.Ended)
                return;
            if (string.IsNullOrEmpty(text))
                return;

            world_data.state = WorldState.EventText;
            world_data.event_champion = champion.uid;
            world_data.event_id = "";
            world_data.event_text = text;
        }

        //Add event to event queue to trigger it after current one
        public virtual void TriggerEventNext(Champion champion, EventData evt)
        {
            if (evt == null || champion == null || world_data.state == WorldState.Ended)
                return;

            EventQueueItem item = new EventQueueItem();
            item.champion_uid = champion.uid;
            item.event_id = evt.id;
            event_queue.Enqueue(item);
        }

        public virtual void CompleteAction(Champion champion)
        {
            champion.action_completed = true;
            champion.reward = null;
            if (world_data.AreAllActionsCompleted())
                StopMapEvent();
            RefreshWorld();
        }

        //Complete actions and go to back to map state (for example, finish shopping or finish upgrading card)
        public virtual void CompleteAction(int player_id)
        {
            world_data.SetActionCompleted(player_id, true);
            if (world_data.AreAllActionsCompleted())
                StopMapEvent();
            RefreshWorld();
        }

        public virtual void StopMapEvent()
        {
            if (world_data.state == WorldState.Ended)
                return;

            world_data.state = WorldState.Map;
            world_data.event_id = "";
            world_data.event_text = "";
            world_data.event_champion = "";

            MapLocation location = world_data.GetCurrentLocation();
            MapData map = MapData.Get(world_data.map_id);
            if (location != null && location.depth == map.depth)
                GoToNextMap();

            UpdateOngoing();
            RefreshWorld();
        }



        public virtual void GoToNextMap()
        {
            world_data.map_index++;

            ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
            if (world_data.map_index < scenario.maps.Length)
            {
                MapData map = scenario.maps[world_data.map_index];
                world_data.map_id = map.id;
                world_data.map_location_id = 0;
                world_data.state = WorldState.Map;

                RefreshWorld();
            }
            else
            {
                EndGame(true);
            }
        }

        //--------------------

        public virtual void StartBattle(EventBattle battle)
        {
            world_data.state = WorldState.Battle;
            world_data.battle = new Battle(battle.id);
            world_data.event_id = battle.id;
            battle_logic.SetData(world_data.battle);
            battle_logic.StartBattle(world_data, battle);

            //EndBattle(1);
        }

        public virtual void FleeBattle(int player_id)
        {
            if (world_data.battle != null && world_data.battle.IsActive())
            {
                int hp_cost = 5;
                EventBattle battle = EventBattle.Get(world_data.battle.battle_id);
                BattleCharacter pcharacter = world_data.battle.GetChampion(player_id);
                if (battle != null && pcharacter != null && pcharacter.GetHP() > hp_cost)
                {
                    foreach (BattleCharacter character in world_data.battle.characters)
                    {
                        if(!character.IsEnemy())
                            character.damage += hp_cost;
                    }

                    battle_logic.EndBattle(0); //Flee
                }
            }

            RefreshWorld();
        }

        public virtual void EndBattle(int result) //-1 lose, 1 win, 0 retreat
        {
            if (world_data.AreAllChampionsDead())
            {
                EndGame(false);
                return;
            }

            foreach (BattleCharacter character in world_data.battle.characters)
            {
                //Set HP, items and new position
                Champion champion = world_data.GetChampion(character.uid);
                if (champion != null)
                {
                    champion.damage = character.GetDamage();
                    champion.position = world_data.battle.GetOrderIndex(character);

                    if (champion.GetHP() <= 0)
                        champion.damage = champion.GetHPMax() - 1; //Revive with 1 HP
                }


                //Set ally position
                CharacterAlly ally = world_data.GetAlly(character.uid);
                if (ally != null)
                {
                    ally.position = world_data.battle.GetOrderIndex(character);
                }
            }

            TriggerAbilityType(AbilityTrigger.BattleEnd);

            EventBattle battle = EventBattle.Get(world_data.event_id);
            Champion bchampion = world_data.GetChampion(world_data.event_champion);
            TriggerEventNext(bchampion, battle?.win_event);
            StopMapEvent();

            if (result > 0)
            {
                GainBattleRewards(battle); //WIN
            }
            if (result < 0)
            {
                EndGame(false);
            }
        }

        //--------------------

        public virtual void GainLevelUpRewards(Champion champion)
        {
            world_data.state = WorldState.LevelUp;
            world_data.event_champion = champion.uid;
            world_data.ResetActionCompleted(true);
            champion.action_completed = false;

            //Cards rewards
            Player player = world_data.GetPlayer(champion.player_id);
            List<CardData> unlockable_cards = champion.ChampionData.GetRewardCards(player, null);
            List<CardData> reward_cards = GetRandomCardsByProbability(unlockable_cards, 3, champion.Hash + 3);

            champion.reward = new Reward();
            foreach (CardData card in reward_cards)
                champion.reward.cards.Add(card.id);

            RefreshWorld();
        }


        public virtual void GainCardRewards(RarityData rarity)
        {
            world_data.state = WorldState.Reward;
            world_data.event_champion = "";
            world_data.ResetActionCompleted();

            foreach (Champion champion in world_data.champions)
            {
                //Cards rewards
                Player player = world_data.GetPlayer(champion.player_id);
                List<CardData> unlockable_cards = champion.ChampionData.GetRewardCards(player, rarity);
                List<CardData> reward_cards = GetRandomCardsByProbability(unlockable_cards, 3, champion.Hash + 1);

                champion.reward = new Reward();
                foreach (CardData card in reward_cards)
                    champion.reward.cards.Add(card.id);
            }

            RefreshWorld();
        }

        public virtual void GainBattleRewards(EventBattle battle)
        {
            world_data.state = WorldState.Reward;
            world_data.event_champion = "";
            world_data.ResetActionCompleted();

            foreach (Champion champion in world_data.champions)
            {
                champion.reward = new Reward();
                champion.reward.gold += Mathf.RoundToInt(battle.reward_gold * champion.GetGoldFactor());
                champion.reward.xp += Mathf.RoundToInt(battle.reward_xp * champion.GetXPFactor());

                foreach (BattleCharacter character in world_data.battle.characters)
                {
                    if (character.IsEnemy())
                    {
                        champion.reward.gold += Mathf.RoundToInt(character.CharacterData.reward_gold * champion.GetGoldFactor());
                        champion.reward.xp += Mathf.RoundToInt(character.CharacterData.reward_xp * champion.GetXPFactor());
                    }
                }

                Player player = world_data.GetPlayer(champion.player_id);
                champion.xp += champion.reward.xp;
                player.gold += champion.reward.gold;

                //Cards rewards
                if (battle.reward_cards)
                {
                    List<CardData> unlockable_cards = champion.ChampionData.GetRewardCards(player, battle.card_rarity);
                    List<CardData> reward_cards = GetRandomCardsByProbability(unlockable_cards, 3, champion.Hash);

                    foreach (CardData card in reward_cards)
                        champion.reward.cards.Add(card.id);
                }

                //Items rewards
                if (battle.reward_items)
                {
                    List<CardData> unlockable_items = CardData.GetRewardItems(player.udata, battle.item_team, battle.card_rarity);
                    List<CardData> reward_items = GetRandomCardsByProbability(unlockable_items, 3, champion.Hash);

                    foreach (CardData card in reward_items)
                        champion.reward.items.Add(card.id);
                }
            }

            RefreshWorld();
        }

        public virtual List<CardData> GetRandomCardsByProbability(List<CardData> cards, int nb, int seed_offset = 0)
        {
            System.Random rand = new System.Random(world_data.GetLocationSeed(13851 + seed_offset));
            List <CardData> ocards = new List<CardData>();
            int tries = 0;
            int tries_total = cards.Count;
            while(ocards.Count < nb && cards.Count > 0 && tries < tries_total)
            {
                float total_probability = 0f;
                foreach (CardData card in cards)
                {
                    total_probability += card.rarity.probability;
                }

                CardData to_add = null;
                float val = (float)rand.NextDouble() * total_probability;
                foreach (CardData card in cards)
                {
                    if (val >= 0f && val < card.rarity.probability)
                        to_add = card;
                    val -= card.rarity.probability;
                }

                if (to_add != null)
                {
                    ocards.Add(to_add);
                    cards.Remove(to_add);
                }
                tries++;
            }
            return ocards;
        }

        //--------------------

        public virtual void RefreshWorld()
        {
            onRefreshWorld?.Invoke();
        }

        public bool IsResolving()
        {
            return world_data.state == WorldState.Battle && battle_logic.IsResolving();
        }

        public World WorldData { get { return world_data; } }
    }
}
