using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace RogueEngine.Server
{
    /// <summary>
    /// Main server script for the matchmaker and lobby
    /// will receive player request and then match players together and send them the game uid and game url to connect to
    /// </summary>

    public class LobbyServer : MonoBehaviour
    {
        [Header("Matchmaker")]
        public string[] servers;

        private Dictionary<string, LobbyGame> lobbies = new Dictionary<string, LobbyGame>();
        private List<string> remove_list = new List<string>();

        private Dictionary<ulong, ClientData> client_list = new Dictionary<ulong, ClientData>();  //List of clients
        private Dictionary<string, MatchPlayerData> matchmaking_players = new Dictionary<string, MatchPlayerData>(); //Get deleted every 20 sec
        private Dictionary<string, MatchData> matched_players = new Dictionary<string, MatchData>(); //user_id -> match
        private List<MatchPlayerData> valid_users = new List<MatchPlayerData>(); //temporary array

        private float timer = 0f;
        private float matchmake_timer = 0f;
        private ulong server_tick = 0;

        public static ulong lobby_expiration_time = 30;
        public static int max_players = 4;

        private static LobbyServer _instance;

        protected virtual void Awake()
        {
            _instance = this;
            Application.runInBackground = true;
        }

        protected virtual void Start()
        {
            TcgNetwork network = TcgNetwork.Get();
            network.onClientJoin += OnClientConnected;
            network.onClientQuit += OnClientDisconnected;

            Messaging.ListenMsg("matchmaking", ReceiveMatchmaking);
            Messaging.ListenMsg("lobby_list", ReceiveLobbyList);
            Messaging.ListenMsg("lobby_keep", ReceiveLobbyKeep);
            Messaging.ListenMsg("lobby_create", ReceiveLobbyCreate);
            Messaging.ListenMsg("lobby_join", ReceiveLobbyJoin);
            Messaging.ListenMsg("lobby_quit", ReceiveLobbyQuit);
            Messaging.ListenMsg("lobby_start", ReceiveLobbyStart);
            Messaging.ListenMsg("lobby_chat", ReceiveLobbyChat);

            if (!network.IsActive())
            {
                network.StartServer(NetworkData.Get().port);
            }
        }

        protected virtual void Update()
        {
            //Matchmaking
            matchmake_timer += Time.deltaTime;
            if (matchmake_timer > 20f)
            {
                matchmake_timer = 0f;
                matchmaking_players.Clear(); //Delete and restart, to make sure you only keep recent players
            }

            //Slow Update
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                timer = 0f;
                SlowUpdate();
            }
        }

        void SlowUpdate()
        {
            server_tick++;

            //Remove rooms with no players
            foreach (KeyValuePair<string, LobbyGame> game in lobbies)
            {
                ulong expire_at = game.Value.last_update + lobby_expiration_time;
                if (game.Value.players.Count == 0 || server_tick > expire_at)
                {
                    game.Value.expired = true;
                    remove_list.Add(game.Key);
                }
            }
            foreach (string id in remove_list)
            {
                lobbies.Remove(id);
            }

            if (remove_list.Count > 0)
                remove_list.Clear();
        }

        protected virtual void OnClientConnected(ulong client_id)
        {
            ClientData iclient = new ClientData(client_id);
            client_list[client_id] = iclient;
        }

        protected virtual void OnClientDisconnected(ulong client_id)
        {
            if (client_list.ContainsKey(client_id))
            {
                ClientData iclient = client_list[client_id];
                if(iclient.username != null)
                    matchmaking_players.Remove(iclient.user_id);
                client_list.Remove(client_id);
            }
        }

        protected virtual void ReceiveMatchmaking(ulong client_id, FastBufferReader reader)
        {
            ClientData iclient = GetClient(client_id);
            reader.ReadNetworkSerializable(out MsgMatchmaking msg);

            if (iclient == null || string.IsNullOrWhiteSpace(msg.user_id) || string.IsNullOrWhiteSpace(msg.username))
                return;

            string user_id = msg.user_id;
            bool is_refresh = msg.refresh;

            iclient.user_id = msg.user_id;
            iclient.username = msg.username;

            //Restart matching
            if (!is_refresh)
                matched_players.Remove(user_id);

            //Check if already matched
            if (matched_players.ContainsKey(user_id))
            {
                MatchData match = matched_players[user_id];
                if (!match.ended)
                {
                    SendMatchmakingResponse(iclient, match, msg.group, match.players.Length); //Was already matched, return saved result!
                    return;
                }
            }

            //Data
            MatchPlayerData pdata = new MatchPlayerData();
            pdata.user_id = msg.user_id;
            pdata.username = msg.username;
            pdata.group = msg.group;
            pdata.nb_players = msg.players;

            //Add to matchking players
            if (!matchmaking_players.ContainsKey(user_id))
                matchmaking_players.Add(user_id, pdata);

            //Start searching for other valid players
            float wait_max = 20f;
            int variance_max = 2000;

            bool friendly = msg.group.StartsWith("u_");
            float wait_timer = msg.time;
            float wait_value = Mathf.Clamp01(wait_timer / wait_max);
            int elo_variance = Mathf.RoundToInt(wait_value * variance_max);

            valid_users.Clear();
            valid_users.Add(pdata); //Add self

            foreach (KeyValuePair<string, MatchPlayerData> opair in matchmaking_players)
            {
                string auser_id = opair.Key;
                MatchPlayerData adata = opair.Value;
                bool same_group = adata.group == msg.group;
                bool same_players = adata.nb_players == msg.players;
                if (auser_id != user_id && same_group && same_players)
                {
                    valid_users.Add(adata);
                }
            }

            //Not enough players found, send current count
            if (valid_users.Count < msg.players)
            {
                SendMatchmakingResponse(iclient, null, msg.group, valid_users.Count);
                return; //Not enough valid users
            }

            //Match success, send result
            string prefix = msg.group.Length >= 2 ? msg.group.Substring(0, 2) : "";
            string game_code = prefix + GameTool.GenerateRandomID(12, 15);
            string game_url = ""; //Empty url means it will use the default NetworkData url set on the client
            if (servers.Length > 0)
                game_url = servers[Random.Range(0, servers.Length)];

            int pindex = 0;
            MatchData nmatch = new MatchData(msg.group, game_code, game_url, msg.players);
            foreach (MatchPlayerData vuser in valid_users)
            {
                if (pindex < nmatch.players.Length)
                {
                    matchmaking_players.Remove(vuser.user_id);
                    matched_players[vuser.user_id] = nmatch;
                    nmatch.players[pindex] = vuser.username;
                    pindex++;
                }
            }

            //Send response to current request
            if (matched_players.ContainsKey(user_id))
            {
                SendMatchmakingResponse(iclient, nmatch, nmatch.group, nmatch.players.Length); //Just matched to new player!
            }
        }

        protected virtual void SendMatchmakingResponse(ClientData iclient, MatchData match, string group, int players)
        {
            MatchmakingResult msg_match = new MatchmakingResult();
            msg_match.success = match != null;
            msg_match.players = players;
            msg_match.group = group;
            msg_match.game_uid = match != null ? match.game_uid : "";
            msg_match.server_url = match != null ? match.server_url : "";

            Messaging.SendObject("matchmaking", iclient.client_id, msg_match, NetworkDelivery.Reliable);
        }

        protected virtual void ReceiveLobbyList(ulong client_id, FastBufferReader reader)
        {
            List<LobbyGame> items = new List<LobbyGame>();

            foreach (KeyValuePair<string, LobbyGame> pair in lobbies)
            {
                if (pair.Value.CanBeJoined())
                {
                    LobbyGame pdata = pair.Value;
                    items.Add(pdata);
                }
            }

            LobbyList msg_list = new LobbyList();
            msg_list.games = items.ToArray();

            Messaging.SendObject("lobby_list", client_id, msg_list, NetworkDelivery.Reliable);
        }

        protected virtual void ReceiveLobbyKeep(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgLobbyUID msg);

            if (lobbies.ContainsKey(msg.uid))
            {
                LobbyGame game = lobbies[msg.uid];
                game.last_update = server_tick;

                Messaging.SendObject("lobby_refresh", client_id, game, NetworkDelivery.Reliable);
            }
        }

        protected virtual void ReceiveLobbyCreate(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgLobbyCreate msg);

            string server_url = NetworkData.Get().url;

            LobbyGame game = new LobbyGame();
            game.game_uid = GameTool.GenerateRandomID();
            game.game_url = server_url;
            game.title = msg.title;
            game.subtitle = msg.subtitle;
            game.filename = msg.filename;
            game.join_code = "";
            game.load = msg.load;
            game.players_max = max_players;
            game.last_update = server_tick;
            lobbies[game.game_uid] = game;

            LobbyPlayer player = CreatePlayer(client_id, msg.user_id, msg.username);
            AddPlayerGame(player, game);

            Messaging.SendObject("lobby_refresh", client_id, game, NetworkDelivery.Reliable);
        }

        protected virtual void ReceiveLobbyJoin(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgLobbyJoin msg);

            if (lobbies.ContainsKey(msg.uid))
            {
                LobbyGame game = lobbies[msg.uid];
                if (game.CanBeJoined())
                {
                    LobbyPlayer player = CreatePlayer(client_id, msg.user_id, msg.username);
                    AddPlayerGame(player, game);
                    game.last_update = server_tick;

                    Messaging.SendObject("lobby_refresh", client_id, game, NetworkDelivery.Reliable);
                }
            }
        }

        protected virtual void ReceiveLobbyQuit(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgLobbyJoin msg);

            if (lobbies.ContainsKey(msg.uid))
            {
                LobbyGame game = lobbies[msg.uid];
                RemovePlayerGame(msg.user_id, game);
                Messaging.SendObject("lobby_refresh", client_id, game, NetworkDelivery.Reliable);
            }
        }

        protected virtual void ReceiveLobbyStart(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgLobbyUID msg);

            if (lobbies.ContainsKey(msg.uid))
            {
                LobbyGame game = lobbies[msg.uid];
                game.last_update = server_tick;
                game.started = true;
                Messaging.SendObject("lobby_refresh", client_id, game, NetworkDelivery.Reliable);
            }
        }

        protected virtual void ReceiveLobbyChat(ulong client_id, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgChat msg);
            Messaging.SendObjectAll("lobby_chat", msg, NetworkDelivery.Reliable);
        }

        public void EndMatch(string uid)
        {
            foreach (KeyValuePair<string, MatchData> pair in matched_players)
            {
                if (pair.Value.game_uid == uid)
                    pair.Value.ended = true;
            }
        }

        public ClientData GetClient(ulong client_id)
        {
            if (client_list.ContainsKey(client_id))
                return client_list[client_id];
            return null;
        }

        public ClientData GetClientByUser(string username)
        {
            foreach (KeyValuePair<ulong, ClientData> pair in client_list)
            {
                if (pair.Value.username == username)
                    return pair.Value;
            }
            return null;
        }

        public LobbyPlayer CreatePlayer(ulong client_id, string user_id, string username)
        {
            LobbyPlayer player = new LobbyPlayer();
            player.user_id = user_id;
            player.username = username;
            return player;
        }

        public void AddPlayerGame(LobbyPlayer player, LobbyGame game)
        {
            if (game != null && player != null)
            {
                game.AddPlayer(player);
                game.last_update = server_tick;
            }
        }

        public void RemovePlayerGame(string user_id, LobbyGame game)
        {
            if (game != null)
            {
                game.RemovePlayer(user_id);
            }

            if (game != null && game.players.Count == 0)
            {
                lobbies.Remove(game.game_uid);
            }
        }

        public void RemovePlayerAllGames(string user_id)
        {
            foreach (KeyValuePair<string, LobbyGame> game in lobbies)
            {
                game.Value.RemovePlayer(user_id);
            }
        }

        public LobbyGame GetGame(string game_id)
        {
            if (lobbies.ContainsKey(game_id))
                return lobbies[game_id];
            return null;
        }

        public ulong ServerID { get { return TcgNetwork.Get().ServerID; } }
        public NetworkMessaging Messaging { get { return TcgNetwork.Get().Messaging; } }

        public static LobbyServer Get()
        {
            return _instance;
        }
    }

    public class MatchPlayerData
    {
        public string user_id;
        public string username;
        public string group;
        public int nb_players;
    }

    public class MatchData
    {
        public string group;
        public string game_uid;
        public string server_url;
        public bool ended = false;
        public string[] players;

        public MatchData(string grp, string uid, string url, int players) { group = grp; game_uid = uid; server_url = url; this.players = new string[players]; }
    }



}
