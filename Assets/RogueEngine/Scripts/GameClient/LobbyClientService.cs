using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace RogueEngine.Client
{
    /// <summary>
    /// Connect to Unity Lobby Services
    /// </summary>

    public class LobbyClientService : LobbyClient
    {

        private Lobby lobby;
        private LobbyEventCallbacks lobby_events;
        private string unity_player_id = "";
        private RelayConnectData relay_data;

        private const int max_lobby_display = 32;

        protected static LobbyClientService instance;

        protected virtual void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            enabled = NetworkData.Get().server_type == ServerType.UnityServices;
        }

        public override async void Connect()
        {
            bool is_login = Authenticator.Get().IsConnected();
            unity_player_id = Authenticator.Get().UserID;
            logged_in = enabled && is_login;
            onConnect?.Invoke(is_login);
            await TimeTool.Delay(0);
        }

        public override async void RefreshLobby()
        {
            List<QueryFilter> filters = new List<QueryFilter>();
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Count = max_lobby_display,
                Filters = filters
            };

            QueryResponse res = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            LobbyList list = ConvertList(res.Results);
            onLobbyList?.Invoke(list);
        }

        public override async void RefreshGame()
        {
            if (!IsInLobby())
                return;

            lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

            LobbyGame game = ConvertLobby(lobby);
            onLobbyRefresh?.Invoke(game);
        }

        public override async void CreateGame(string title, string subtitle, string filename, bool load = false)
        {
            Dictionary<string, PlayerDataObject> user_data = new Dictionary<string, PlayerDataObject>();
            PlayerDataObject user_obj = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, Authenticator.Get().Username);
            user_data.Add("Username", user_obj);

            int state = 0;
            int load_state = load ? 1 : 0;
            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Unity.Services.Lobbies.Models.Player(id: unity_player_id, data: user_data),
                Data = new Dictionary<string, DataObject>()
                {
                    {"Subtitle", new DataObject(DataObject.VisibilityOptions.Public, subtitle)},
                    {"Filename", new DataObject(DataObject.VisibilityOptions.Public, filename)},
                    {"Load", new DataObject(DataObject.VisibilityOptions.Public, load_state.ToString())},
                    {"Started", new DataObject(DataObject.VisibilityOptions.Public, state.ToString())},
                }
            };

            loading = true;
            lobby = await LobbyService.Instance.CreateLobbyAsync(title, players_max, createOptions);
            LinkEvents();
            loading = false;

            LobbyGame game = ConvertLobby(lobby);
            onLobbyRefresh?.Invoke(game);
        }

        public override async void JoinGame(string game_id)
        {
            Dictionary<string, PlayerDataObject> user_data = new Dictionary<string, PlayerDataObject>();

            PlayerDataObject user_obj = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, Authenticator.Get().Username);
            user_data.Add("Username", user_obj);

            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions { Player = new Unity.Services.Lobbies.Models.Player(id: unity_player_id, data: user_data) };
            loading = true;
            lobby = await LobbyService.Instance.JoinLobbyByIdAsync(game_id, joinOptions);
            LinkEvents();
            loading = false;

            LobbyGame game = ConvertLobby(lobby);
            onLobbyRefresh?.Invoke(game);
        }

        public override async void LeaveGame()
        {
            if (!IsInLobby())
                return;

            string lobby_id = lobby.Id;
            await LobbyService.Instance.RemovePlayerAsync(lobby_id, unity_player_id);

            RemoveFromLobby();
            RefreshLobby();
        }

        public override async void StartGame()
        {
            if (!IsInLobby())
                return;

            loading = true;

            string join_code = "";
            if (NetworkData.Get().server_type == ServerType.UnityServices)
            {
                //Before starting the game, need to create it on the relay server to get the join_code
                relay_data = await NetworkRelay.HostGame(players_max);
                if (relay_data == null)
                    return; //Failed to create relay game
                join_code = relay_data.join_code;
                Debug.Log("RELAY HOST CODE " + relay_data.join_code);
            }

            int state = 1;
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>()
                {
                    {"Started", new DataObject(DataObject.VisibilityOptions.Public, state.ToString())},
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Member, join_code)}
                }
            };

            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateOptions);
            loading = false;

            LobbyGame game = ConvertLobby(lobby);
            onLobbyRefresh?.Invoke(game);
        }

        public override async void SendChat(string text)
        {
            LobbyGame game = ConvertLobby(lobby);
            MsgChat msg = new MsgChat(Username, text);
            game.chats.Add(msg);

            if (game.chats.Count > 20)
                game.chats.RemoveAt(0);

            string data = WriteChat(game.chats, Username);

            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    {"Chat", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, data)}
                }
            };

            lobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, unity_player_id, updateOptions);

            LobbyGame ugame = ConvertLobby(lobby);
            onLobbyRefresh?.Invoke(ugame);
        }

        public override void StartMatchmaking(string group, int nb_players)
        {
            Debug.LogError("Matchmaking NOT supported with Unity Services");
        }

        public override async void Disconnect()
        {
            logged_in = false;
            unity_player_id = "";
            loading = false;
            RemoveFromLobby();
            await TimeTool.Delay(0);
        }

        public override async void KeepAlive()
        {
            if (!IsInLobby())
                return;

            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
        }

        private async void LinkEvents()
        {
            lobby_events = new LobbyEventCallbacks();
            lobby_events.LobbyDeleted += OnLobbyDelete;
            lobby_events.KickedFromLobby += OnKicked;
            lobby_events.PlayerJoined += OnPlayerJoin;
            lobby_events.PlayerLeft += OnPlayerLeft;

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, lobby_events);
        }

        private void RemoveFromLobby()
        {
            lobby = null;
            lobby_events = new LobbyEventCallbacks();
            joined_game_id = "";
        }

        private void OnLobbyDelete()
        {
            if (lobby != null)
            {
                RemoveFromLobby();
                RefreshLobby();
            }
        }

        private void OnKicked()
        {
            if (lobby != null)
            {
                RemoveFromLobby();
                RefreshLobby();
            }
        }

        private void OnPlayerJoin(List<LobbyPlayerJoined> players)
        {

        }

        private void OnPlayerLeft(List<int> players)
        {

        }

        public LobbyList ConvertList(List<Lobby> lobbies)
        {
            LobbyList list = new LobbyList();
            List<LobbyGame> games = new List<LobbyGame>();
            for (int i = 0; i < lobbies.Count; i++)
            {
                LobbyGame game = ConvertLobby(lobbies[i]);
                if (!game.started)
                    games.Add(game);
            }
            list.games = games.ToArray();
            return list;
        }

        public LobbyGame ConvertLobby(Lobby lobby)
        {
            string game_uid = lobby.Id;
            LobbyGame game = new LobbyGame(game_uid);
            game.title = lobby.Name;
            game.players_max = lobby.MaxPlayers;
            game.players = new List<LobbyPlayer>();

            game.subtitle = GetValue(lobby, "Subtitle");
            game.filename = GetValue(lobby, "Filename");
            game.join_code = GetValue(lobby, "JoinCode");

            string state_str = GetValue(lobby, "Started");
            bool valid_state = int.TryParse(state_str, out int state_int);
            game.started = valid_state && state_int > 0;

            foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
            {
                string username = GetValue(player, "Username");
                LobbyPlayer lplayer = new LobbyPlayer(player.Id, username);
                lplayer.game_uid = game_uid;
                game.players.Add(lplayer);
            }

            game.chats = ReadChat(lobby.Players);

            return game;
        }

        private string GetValue(Lobby lobby, string id)
        {
            if (lobby.Data != null)
            {
                bool valid = lobby.Data.TryGetValue(id, out DataObject val);
                return valid ? val.Value : "";
            }
            return "";
        }

        private string GetValue(Unity.Services.Lobbies.Models.Player player, string id)
        {
            if (player.Data != null)
            {
                bool valid = player.Data.TryGetValue(id, out PlayerDataObject val);
                return valid ? val.Value : "";
            }
            return "";
        }

        public string WriteChat(List<MsgChat> msgs, string username)
        {
            int index = 0;
            List<string> lines = new List<string>();
            foreach (MsgChat msg in msgs)
            {
                if (msg.username == username)
                {
                    string line = index + "--::--" + msg.text;
                    lines.Add(line);
                }
                index++;
            }
            return string.Join("--$$--", lines);
        }

        public List<MsgChat> ReadChat(List<Unity.Services.Lobbies.Models.Player> players)
        {
            List<MsgChat> msgs = new List<MsgChat>();
            foreach (Unity.Services.Lobbies.Models.Player player in players)
            {
                string username = GetValue(player, "Username");
                string chat_str = GetValue(player, "Chat");
                List<MsgChat> player_chat = ReadChat(chat_str, username);
                msgs.AddRange(player_chat);
            }

            msgs.Sort((MsgChat a, MsgChat b) => { return a.index.CompareTo(b.index); });

            return msgs;
        }

        public List<MsgChat> ReadChat(string data, string username)
        {
            string[] lines = data.Split(new string[] { "--$$--" }, System.StringSplitOptions.None);
            List<MsgChat> msgs = new List<MsgChat>();
            foreach (string line in lines)
            {
                if (line.Contains("--::--"))
                {
                    MsgChat msg = new MsgChat();
                    string[] parts = line.Split(new string[] { "--::--" }, System.StringSplitOptions.None);
                    int.TryParse(parts[0], out int index);
                    msg.index = index;
                    msg.text = parts[1];
                    msg.username = username;
                    msgs.Add(msg);
                }
            }
            return msgs;
        }

        public override bool IsInLobby() { return logged_in && lobby != null; }
        public override RelayConnectData GetRelayData() { return relay_data; }

        public static new LobbyClientService Get()
        {
            return instance;
        }

    }
}
