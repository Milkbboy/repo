using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class LobbyRoomPanel : UIPanel
    {
        [Header("Game Info")]
        public Text info_title;
        public Text info_subtitle;
        public Text info_players;
        public Transform players_grid;
        public PlayerLine players_line_template;
        public Button start_btn;

        [Header("Chat")]
        public Transform chat_group;
        public GameObject chat_line_template;
        public InputField chat_field;
        public int max_chat_lines = 12;

        private LobbyGame game = null;
        private float removed_timer = 0f;

        private List<PlayerLine> players_lines = new List<PlayerLine>();
        private List<Text> chat_lines = new List<Text>();
        private List<string> chat_text = new List<string>();


        private static LobbyRoomPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            chat_line_template.SetActive(false);
            players_line_template.gameObject.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.Return))
                OnPressChatEnter();

            if (removed_timer > 15f)
                OnClickBack(); //Didnt received msg from server
        }

        protected override void Start()
        {
            base.Start();

            LobbyClient client = LobbyClient.Get();
            client.onLobbyRefresh += ReceiveRefresh;
            client.onChat += ReceiveChat;
        }

        protected void OnDestroy()
        {
            LobbyClient client = LobbyClient.Get();
            client.onLobbyRefresh -= ReceiveRefresh;
            client.onChat -= ReceiveChat;
        }

        private void ReceiveRefresh(LobbyGame room)
        {
            if (!IsVisible())
                return;

            if (room == null)
            {
                OnClickBack(); //Room dont exists anymore, quit
                return;
            }

            if (room.HasPlayer(Authenticator.Get().UserID))
            {
                game = room;
                RefreshGameRoom();

                if (game.started)
                    StartGame(game);
            }
            else
            {
                OnClickBack();
            }
        }

        private void ReceiveChat(string username, string msg)
        {
            if (chat_text.Count >= max_chat_lines)
                chat_text.RemoveAt(0);

            string smsg = "<b>" + username + ":</b> " + msg;
            chat_text.Add(smsg);

            RefreshChat();
        }

        private void ClearGameRoom()
        {
            foreach (PlayerLine line in players_lines)
                Destroy(line.gameObject);
            foreach (Text line in chat_lines)
                Destroy(line.gameObject);
            players_lines.Clear();
            chat_lines.Clear();
            chat_field.text = "";
        }

        private void RefreshGameRoom()
        {
            if (this == null || game == null || !IsVisible())
                return;

            removed_timer = 0f;
            start_btn.interactable = CanStartGame();
            info_title.text = game.title;
            info_subtitle.text = game.subtitle;
            info_players.text = game.players.Count + "/" + game.players_max;

            foreach (PlayerLine line in players_lines)
                line.Hide();

            //Players
            for (int i = 0; i < game.players.Count; i++)
            {
                PlayerLine line = GetPlayerLine(i);
                line.SetLine(game.players[i]);
            }

            RefreshChat();
        }

        private void RefreshChat()
        {
            if (game != null && game.chats.Count > 0)
            {
                chat_text.Clear();
                foreach (MsgChat msg in game.chats)
                {
                    string smsg = "<b>" + msg.username + ":</b> " + msg.text;
                    chat_text.Add(smsg);
                }
            }

            //Chat
            int index = 0;
            foreach (string chat in chat_text)
            {
                if (index >= chat_lines.Count)
                {
                    GameObject obj = Instantiate(chat_line_template.gameObject, chat_group);
                    obj.SetActive(true);
                    Text line = obj.GetComponent<Text>();
                    chat_lines.Add(line);
                }

                chat_lines[index].text = chat;
                index++;
            }
        }

        private PlayerLine GetPlayerLine(int index)
        {
            if (index < players_lines.Count)
                return players_lines[index];
            GameObject obj = Instantiate(players_line_template.gameObject, players_grid);
            obj.SetActive(true);
            PlayerLine line = obj.GetComponent<PlayerLine>();
            players_lines.Add(line);
            return line;
        }

        public void ShowGame(LobbyGame room)
        {
            game = room;
            ClearGameRoom();
            Show();
            RefreshGameRoom();
        }

        public async void StartGame(LobbyGame game)
        {
            UserData udata = Authenticator.Get().UserData;
            bool unity_services = NetworkData.Get().server_type == ServerType.UnityServices;
            bool is_host = game.IsHost(udata.id);

            GameClient.connect_settings.file_host = is_host;
            GameClient.connect_settings.game_type = GameType.MultiJoin; //Dedicated server
            GameClient.connect_settings.server_url = game.game_url;
            GameClient.connect_settings.game_uid = game.game_uid;
            GameClient.connect_settings.title = game.title;
            GameClient.connect_settings.filename = game.filename;
            GameClient.connect_settings.load = game.load;

            if (unity_services)
            {
                GameClient.connect_settings.game_type = is_host ? GameType.RelayHost : GameType.RelayJoin;
                GameClient.connect_settings.relay_data = LobbyClient.Get().GetRelayData();

                if (!is_host)
                {
                    GameClient.connect_settings.relay_data = await NetworkRelay.JoinGame(game.join_code); //Relay data need to be retrieved now
                }
            }

            MainMenu.Get().StartGame();
        }

        public void OnClickStart()
        {
            if (CanStartGame())
            {
                LobbyClient.Get().StartGame();
            }
        }

        public void OnPressChatEnter()
        {
            if (!chat_field.isFocused)
            {
                chat_field.Select();
                chat_field.ActivateInputField();
            }

            OnClickSendChat();
        }

        public void OnClickSendChat()
        {
            if (game == null || chat_field.text.Length == 0)
                return;

            LobbyClient.Get().SendChat(chat_field.text);
            chat_field.text = "";
        }

        public void OnClickBack()
        {
            LobbyClient.Get().LeaveGame();
            LobbyPanel.Get().Show();
            Hide();
        }

        private bool CanStartGame()
        {
            if (game == null) return false;
            bool host = game.IsHost(Authenticator.Get().UserID);
            return !game.started && host;
        }

        public static LobbyRoomPanel Get()
        {
            return instance;
        }
    }
}