using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using RogueEngine.Gameplay;
using RogueEngine.AI;
using Unity.Netcode;

namespace RogueEngine.Server
{
    /// <summary>
    /// Represent one game on the server, when playing solo this will be created locally, 
    /// or if online multiple GameServer, one for each match, will be created by the dedicated server
    /// Manage receiving actions, sending refresh, and running AI
    /// </summary>
    
    public class GameServer
    {
        public static float game_expire_time = 30f;      //How long for the game to be deleted when no one is connected
        public static int players_max = 4;

        private World world_data;
        private WorldLogic world_logic;
        private BattleLogic battle_logic;
        private float expiration = 0f;

        private List<ClientData> players = new List<ClientData>();            //Exclude observers, stays in array when disconnected, only players can send commands
        private List<ClientData> connected_clients = new List<ClientData>();  //Include obervers, removed from array when disconnected, all clients receive refreshes
        private List<AIPlayer> ai_list = new List<AIPlayer>();
        
        private Dictionary<ushort, CommandEvent> registered_commands = new Dictionary<ushort, CommandEvent>();
        private Queue<QueuedGameAction> queued_actions = new Queue<QueuedGameAction>(); //List of action waiting to be processed

        public GameServer(World world, bool online)
        {
            Init(world, online);
        }

        ~GameServer()
        {
            Clear();
        }

        protected virtual void Init(World world, bool online)
        {
            world_data = world;
            battle_logic = new BattleLogic(world_data);
            world_logic = new WorldLogic(battle_logic, world);

            //Actions
            RegisterAction(GameAction.NewScenario, ReceiveCreateScenario);
            RegisterAction(GameAction.LoadScenario, ReceiveLoadScenario);
            RegisterAction(GameAction.SendUserData, ReceiveUserData);
            RegisterAction(GameAction.CreateChampion, ReceiveCreateChampion);
            RegisterAction(GameAction.DeleteChampion, ReceiveDeleteChampion);
            RegisterAction(GameAction.StartGame, ReceiveStartGame);
            RegisterAction(GameAction.StartTest, ReceiveStartTest);

            RegisterAction(GameAction.MapMove, ReceiveMapMove);
            RegisterAction(GameAction.MapEventDone, ReceiveCompleteAction);
            RegisterAction(GameAction.MapEventDoneChampion, ReceiveCompleteActionChampion);
            RegisterAction(GameAction.MapEventChoice, ReceiveMapChoice);
            RegisterAction(GameAction.MapRewardCardChoice, ReceiveMapRewardCard);
            RegisterAction(GameAction.MapRewardItemChoice, ReceiveMapRewardItem);
            RegisterAction(GameAction.MapUpgradeCard, ReceiveMapUpgrade);
            RegisterAction(GameAction.MapLevelUp, ReceiveLevelUp);
            RegisterAction(GameAction.MapTrashCard, ReceiveTrashCard);

            RegisterAction(GameAction.ShopBuyItem, ReceiveBuyItem);
            RegisterAction(GameAction.ShopBuyCard, ReceiveBuyCard);
            RegisterAction(GameAction.ShopSellItem, ReceiveSellItem);
            RegisterAction(GameAction.UseItem, ReceiveUseItem);

            //Battle Actions
            RegisterAction(GameAction.PlayCard, ReceivePlayCard);
            RegisterAction(GameAction.Move, ReceiveMoveCharacter);
            RegisterAction(GameAction.CastAbility, ReceiveCastCardAbility);
            RegisterAction(GameAction.SelectCard, ReceiveSelectCard);
            RegisterAction(GameAction.SelectCharacter, ReceiveSelectCharacter);
            RegisterAction(GameAction.SelectSlot, ReceiveSelectSlot);
            RegisterAction(GameAction.SelectChoice, ReceiveSelectChoice);
            RegisterAction(GameAction.CancelSelect, ReceiveCancelSelection);
            RegisterAction(GameAction.EndTurn, ReceiveEndTurn);
            RegisterAction(GameAction.Resign, ReceiveResign);
            RegisterAction(GameAction.ChatMessage, ReceiveChat);

            //Map events
            world_logic.onGameStart += OnGameStart;
            world_logic.onGameEnd += OnGameEnd;
            world_logic.onMove += OnMove;
            world_logic.onEventChoice += OnEventChoice;
            world_logic.onRewardChoice += OnRewardCardChoice;
            world_logic.onRefreshWorld += RefreshWorld;

            //Events
            battle_logic.onBattleStart += OnBattleStart;
            battle_logic.onBattleEnd += OnBattleEnd;
            battle_logic.onTurnStart += OnTurnStart;

            battle_logic.onCharacterMoved += OnCharacterMoved;
            battle_logic.onCharacterDamaged += OnCharacterDamaged;

            battle_logic.onCardPlayed += OnCardPlayed;
            battle_logic.onCardSummoned += OnCardSummoned;
            battle_logic.onCardTransformed += OnCardTransformed;
            battle_logic.onCardDiscarded += OnCardDiscarded;
            battle_logic.onCardDrawn += OnCardDraw;

            battle_logic.onItemUsed += OnItemUsed;
            battle_logic.onRollValue += OnValueRolled;

            battle_logic.onAbilityStart += OnAbilityStart;
            battle_logic.onAbilityTargetCard += OnAbilityTargetCard;
            battle_logic.onAbilityTargetCharacter += OnAbilityTargetCharacter;
            battle_logic.onAbilityTargetSlot += OnAbilityTargetSlot;
            battle_logic.onAbilityEnd += OnAbilityEnd;

            battle_logic.onSecretTrigger += OnSecretTriggered;
            battle_logic.onSecretResolve += OnSecretResolved;

            battle_logic.refreshWorld += RefreshWorld;
            battle_logic.refreshBattle += RefreshBattle;

            //Create ai
            AIPlayer ai = AIPlayer.Create(AIType.Behavior, battle_logic, -1);
            ai_list.Add(ai);
        }

