using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace RogueEngine.Client
{
    /// <summary>
    /// Base class for ClientLobbyService and ClientLobby
    /// </summary>

    public abstract class LobbyClient : MonoBehaviour
    {
        public UnityAction<bool> onConnect;
        public UnityAction<MatchmakingResult> onMatchmaking;
        public UnityAction<LobbyList> onLobbyList;
        public UnityAction<LobbyGame> onLobbyRefresh;
        public UnityAction<string, string> onChat;

        protected string joined_game_id;
        protected bool logged_in = false;
        protected bool loading = false;

        protected float refresh_timer = 0f;
        protected float keep_timer = 0f;

        public static int players_max = 4;

        private static LobbyClient instance;

        protected virtual void Update()
        {
            if (!logged_in)
                return;

            //Refresh lobby
            refresh_timer += Time.deltaTime;
            if (refresh_timer > 1f)
            {
                refresh_timer = 0;
                RefreshGame();
            }

            //Keep alive connection
            keep_timer += Time.deltaTime;
            if (keep_timer > 8f)
            {
                keep_timer = 0;
                KeepAlive();
            }
        }

        public virtual async void Connect()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void RefreshLobby()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void RefreshGame()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void CreateGame(string title, string subtitle, string filename, bool load = false)
        {
            await TimeTool.Delay(0);
        }

        public virtual async void JoinGame(string game_id)
        {
            await TimeTool.Delay(0);
        }

        public virtual async void LeaveGame()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void StartGame()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void SendChat(string text)
        {
            await TimeTool.Delay(0);
        }

        public virtual async void KeepAlive()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void KeepAlive(string game_id)
        {
            await TimeTool.Delay(0);
        }

        public virtual async void StartMatchmaking(string group, int nb_players)
        {
            await TimeTool.Delay(0);
        }

        public virtual async void StopMatchmaking()
        {
            await TimeTool.Delay(0);
        }

        public virtual async void Disconnect()
        {
            await TimeTool.Delay(0);
        }

        protected virtual void ReceiveMatchmakingResult(LobbyGame result)
        {

        }

        public virtual bool IsMatchmaking()
        {
            return false;
        }

        public virtual bool CanConnectToGame() { return logged_in && !TcgNetwork.Get().IsConnected(); }
        public virtual bool IsConnected() { return logged_in; }
        public virtual bool IsInLobby() { return logged_in && !string.IsNullOrEmpty(joined_game_id); }
        public virtual bool IsLoading() { return loading; }
        public virtual RelayConnectData  GetRelayData(){ return null; }

        public string UserID { get { return Authenticator.Get().UserID; } }
        public string Username { get { return Authenticator.Get().Username; } }

        public static LobbyClient Get()
        {
            if (instance == null)
            {
                if (NetworkData.Get().server_type == ServerType.UnityServices)
                    instance = LobbyClientService.Get();
                else
                    instance = LobbyClientDedicated.Get();
            }
            return instance;
        }
    }
}
