using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace RogueEngine.Client
{
    /// <summary>
    /// Main script for the client-side of the game, should be in game scene only
    /// Will connect to server, then connect to the game on that server (with uid) and then will send game settings
    /// During the game, will send all actions performed by the player and receive game refreshes
    /// </summary>

    public class GameClient : MonoBehaviour
    {
        public static ConnectSettings connect_settings = ConnectSettings.Default;

        //-----

        public UnityAction onConnectedServer;
        public UnityAction onConnectedGame;
        public UnityAction onDisconnect;

        public UnityAction onGameStart;
        public UnityAction onGameEnd;

        public UnityAction<string, string> onChatMsg;  //player_id, msg
        public UnityAction<string> onServerMsg;  //msg
        public UnityAction onRefreshWorld;
        public UnityAction onRefreshBattle;

        //Map events -----

        public UnityAction<MapLocation> onMapMove;
        public UnityAction<Champion, EventData> onEventChoice;
        public UnityAction<Champion, CardData> onRewardChoice;

        //Battle events --------

        public UnityAction onBattleStart;
        public UnityAction onBattleEnd;
        public UnityAction onNewTurn;
        public UnityAction<BattleCharacter, Slot> onCharacterMoved;
        public UnityAction<BattleCharacter, int> onCharacterDamaged;
        public UnityAction<Card> onItemUsed;

        public UnityAction<Card, Slot> onCardPlayed;
        public UnityAction<Card> onCardTransformed;
        public UnityAction<Card> onCardDiscarded;
        public UnityAction<int> onCardDraw;
        public UnityAction<int> onValueRolled;

        public UnityAction<AbilityData, Card> onAbilityStart;
        public UnityAction<AbilityData, Card, Card> onAbilityTargetCard;      //Ability, Caster, Target
        public UnityAction<AbilityData, Card, BattleCharacter> onAbilityTargetCharacter;
        public UnityAction<AbilityData, Card, Slot> onAbilityTargetSlot;
        public UnityAction<AbilityData, Card> onAbilityEnd;
        public UnityAction<Card, Card> onSecretTrigger;    //Secret, Triggerer
        public UnityAction<Card, Card> onSecretResolve;    //Secret, Triggerer

        public UnityAction<Card, Card> onAttackStart;   //Attacker, Defender
        public UnityAction<Card, Card> onAttackEnd;     //Attacker, Defender
        public UnityAction<Card, Player> onAttackPlayerStart;
        public UnityAction<Card, Player> onAttackPlayerEnd;


        private int player_id = 0; //Player playing on this device;
        private World world_data;
        private float timer = 0f;

        private Dictionary<ushort, RefreshEvent> registered_commands = new Dictionary<ushort, RefreshEvent>();

        private static GameClient instance;

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return; //Manager already exists, destroy this one
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 120;
        }

        protected virtual void Start()
        {
            RegisterRefresh(GameAction.Connected, OnConnectedToGame);
            RegisterRefresh(GameAction.GameStarted, OnGameStart);
            RegisterRefresh(GameAction.GameEnded, OnGameEnd);

            RegisterRefresh(GameAction.MapMoved, OnMapMoved);
            RegisterRefresh(GameAction.MapChoiceSelected, OnMapChoiceSelected);
            RegisterRefresh(GameAction.MapRewardSelected, OnMapRewardSelected);

            RegisterRefresh(GameAction.ChatMessage, OnChat);
            RegisterRefresh(GameAction.ServerMessage, OnServerMsg);
            RegisterRefresh(GameAction.RefreshWorld, OnRefreshWorld);

            //----

            RegisterRefresh(GameAction.BattleStarted, OnBattleStart);
            RegisterRefresh(GameAction.BattleEnded, OnBattleEnd);
            RegisterRefresh(GameAction.NewTurn, OnNewTurn);
            RegisterRefresh(GameAction.CharacterMoved, OnCharacterMoved);
            RegisterRefresh(GameAction.CharacterDamaged, OnCharacterDamaged);
            RegisterRefresh(GameAction.ItemUsed, OnItemUsed);
            RegisterRefresh(GameAction.CardPlayed, OnCardPlayed);
            RegisterRefresh(GameAction.CardTransformed, OnCardTransformed);
            RegisterRefresh(GameAction.CardDiscarded, OnCardDiscarded);
            RegisterRefresh(GameAction.CardDrawn, OnCardDraw);
            RegisterRefresh(GameAction.ValueRolled, OnValueRolled);

            RegisterRefresh(GameAction.AbilityTrigger, OnAbilityTrigger);
            RegisterRefresh(GameAction.AbilityTargetCard, OnAbilityTargetCard);
            RegisterRefresh(GameAction.AbilityTargetCharacter, OnAbilityTargetCharacter);
            RegisterRefresh(GameAction.AbilityTargetSlot, OnAbilityTargetSlot);
            RegisterRefresh(GameAction.AbilityEnd, OnAbilityAfter);

            RegisterRefresh(GameAction.RefreshBattle, OnRefreshBattle);

            //------

            TcgNetwork.Get().onConnect += OnConnectedServer;
            TcgNetwork.Get().onDisconnect += OnDisconnectedServer;
        }

        protected virtual void OnDestroy()
        {
            TcgNetwork.Get().onConnect -= OnConnectedServer;
            TcgNetwork.Get().onDisconnect -= OnDisconnectedServer;
        }

        protected virtual void Update()
        {
            bool is_starting = world_data == null || world_data.state == WorldState.None;
            bool is_client = !TcgNetwork.Get().IsHost;
            bool is_connecting = TcgNetwork.Get().IsConnecting();
            bool is_connected = TcgNetwork.Get().IsConnected();

            //Reconnect to server
            if (!is_starting && !is_connecting && is_client && !is_connected)
            {
                timer += Time.deltaTime;
                if (timer > 5f)
                {
                    timer = 0f;
                    ConnectToServer();
                }
            }
        }

        //--------------------

        //Join server or host server based on connect_settings
        public virtual void Connect()
        {
            if (IsReady())
                return;

            LoginUser();

            if (connect_settings.IsServerHost())
                HostServer();
            else
                ConnectToServer();
        }

        private void LoginUser()
        {
            if (!Authenticator.Get().IsSignedIn())
            {
                Authenticator.Get().LoginTest("Player");
            }
        }

        public virtual async void HostServer()
        {
            await Task.Yield(); //Wait for initialization to finish

            if (TcgNetwork.Get().IsActive())
                return; // Already connected

            if (connect_settings.IsRelay())
                TcgNetwork.Get().StartHostRelay(connect_settings.relay_data);
            else if (connect_settings.IsOnline() || NetworkData.Get().solo_type == SoloType.UseNetcode)
                TcgNetwork.Get().StartHost(NetworkData.Get().port);
            else
                TcgNetwork.Get().StartHostOffline();
        }

        public virtual async void ConnectToServer()
        {
            await Task.Yield(); //Wait for initialization to finish

            if (TcgNetwork.Get().IsActive())
                return; // Already connected

            if (connect_settings.IsRelay())
                TcgNetwork.Get().StartClientRelay(connect_settings.relay_data);
            else
                TcgNetwork.Get().StartClient(connect_settings.GetUrl(), NetworkData.Get().port);       //Join server
        }

        public virtual async void ConnectToGame()
        {
            await Task.Yield(); //Wait for initialization to finish

            if (!TcgNetwork.Get().IsActive())
                return; //Not connected to server

            Debug.Log("Connect to Game: " + connect_settings.game_uid);

            MsgPlayerConnect nplayer = new MsgPlayerConnect();
            nplayer.user_id = Authenticator.Get().UserID;
            nplayer.username = Authenticator.Get().Username;
            nplayer.game_uid = connect_settings.game_uid;

            Messaging.SendObject("connect", ServerID, nplayer, NetworkDelivery.Reliable);
        }

        public virtual void Disconnect()
        {
            TcgNetwork.Get().Disconnect();
            world_data = null;
            onDisconnect?.Invoke();
        }

        private void RegisterRefresh(ushort tag, UnityAction<SerializedData> callback)
        {
            RefreshEvent cmdevt = new RefreshEvent();
            cmdevt.tag = tag;
            cmdevt.callback = callback;
            registered_commands.Add(tag, cmdevt);
        }

        public void OnReceiveRefresh(ulong client_id, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ushort type);
            bool found = registered_commands.TryGetValue(type, out RefreshEvent command);
            if (found)
            {
                command.callback.Invoke(new SerializedData(reader));
            }
        }

        //------ Scenario

        public void CreateGame(ScenarioData scenario)
        {
            string filename = connect_settings.filename != null ? connect_settings.filename : "test";
            string title = connect_settings.filename != null ? connect_settings.title : filename;

            if (connect_settings.load)
            {
                //Load Game
                LoadScenario(filename);
            }
            else
            {
                //New Game (default to test scenario, can be changed)
                NewScenario(scenario, filename, title, connect_settings.IsOnline());
            }
        }

        public void NewScenario(ScenarioData scenario, string filename, bool online = false)
        {
            MsgNewScenario mdata = new MsgNewScenario();
            mdata.scenario = scenario.id;
            mdata.filename = filename;
            mdata.title = filename;
            mdata.online = online;
            SendAction(GameAction.NewScenario, mdata);
        }

        public void NewScenario(ScenarioData scenario, string filename, string title, bool online = false)
        {
            MsgNewScenario mdata = new MsgNewScenario();
            mdata.scenario = scenario.id;
            mdata.filename = filename;
            mdata.title = title;
            mdata.online = online;
            SendAction(GameAction.NewScenario, mdata);
        }

        public void LoadScenario(string filename)
        {
            World world = World.Load(filename);
            if (world != null && world.state != WorldState.None)
            {
                LoadScenario(world);
            }
        }

        public void LoadScenario(World world)
        {
            MsgLoadScenario mdata = new MsgLoadScenario();
            mdata.world = world;
            SendAction(GameAction.LoadScenario, mdata, NetworkDelivery.ReliableFragmentedSequenced);
        }

        public void SendUserData(UserData udata)
        {
            if (udata != null)
            {
                MsgUserData mdata = new MsgUserData();
                mdata.user_data = udata;
                SendAction(GameAction.SendUserData, mdata, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public void CreateChampion(ChampionData champion, int slot_x)
        {
            MsgAddChampion mdata = new MsgAddChampion();
            mdata.champion_id = champion.id;
            mdata.slot_x = slot_x;
            SendAction(GameAction.CreateChampion, mdata);
        }

        public void DeleteChampion(int slot_x)
        {
            MsgAddChampion mdata = new MsgAddChampion();
            mdata.champion_id = "";
            mdata.slot_x = slot_x;
            SendAction(GameAction.DeleteChampion, mdata);
        }

        public void StartGame()
        {
            MsgStartGame mdata = new MsgStartGame();
            mdata.seed = GameTool.GenerateRandomInt();
            SendAction(GameAction.StartGame, mdata);
        }

        public void StartGame(int seed)
        {
            MsgStartGame mdata = new MsgStartGame();
            mdata.seed = seed;
            SendAction(GameAction.StartGame, mdata);
        }

        public void StartTest(WorldState test_state)
        {
            MsgStartTest mdata = new MsgStartTest();
            mdata.seed = GameTool.GenerateRandomInt();
            mdata.test_state = test_state;
            SendAction(GameAction.StartTest, mdata);
        }

        //------ Map Actions

        public void MapMove(MapLocation location)
        {
            MsgMapMove mdata = new MsgMapMove();
            mdata.map_id = location.map_id;
            mdata.location_id = location.ID;
            SendAction(GameAction.MapMove, mdata);
        }

        public void MapEventContinue()
        {
            SendAction(GameAction.MapEventDone);
        }

        public void MapEventContinue(Champion champion)
        {
            if (champion != null)
            {
                MsgChampion msg = new MsgChampion();
                msg.champion_uid = champion.uid;
                SendAction(GameAction.MapEventDoneChampion, msg);
            }
        }

        public void MapSelectChoice(Champion champion, EventData choice)
        {
            MsgEventChoice mdata = new MsgEventChoice();
            mdata.champion_uid = champion.uid;
            mdata.choice = choice.id;
            SendAction(GameAction.MapEventChoice, mdata);
        }

        public void MapSelectCardReward(Champion champion, CardData card)
        {
            MsgRewardChoice mdata = new MsgRewardChoice();
            mdata.champion_uid = champion.uid;
            mdata.target_id = card != null ? card.id : "";
            SendAction(GameAction.MapRewardCardChoice, mdata);
        }

        public void MapSelectItemReward(Champion champion, CardData card)
        {
            MsgRewardChoice mdata = new MsgRewardChoice();
            mdata.champion_uid = champion.uid;
            mdata.target_id = card != null ? card.id : "";
            SendAction(GameAction.MapRewardItemChoice, mdata);
        }

        public void MapUpgradeCard(Champion champion, ChampionCard card)
        {
            MsgChampionCard mdata = new MsgChampionCard();
            mdata.champion_uid = champion.uid;
            mdata.card_uid = card.uid;
            SendAction(GameAction.MapUpgradeCard, mdata);
        }

        public void MapTrashCard(Champion champion, ChampionCard card)
        {
            MsgChampionCard mdata = new MsgChampionCard();
            mdata.champion_uid = champion.uid;
            mdata.card_uid = card.uid;
            SendAction(GameAction.MapTrashCard, mdata);
        }
        
        public void MapBuyCard(Champion champion, CardData card)
        {
            MsgShop mdata = new MsgShop();
            mdata.champion_uid = champion.uid;
            mdata.item_id = card.id;
            SendAction(GameAction.ShopBuyCard, mdata);
        }

        public void MapBuyItem(Champion champion, CardData item)
        {
            MsgShop mdata = new MsgShop();
            mdata.champion_uid = champion.uid;
            mdata.item_id = item.id;
            SendAction(GameAction.ShopBuyItem, mdata);
        }

        public void MapSellItem(Champion champion, CardData item)
        {
            MsgShop mdata = new MsgShop();
            mdata.champion_uid = champion.uid;
            mdata.item_id = item.id;
            SendAction(GameAction.ShopSellItem, mdata);
        }

        public void UseItem(Champion character, ChampionCard item)
        {
            MsgUse mdata = new MsgUse();
            mdata.character_uid = character.uid;
            mdata.uid = item.uid;
            SendAction(GameAction.UseItem, mdata);
        }

        public void UseItem(BattleCharacter character, Card item)
        {
            MsgUse mdata = new MsgUse();
            mdata.character_uid = character.uid;
            mdata.uid = item.uid;
            SendAction(GameAction.UseItem, mdata);
        }

        public void LevelUp(Champion character)
        {
            MsgChampion mdata = new MsgChampion();
            mdata.champion_uid = character.uid;
            SendAction(GameAction.MapLevelUp, mdata);
        }

        //--------------------------

        public void PlayCard(Card card, Slot target)
        {
            MsgPlayCard mdata = new MsgPlayCard();
            mdata.card_uid = card.uid;
            mdata.slot = target;
            SendAction(GameAction.PlayCard, mdata);
        }

        public void MoveCharacter(BattleCharacter character, Slot target)
        {
            MsgMove mdata = new MsgMove();
            mdata.character_uid = character.uid;
            mdata.slot = target;
            SendAction(GameAction.Move, mdata);
        }

        public void CastAbility(Card card, AbilityData ability)
        {
            MsgCastAbility mdata = new MsgCastAbility();
            mdata.card_uid = card.uid;
            mdata.ability_id = ability.id;
            mdata.target_uid = "";
            SendAction(GameAction.CastAbility, mdata);
        }

        public void SelectCard(Card card)
        {
            MsgUID mdata = new MsgUID();
            mdata.uid = card.uid;
            SendAction(GameAction.SelectCard, mdata);
        }

        public void SelectCharacter(BattleCharacter character)
        {
            MsgUID mdata = new MsgUID();
            mdata.uid = character.uid;
            SendAction(GameAction.SelectCharacter, mdata);
        }

        public void SelectSlot(Slot slot)
        {
            SendAction(GameAction.SelectSlot, slot);
        }

        public void SelectChoice(int c)
        {
            MsgInt choice = new MsgInt();
            choice.value = c;
            SendAction(GameAction.SelectChoice, choice);
        }

        public void CancelSelection()
        {
            SendAction(GameAction.CancelSelect);
        }

        public void SendChatMsg(string msg)
        {
            MsgChat chat = new MsgChat();
            chat.text = msg;
            chat.username = Authenticator.Get().Username;
            SendAction(GameAction.ChatMessage, chat);
        }

        public void EndTurn()
        {
            SendAction(GameAction.EndTurn);
        }

        public void Resign()
        {
            SendAction(GameAction.Resign);
        }

        public void SendAction<T>(ushort type, T data, NetworkDelivery delivery = NetworkDelivery.Reliable) where T : INetworkSerializable
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteNetworkSerializable(data);
            Messaging.Send("action", ServerID, writer, delivery);
            writer.Dispose();
        }

        public void SendAction(ushort type, byte[] bytes, NetworkDelivery delivery = NetworkDelivery.Reliable)
        {
            FastBufferWriter writer = new FastBufferWriter(bytes.Length, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(bytes);
            Messaging.Send("action", ServerID, writer, delivery);
            writer.Dispose();
        }

        public void SendAction(ushort type, int data)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(data);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        public void SendAction(ushort type)
        {
            FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp, TcgNetwork.MsgSizeMax);
            writer.WriteValueSafe(type);
            Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
            writer.Dispose();
        }

        //--- Receive Refresh ----------------------

        protected virtual void OnConnectedServer()
        {
            TcgNetwork.Get().Messaging.ListenMsg("refresh", OnReceiveRefresh);
            ConnectToGame();
            onConnectedServer?.Invoke();
        }

        protected virtual void OnDisconnectedServer()
        {
            TcgNetwork.Get().Messaging.UnListenMsg("refresh");
        }

        protected virtual void OnConnectedToGame(SerializedData sdata)
        {
            MsgAfterConnected msg = sdata.Get<MsgAfterConnected>();
            player_id = msg.player_id;
            world_data = msg.world;

            if (onConnectedGame != null)
                onConnectedGame.Invoke();

            UserData udata = Authenticator.Get().GetUserData();
            SendUserData(udata);
        }

        private void OnGameStart(SerializedData sdata)
        {
            onGameStart?.Invoke();
        }

        private void OnGameEnd(SerializedData sdata)
        {
            onGameEnd?.Invoke();
        }

        private void OnChat(SerializedData sdata)
        {
            MsgChat msg = sdata.Get<MsgChat>();
            onChatMsg?.Invoke(msg.username, msg.text);
        }

        private void OnServerMsg(SerializedData sdata)
        {
            string msg = sdata.GetString();
            onServerMsg?.Invoke(msg);
        }

        private void OnRefreshWorld(SerializedData sdata)
        {
            MsgRefreshWorld msg = sdata.Get<MsgRefreshWorld>();
            world_data = msg.world_data;
            onRefreshWorld?.Invoke();
        }

        private void OnRefreshBattle(SerializedData sdata)
        {
            MsgRefreshBattle msg = sdata.Get<MsgRefreshBattle>();
            world_data.battle = msg.battle_data;
            onRefreshBattle?.Invoke();
        }

        //-----------------------------------

        private void OnMapMoved(SerializedData sdata)
        {
            MsgMapMove msg = sdata.Get<MsgMapMove>();
            MapLocation loc = world_data.GetLocation(msg.map_id, msg.location_id);
            onMapMove?.Invoke(loc);
        }

        private void OnMapRewardSelected(SerializedData sdata)
        {
            MsgRewardChoice msg = sdata.Get<MsgRewardChoice>();
            Champion champ = world_data.GetChampion(msg.champion_uid);
            CardData card = CardData.Get(msg.target_id);
            onRewardChoice?.Invoke(champ, card);
        }

        private void OnMapChoiceSelected(SerializedData sdata)
        {
            MsgEventChoice msg = sdata.Get<MsgEventChoice>();
            Champion champ = world_data.GetChampion(msg.champion_uid);
            EventData choice = EventData.Get(msg.choice);
            onEventChoice?.Invoke(champ, choice);
        }


        //-----------------------------------

        private void OnBattleStart(SerializedData sdata)
        {
            onBattleStart?.Invoke();
        }

        private void OnBattleEnd(SerializedData sdata)
        {
            onBattleEnd?.Invoke();
        }

        private void OnNewTurn(SerializedData sdata)
        {
            onNewTurn?.Invoke();
        }

        private void OnCardPlayed(SerializedData sdata)
        {
            MsgPlayCard msg = sdata.Get<MsgPlayCard>();
            Card card = world_data.battle.GetCard(msg.card_uid);
            onCardPlayed?.Invoke(card, msg.slot);
        }

        private void OnCharacterMoved(SerializedData sdata)
        {
            MsgMove msg = sdata.Get<MsgMove>();
            BattleCharacter character = world_data.battle.GetCharacter(msg.character_uid);
            onCharacterMoved?.Invoke(character, msg.slot);
        }

        private void OnCharacterDamaged(SerializedData sdata)
        {
            MsgDamaged msg = sdata.Get<MsgDamaged>();
            BattleCharacter character = world_data.battle.GetCharacter(msg.character_uid);
            onCharacterDamaged?.Invoke(character, msg.damage);
        }

        private void OnItemUsed(SerializedData sdata)
        {
            MsgUse msg = sdata.Get<MsgUse>();
            BattleCharacter character = world_data.battle.GetCharacter(msg.character_uid);
            Card card = character?.GetCard(msg.uid);
            onItemUsed?.Invoke(card);
        }

        private void OnCardTransformed(SerializedData sdata)
        {
            MsgUID msg = sdata.Get<MsgUID>();
            Card card = world_data.battle.GetCard(msg.uid);
            onCardTransformed?.Invoke(card);
        }

        private void OnCardDiscarded(SerializedData sdata)
        {
            MsgUID msg = sdata.Get<MsgUID>();
            Card card = world_data.battle.GetCard(msg.uid);
            onCardDiscarded?.Invoke(card);
        }

        private void OnCardDraw(SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            onCardDraw?.Invoke(msg.value);
        }

        private void OnValueRolled(SerializedData sdata)
        {
            MsgInt msg = sdata.Get<MsgInt>();
            onValueRolled?.Invoke(msg.value);
        }

        private void OnAbilityTrigger(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.ability_id);
            Card caster = world_data.battle.GetCard(msg.card_uid);
            onAbilityStart?.Invoke(ability, caster);
        }

        private void OnAbilityTargetCard(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.ability_id);
            Card caster = world_data.battle.GetCard(msg.card_uid);
            Card target = world_data.battle.GetCard(msg.target_uid);
            onAbilityTargetCard?.Invoke(ability, caster, target);
        }

        private void OnAbilityTargetCharacter(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.ability_id);
            Card caster = world_data.battle.GetCard(msg.card_uid);
            BattleCharacter target = world_data.battle.GetCharacter(msg.target_uid);
            onAbilityTargetCharacter?.Invoke(ability, caster, target);
        }

        private void OnAbilityTargetSlot(SerializedData sdata)
        {
            MsgCastAbilitySlot msg = sdata.Get<MsgCastAbilitySlot>();
            AbilityData ability = AbilityData.Get(msg.ability_id);
            Card caster = world_data.battle.GetCard(msg.card_uid);
            onAbilityTargetSlot?.Invoke(ability, caster, msg.slot);
        }

        private void OnAbilityAfter(SerializedData sdata)
        {
            MsgCastAbility msg = sdata.Get<MsgCastAbility>();
            AbilityData ability = AbilityData.Get(msg.ability_id);
            Card caster = world_data.battle.GetCard(msg.card_uid);
            onAbilityEnd?.Invoke(ability, caster);
        }

        //--------------------------

        public virtual bool IsConnected()
        {
            return TcgNetwork.Get().IsConnected();
        }

        public virtual bool IsReady()
        {
            return world_data != null && world_data.state != WorldState.None && TcgNetwork.Get().IsConnected();
        }

        public virtual bool IsBattleReady()
        {
            return world_data != null && world_data.battle != null && TcgNetwork.Get().IsConnected();
        }

        public virtual bool IsGameStarted()
        {
            return world_data != null && world_data.state != WorldState.None && world_data.state != WorldState.Setup;
        }

        public virtual bool IsGameCreated()
        {
            return world_data != null && world_data.state != WorldState.None;
        }

        public Player GetPlayer()
        {
            World wdata = GetWorld();
            return wdata.GetPlayer(GetPlayerID());
        }

        public Champion GetChampion()
        {
            World wdata = GetWorld();
            return wdata.GetChampion(GetPlayerID());
        }

        public int GetPlayerID()
        {
            return player_id;
        }

        public int GetOpponentPlayerID()
        {
            return GetPlayerID() == 0 ? 1 : 0;
        }

        public virtual bool IsYourTurn()
        {
            Battle game_data = GetBattle();
            return IsReady() && game_data.IsPlayerTurn(player_id);
        }

        public World GetWorld()
        {
            return world_data;
        }

        public Battle GetBattle()
        {
            if(world_data != null)
                return world_data.battle;
            return null;
        }

        public bool IsHost { get { return TcgNetwork.Get().IsHost; } }
        public ulong ServerID { get { return TcgNetwork.Get().ServerID; } }
        public NetworkMessaging Messaging { get { return TcgNetwork.Get().Messaging; } }

        public static GameClient Get()
        {
            return instance;
        }

    }

    public class RefreshEvent
    {
        public ushort tag;
        public UnityAction<SerializedData> callback;
    }
}