        protected virtual void Clear()
        {
            //Map events
            world_logic.onGameStart -= OnGameStart;
            world_logic.onGameEnd -= OnGameEnd;
            world_logic.onMove -= OnMove;
            world_logic.onEventChoice -= OnEventChoice;
            world_logic.onRewardChoice -= OnRewardCardChoice;
            world_logic.onRefreshWorld -= RefreshWorld;

            //Battle event
            battle_logic.onBattleStart -= OnBattleStart;
            battle_logic.onBattleEnd -= OnBattleEnd;
            battle_logic.onTurnStart -= OnTurnStart;

            battle_logic.onCharacterMoved -= OnCharacterMoved;
            battle_logic.onCharacterDamaged -= OnCharacterDamaged;

            battle_logic.onCardPlayed -= OnCardPlayed;
            battle_logic.onCardSummoned -= OnCardSummoned;
            battle_logic.onCardTransformed -= OnCardTransformed;
            battle_logic.onCardDiscarded -= OnCardDiscarded;
            battle_logic.onCardDrawn -= OnCardDraw;

            battle_logic.onItemUsed -= OnItemUsed;
            battle_logic.onRollValue -= OnValueRolled;

            battle_logic.onAbilityStart -= OnAbilityStart;
            battle_logic.onAbilityTargetCard -= OnAbilityTargetCard;
            battle_logic.onAbilityTargetCharacter -= OnAbilityTargetCharacter;
            battle_logic.onAbilityTargetSlot -= OnAbilityTargetSlot;
            battle_logic.onAbilityEnd -= OnAbilityEnd;

            battle_logic.onSecretTrigger -= OnSecretTriggered;
            battle_logic.onSecretResolve -= OnSecretResolved;

            battle_logic.refreshWorld -= RefreshWorld;
            battle_logic.refreshBattle -= RefreshBattle;
        }

        public virtual void Update()
        {
            //Game Expiration if no one is connected or game ended
            int connected_players = CountConnectedPlayers();
            if (HasEnded() || connected_players == 0)
                expiration += Time.deltaTime;

            world_logic.Update(Time.deltaTime);

            if (world_data.state == WorldState.Battle)
            {
                battle_logic.Update(Time.deltaTime);

                //Update AI
                foreach (AIPlayer ai in ai_list)
                {
                    ai.Update();
                }
            }

            //Process queued actions
            if (queued_actions.Count > 0 && !world_logic.IsResolving())
            {
                QueuedGameAction action = queued_actions.Dequeue();
                ExecuteAction(action.type, action.client, action.sdata);
            }
        }

        //------ Receive Actions -------

        private void RegisterAction(ushort tag, UnityAction<ClientData, SerializedData> callback)
        {
            CommandEvent cmdevt = new CommandEvent();
            cmdevt.tag = tag;
            cmdevt.callback = callback;
            registered_commands.Add(tag, cmdevt);
        }

        public void ReceiveAction(ulong client_id, FastBufferReader reader)
        {
            ClientData client = GetClient(client_id);
            if (client != null)
            {
                reader.ReadValueSafe(out ushort type);
                SerializedData sdata = new SerializedData(reader);
                if (!battle_logic.IsResolving())
                {
                    //Not resolving, execute now
                    ExecuteAction(type, client, sdata);
                }
                else
                {
                    //Resolving, wait before executing
                    QueuedGameAction action = new QueuedGameAction();
                    action.type = type;
                    action.client = client;
                    action.sdata = sdata;
                    sdata.PreRead();
                    queued_actions.Enqueue(action);
                }
            }
        }

        public void ExecuteAction(ushort type, ClientData client, SerializedData sdata)
        {
            bool found = registered_commands.TryGetValue(type, out CommandEvent command);
            if (found)
                command.callback.Invoke(client, sdata);
        }

        //----------------------

        public void ReceiveCreateScenario(ClientData iclient, SerializedData sdata)
        {
            MsgNewScenario msg = sdata.Get<MsgNewScenario>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                ScenarioData scenario = ScenarioData.Get(msg.scenario);
                if (scenario != null)
                {
                    world_logic.CreateGame(scenario, msg.online, msg.filename, msg.title);
                }
            }
        }

        public void ReceiveLoadScenario(ClientData iclient, SerializedData sdata)
        {
            MsgLoadScenario msg = sdata.Get<MsgLoadScenario>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                Player hplayer = msg.world.GetPlayer(iclient.user_id);
                if (hplayer != null)
                {
                    world_data = msg.world;
                    world_logic.SetData(world_data);
                    world_data.DisconnectAll(); //Set players disconnected
                    hplayer.connected = true;

                    RefreshWorld();
                }
            }
        }

        public void ReceiveUserData(ClientData iclient, SerializedData sdata)
        {
            MsgUserData msg = sdata.Get<MsgUserData>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && msg.user_data != null && player.user_id == msg.user_data.id)
            {
                player.udata = msg.user_data;
                player.udata.FixData();
                RefreshWorld();
            }
        }

        public void ReceiveCreateChampion(ClientData iclient, SerializedData sdata)
        {
            MsgAddChampion msg = sdata.Get<MsgAddChampion>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
                ChampionData champion = ChampionData.Get(msg.champion_id);
                if (scenario != null && champion != null)
                {
                    world_logic.CreateChampion(player.player_id, champion, msg.slot_x);
                }
            }
        }

        public void ReceiveDeleteChampion(ClientData iclient, SerializedData sdata)
        {
            MsgAddChampion msg = sdata.Get<MsgAddChampion>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                Champion champ = world_data.GetSlotChampion(msg.slot_x);
                if (champ != null)
                {
                    bool valid_player = champ.player_id == player.player_id || player.player_id == 1; //Only first player or self can remove champion
                    ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
                    if (scenario != null && valid_player)
                    {
                        world_logic.WorldData.RemoveChampion(msg.slot_x);
                        RefreshWorld();
                    }
                }
            }
        }

        public void ReceiveStartGame(ClientData iclient, SerializedData sdata)
        {
            MsgStartGame msg = sdata.Get<MsgStartGame>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                ScenarioData scenario = ScenarioData.Get(world_data.scenario_id);
                if (scenario != null)
                {
                    world_logic.StartGame(msg.seed);
                }
            }
        }

        public void ReceiveStartTest(ClientData iclient, SerializedData sdata)
        {
            MsgStartTest msg = sdata.Get<MsgStartTest>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                world_logic.StartGame(msg.seed);
                world_logic.StartTest(msg.test_state);
            }
        }

        //------------------------

        public void ReceiveMapMove(ClientData iclient, SerializedData sdata)
        {
            MsgMapMove msg = sdata.Get<MsgMapMove>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                Champion champion = world_data.GetChampion(player.player_id);
                MapLocation loc = world_data.GetLocation(msg.map_id, msg.location_id);
                if (champion != null && loc != null)
                    world_logic.Move(champion, loc);
            }
        }

        public void ReceiveCompleteAction(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                world_logic.CompleteAction(player.player_id);
            }
        }

        public void ReceiveCompleteActionChampion(ClientData iclient, SerializedData sdata)
        {
            MsgChampion msg = sdata.Get<MsgChampion>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                world_logic.CompleteAction(champion);
            }
        }

        public void ReceiveMapRewardCard(ClientData iclient, SerializedData sdata)
        {
            MsgRewardChoice msg = sdata.Get<MsgRewardChoice>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                if (champion != null && champion.player_id == player.player_id)
                    world_logic.SelectCardReward(champion, msg.target_id);
            }
        }

        public void ReceiveMapRewardItem(ClientData iclient, SerializedData sdata)
        {
            MsgRewardChoice msg = sdata.Get<MsgRewardChoice>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                if (champion != null && champion.player_id == player.player_id)
                    world_logic.SelectItemReward(champion, msg.target_id);
            }
        }

        public void ReceiveMapChoice(ClientData iclient, SerializedData sdata)
        {
            MsgEventChoice msg = sdata.Get<MsgEventChoice>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                if (champion != null && champion.player_id == player.player_id)
                    world_logic.SelectEventChoice(champion, msg.choice);
            }
        }

        public void ReceiveMapUpgrade(ClientData iclient, SerializedData sdata)
        {
            MsgChampionCard msg = sdata.Get<MsgChampionCard>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                ChampionCard card = champion.GetCard(msg.card_uid);
                if (champion != null && card != null && champion.player_id == player.player_id)
                    world_logic.UpgradeCard(champion, card);
            }
        }

        public void ReceiveLevelUp(ClientData iclient, SerializedData sdata)
        {
            MsgChampion msg = sdata.Get<MsgChampion>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                if (champion != null && champion.player_id == player.player_id)
                    world_logic.LevelUp(champion);
            }
        }

        public void ReceiveTrashCard(ClientData iclient, SerializedData sdata)
        {
            MsgChampionCard msg = sdata.Get<MsgChampionCard>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                ChampionCard card = champion.GetCard(msg.card_uid);
                if (champion != null && card != null && champion.player_id == player.player_id)
                    world_logic.TrashCard(champion, card);
            }
        }

        public void ReceiveBuyItem(ClientData iclient, SerializedData sdata)
        {
            MsgShop msg = sdata.Get<MsgShop>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                CardData item = CardData.Get(msg.item_id);
                if (champion != null && item != null && champion.player_id == player.player_id)
                    world_logic.BuyItem(champion, item);
            }
        }

        public void ReceiveBuyCard(ClientData iclient, SerializedData sdata)
        {
            MsgShop msg = sdata.Get<MsgShop>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                CardData card = CardData.Get(msg.item_id);
                if (champion != null && card != null && champion.player_id == player.player_id)
                    world_logic.BuyCard(champion, card);
            }
        }

        public void ReceiveSellItem(ClientData iclient, SerializedData sdata)
        {
            MsgShop msg = sdata.Get<MsgShop>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                Champion champion = world_data.GetChampion(msg.champion_uid);
                CardData item = CardData.Get(msg.item_id);
                if (champion != null && item != null && champion.player_id == player.player_id)
                    world_logic.SellItem(champion, item);
            }
        }

        public void ReceiveUseItem(ClientData iclient, SerializedData sdata)
        {
            MsgUse msg = sdata.Get<MsgUse>();
            Player player = GetPlayer(iclient);
            if (player != null)
            {
                if (world_data.state == WorldState.Battle)
                {
                    if (world_data.battle != null && world_data.battle.IsPlayerActionTurn(player.player_id) && !battle_logic.IsResolving())
                    {
                        BattleCharacter character = world_data.battle.GetCharacter(msg.character_uid);
                        Card item = character?.GetItem(msg.uid);
                        if (character != null && item != null && character.player_id == player.player_id)
                            battle_logic.UseItem(character, item);
                    }
                }
                else
                {
                    Champion champion = world_data.GetChampion(msg.character_uid);
                    ChampionItem item = champion?.GetItem(msg.uid);
                    if (champion != null && item != null && champion.player_id == player.player_id)
                        world_logic.UseItem(champion, item);
                }
            }
        }

        //----------------------

        public void ReceivePlayCard(ClientData iclient, SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && world_data.battle != null && world_data.battle.IsPlayerActionTurn(player.player_id) && !battle_logic.IsResolving())
            {
                Card card = world_data.battle.GetCard(msg.card_uid);
                if (card != null && card.player_id == player.player_id)
                    battle_logic.PlayCard(card, msg.slot);
            }
        }

        public void ReceiveMoveCharacter(ClientData iclient, SerializedData sdata)
        {
            MsgMove msg = sdata.Get<MsgMove>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && world_data.battle != null && world_data.battle.IsPlayerActionTurn(player.player_id) && !battle_logic.IsResolving())
            {
                BattleCharacter character = world_data.battle.GetCharacter(msg.character_uid);
                if (character != null && character.player_id == player.player_id)
                    battle_logic.MoveCharacter(character, msg.slot);
            }
        }

        public void ReceiveCastCardAbility(ClientData iclient, SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && world_data.battle != null && world_data.battle.IsPlayerActionTurn(player.player_id) && !battle_logic.IsResolving())
            {
                //Card card = player.GetCard(card_uid);
                //AbilityData iability = AbilityData.Get(ability_id);
                //if (card != null && card.player_id == player.player_id)
                //    gameplay.CastAbility(card, iability);
            }
        }

        public void ReceiveSelectCard(ClientData iclient, SerializedData sdata)
        {
            MsgUID msg = sdata.Get<MsgUID>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && world_data.battle != null && world_data.battle.IsPlayerSelectorTurn(player.player_id) && !battle_logic.IsResolving())
            {
                Card target = world_data.battle.GetCard(msg.uid);
                battle_logic.SelectCard(target);
            }
        }

        public void ReceiveSelectCharacter(ClientData iclient, SerializedData sdata)
        {
            MsgUID msg = sdata.Get<MsgUID>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && world_data.battle != null && world_data.battle.IsPlayerSelectorTurn(player.player_id) && !battle_logic.IsResolving())
            {
                BattleCharacter target = world_data.battle.GetCharacter(msg.uid);
                battle_logic.SelectCharacter(target);
            }
        }

        public void ReceiveSelectSlot(ClientData iclient, SerializedData sdata)
        {
            Slot slot = sdata.Get<Slot>();
            Player player = GetPlayer(iclient);
            if (player != null && world_data.battle != null && world_data.battle.IsPlayerSelectorTurn(player.player_id) && !battle_logic.IsResolving())
            {
                if (slot != null && slot.IsValid())
                {
                    battle_logic.SelectSlot(slot);
                }
            }
        }

        public void ReceiveSelectChoice(ClientData iclient, SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            Player player = GetPlayer(iclient);
            if (player != null && msg != null && world_data.battle != null && world_data.battle.IsPlayerSelectorTurn(player.player_id) && !battle_logic.IsResolving())
            {
                battle_logic.SelectChoice(msg.value);
            }
        }

        public void ReceiveCancelSelection(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null && world_data.battle != null && world_data.battle.IsPlayerSelectorTurn(player.player_id) && !battle_logic.IsResolving())
            {
                battle_logic.CancelSelection();
            }
        }

        public void ReceiveEndTurn(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null && world_data.battle != null && world_data.battle.IsPlayerTurn(player.player_id) && !battle_logic.IsResolving())
            {
                battle_logic.NextStep();
            }
        }

        public void ReceiveResign(ClientData iclient, SerializedData sdata)
        {
            Player player = GetPlayer(iclient);
            if (player != null && world_data.battle != null)
            {
                world_logic.FleeBattle(player.player_id);
            }
        }

        public void ReceiveChat(ClientData iclient, SerializedData sdata)
        {
            MsgChat msg = sdata.Get<MsgChat>();
            if (msg != null)
            {
                Player player = world_data.GetPlayer(iclient.user_id);
                if(player != null)
                    msg.username = player.username; //Override username
                SendToAll(GameAction.ChatMessage, msg, NetworkDelivery.Reliable);
            }
        }

        //--- Setup Commands ------

        public virtual void SetWorld(World world)
        {
            if (world_data.state == WorldState.None)
            {
                world_data = world;
                RefreshWorld();
            }
        }

        public virtual void StartScenario(World world)
        {
            //MapEventBattle evt = GameplayData.Get().test_battle;
            //world_logic.StartBattle(evt);
        }

        //-------------

        public void AddClient(ClientData client)
        {
            if (client != null && !connected_clients.Contains(client))
                connected_clients.Add(client);
        }

        public void RemoveClient(ClientData client)
        {
            if (client == null)
                return;

            connected_clients.Remove(client);

            Player player = world_data.GetPlayer(client.user_id);
            if (player != null && player.connected)
            {
                player.connected = false;
                RefreshWorld();
            }
        }

        public ClientData GetClient(ulong client_id)
        {
            foreach (ClientData client in connected_clients)
            {
                if (client.client_id == client_id)
                    return client;
            }
            return null;
        }

        public int AddPlayer(ClientData client)
        {
            if (!players.Contains(client))
                players.Add(client);

            Player player = world_data.AddPlayer(client.user_id, client.username);
            if (player != null)
            {
                player.connected = true;
                return player.player_id;
            }

            return -1;
        }

        public Player GetPlayer(ClientData client)
        {
            return GetPlayer(client.user_id);
        }

        public Player GetPlayer(string user_id)
        {
            return world_data?.GetPlayer(user_id);
        }

        public bool IsPlayer(string user_id)
        {
            Player player = GetPlayer(user_id);
            return player != null;
        }

        public bool IsConnectedPlayer(string user_id)
        {
            Player player = GetPlayer(user_id);
            return player != null && player.connected;
        }

        public int CountPlayers()
        {
            return players.Count;
        }

        public virtual int GetMaxPlayers()
        {
            return players_max;
        }
        
        public int CountConnectedPlayers()
        {
            int nb = 0;
            World game = GetWorldData();
            foreach (Player player in game.players)
            {
                if (player.IsConnected())
                {
                    nb++;
                }
            }
            return nb;
        }

        public World GetWorldData()
        {
            return world_data;
        }

        public Battle GetBattleData()
        {
            return world_data.battle;
        }

        public virtual bool HasStarted()
        {
            return battle_logic.IsGameStarted();
        }

        public virtual bool HasEnded()
        {
            return world_data.HasEnded();
        }

        public virtual bool IsExpired()
        {
            return expiration > game_expire_time; //Means that the game expired (everyone left or game ended)
        }

        //---------------------------

        protected virtual void OnGameStart()
        {
            SendToAll(GameAction.GameStarted);
        }

        protected virtual void OnGameEnd()
        {
            SendToAll(GameAction.GameEnded);
        }

        protected virtual void OnMove(Champion champ, MapLocation loc)
        {
            MsgMapMove msg = new MsgMapMove();
            msg.map_id = loc.map_id;
            msg.location_id = loc.ID;
            SendToAll(GameAction.MapMoved, msg, NetworkDelivery.Reliable);
            RefreshWorld();
        }

        protected virtual void OnEventChoice(Champion champ, EventData choice)
        {
            MsgEventChoice msg = new MsgEventChoice();
            msg.champion_uid = champ.uid;
            msg.choice = choice != null ? choice.id : "";
            SendToAll(GameAction.MapChoiceSelected, msg, NetworkDelivery.Reliable);
            RefreshWorld();
        }

        protected virtual void OnRewardCardChoice(Champion champ, CardData card)
        {
            MsgRewardChoice msg = new MsgRewardChoice();
            msg.champion_uid = champ.uid;
            msg.target_id = card != null ? card.id : "";
            SendToAll(GameAction.MapRewardSelected, msg, NetworkDelivery.Reliable);
            RefreshWorld();
        }

        //---------------------------

        protected virtual void OnBattleStart()
        {
            SendToAll(GameAction.BattleStarted);
        }

        protected virtual void OnBattleEnd(int result)
        {
            world_logic.EndBattle(result);
            SendToAll(GameAction.BattleEnded);
        }

        protected virtual void OnTurnStart()
        {
            SendToAll(GameAction.NewTurn);
        }

        protected virtual void OnCharacterMoved(BattleCharacter character, Slot target)
        {
            MsgMove mdata = new MsgMove();
            mdata.character_uid = character.uid;
            mdata.slot = target;
            SendToAll(GameAction.CharacterMoved, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCharacterDamaged(BattleCharacter character, int damage)
        {
            MsgDamaged mdata = new MsgDamaged();
            mdata.character_uid = character.uid;
            mdata.damage = damage;
            SendToAll(GameAction.CharacterDamaged, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardPlayed(Card card, Slot target)
        {
            MsgPlayCard mdata = new MsgPlayCard();
            mdata.card_uid = card.uid;
            mdata.slot = target;
            SendToAll(GameAction.CardPlayed, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardSummoned(Card card)
        {
            MsgUID mdata = new MsgUID();
            mdata.uid = card.uid;
            SendToAll(GameAction.CardSummoned, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardTransformed(Card card)
        {
            MsgUID mdata = new MsgUID();
            mdata.uid = card.uid;
            SendToAll(GameAction.CardTransformed, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardDiscarded(Card card)
        {
            MsgUID mdata = new MsgUID();
            mdata.uid = card.uid;
            SendToAll(GameAction.CardDiscarded, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnCardDraw(int nb)
        {
            MsgInt mdata = new MsgInt();
            mdata.value = nb;
            SendToAll(GameAction.CardDrawn, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnItemUsed(Card item)
        {
            MsgUse mdata = new MsgUse();
            mdata.character_uid = item.owner_uid;
            mdata.uid = item.uid;
            SendToAll(GameAction.ItemUsed, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnValueRolled(int nb)
        {
            MsgInt mdata = new MsgInt();
            mdata.value = nb;
            SendToAll(GameAction.ValueRolled, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityStart(AbilityData ability, Card card)
        {
            MsgCastAbility mdata = new MsgCastAbility();
            mdata.ability_id = ability.id;
            mdata.card_uid = card != null ? card.uid : "";
            mdata.target_uid = "";
            SendToAll(GameAction.AbilityTrigger, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityTargetCard(AbilityData ability, Card card, Card target)
        {
            MsgCastAbility mdata = new MsgCastAbility();
            mdata.ability_id = ability.id;
            mdata.card_uid = card != null ? card.uid : "";
            mdata.target_uid = target != null ? target.uid : "";
            SendToAll(GameAction.AbilityTargetCard, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityTargetCharacter(AbilityData ability, Card card, BattleCharacter target)
        {
            MsgCastAbility mdata = new MsgCastAbility();
            mdata.ability_id = ability.id;
            mdata.card_uid = card != null ? card.uid : "";
            mdata.target_uid = target != null ? target.uid : "";
            SendToAll(GameAction.AbilityTargetCharacter, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityTargetSlot(AbilityData ability, Card card, Slot target)
        {
            MsgCastAbilitySlot mdata = new MsgCastAbilitySlot();
            mdata.ability_id = ability.id;
            mdata.card_uid = card != null ? card.uid : "";
            mdata.slot = target;
            SendToAll(GameAction.AbilityTargetSlot, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnAbilityEnd(AbilityData ability, Card card)
        {
            MsgCastAbility mdata = new MsgCastAbility();
            mdata.ability_id = ability.id;
            mdata.card_uid = card != null ? card.uid : "";
            mdata.target_uid = "";
            SendToAll(GameAction.AbilityEnd, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnSecretTriggered(Card secret, Card trigger)
        {
            MsgSecret mdata = new MsgSecret();
            mdata.secret_uid = secret.uid;
            mdata.triggerer_uid = trigger != null ? trigger.uid : "";
            SendToAll(GameAction.SecretTriggered, mdata, NetworkDelivery.Reliable);
        }

        protected virtual void OnSecretResolved(Card secret, Card trigger)
        {
            MsgSecret mdata = new MsgSecret();
            mdata.secret_uid = secret.uid;
            mdata.triggerer_uid = trigger != null ? trigger.uid : "";
            SendToAll(GameAction.SecretResolved, mdata, NetworkDelivery.Reliable);
        }

        public virtual void RefreshWorld()
        {
            if (world_data != null)
            {
                MsgRefreshWorld mdata = new MsgRefreshWorld();
                mdata.world_data = world_data;
                SendToAll(GameAction.RefreshWorld, mdata, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public virtual void RefreshBattle()
        {
            if (world_data != null && world_data.battle != null)
            {
                MsgRefreshBattle mdata = new MsgRefreshBattle();
                mdata.battle_data = world_data.battle;
                SendToAll(GameAction.RefreshBattle, mdata, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public void SendToAll(ushort tag)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            foreach (ClientData iclient in connected_clients)
            {
                if (iclient != null)
                {
                    Messaging.Send("refresh", iclient.client_id, writer, NetworkDelivery.Reliable);
                }
            }
            writer.Dispose();
        }

        public void SendToAll(ushort tag, INetworkSerializable data, NetworkDelivery delivery)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(tag);
            writer.WriteNetworkSerializable(data);
            foreach (ClientData iclient in connected_clients)
            {
                if (iclient != null)
                {
                    Messaging.Send("refresh", iclient.client_id, writer, delivery);
                }
            }
            writer.Dispose();
        }

        public ulong ServerID { get { return TcgNetwork.Get().ServerID; } }
        public NetworkMessaging Messaging { get { return TcgNetwork.Get().Messaging; } }
    }

    public struct QueuedGameAction
    {
        public ushort type;
        public ClientData client;
        public SerializedData sdata;
    }
}
