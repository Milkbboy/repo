using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class LobbyPanel : UIPanel
    {
        [Header("Lines Template")]
        public Transform lobby_grid;
        public LobbyLine line_template;
        public UIPanel load_panel;
        public Text connecting_text;
        public Text no_room_text;

        [Header("Game Info")]
        public UIPanel info_panel;
        public Text info_title;
        public Text info_subtitle;
        public Text info_players;
        public Transform players_grid;
        public PlayerLine players_line_template;


        private List<LobbyGame> game_list = new List<LobbyGame>();
        private List<LobbyLine> lobby_lines = new List<LobbyLine>();
        private List<PlayerLine> players_lines = new List<PlayerLine>();

        private LobbyGame selected_room = null;
        private bool joining = false;

        private static LobbyPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            line_template.gameObject.SetActive(false);
            players_line_template.gameObject.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            connecting_text.enabled = !LobbyClient.Get().IsConnected();
        }

        protected override void Start()
        {
            base.Start();

            LobbyClient client = LobbyClient.Get();
            client.onLobbyList += ReceiveRefreshList;
            client.onLobbyRefresh += ReceiveRefresh;
        }

        public void Connect()
        {
            LobbyClient.Get().Connect();
            Show();
        }

        private void RefreshPanel()
        {
            load_panel.Show();
            info_panel.Hide();
            joining = false;
            selected_room = null;
            no_room_text.enabled = false;

            LobbyClient client = LobbyClient.Get();
            client.RefreshLobby();
        }

        private void ReceiveRefreshList(LobbyList list)
        {
            game_list.Clear();
            game_list.AddRange(list.games);
            load_panel.Hide();
            no_room_text.enabled = list.games.Length == 0;

            foreach (LobbyLine line in lobby_lines)
                Destroy(line.gameObject);
            lobby_lines.Clear();

            foreach (LobbyGame game in game_list)
            {
                GameObject obj = Instantiate(line_template.gameObject, lobby_grid);
                obj.SetActive(true);
                LobbyLine line = obj.GetComponent<LobbyLine>();
                line.SetLine(game);
                lobby_lines.Add(line);
            }
        }

        private void RefreshInfoPanel(LobbyGame room)
        {
            if (room == null)
                return;

            foreach (LobbyLine line in lobby_lines)
                line.SetSelected(room == line.GetGame());

            info_title.text = room.title;
            info_subtitle.text = room.subtitle;
            //info_scene.text = room.scene;
            //info_players.text = room.players.Count + "/" + room.players_max;
            info_panel.Show();

            //Players
            foreach (PlayerLine line in players_lines)
                Destroy(line.gameObject);
            players_lines.Clear();

            foreach (LobbyPlayer player in room.players)
            {
                GameObject obj = Instantiate(players_line_template.gameObject, players_grid);
                obj.SetActive(true);
                PlayerLine line = obj.GetComponent<PlayerLine>();
                line.SetLine(player);
                players_lines.Add(line);
            }
        }

        private void ReceiveRefresh(LobbyGame room)
        {
            if (room != null && joining && !room.started)
            {
                Hide();
                joining = false;

                LobbyRoomPanel.Get().ShowGame(room);
            }
        }

        public void OnClickCreate()
        {
            LobbyCreatePanel.Get().ShowCreate();
        }

        public void OnClickLoad()
        {
            LobbyCreatePanel.Get().ShowLoad();
        }

        public void OnClickRefresh()
        {
            RefreshPanel();
        }

        public void OnClickLine(LobbyLine line)
        {
            selected_room = line.GetGame();
            RefreshInfoPanel(selected_room);
        }

        public void OnClickJoin()
        {
            if (selected_room != null)
            {
                joining = true;
                LobbyClient.Get().JoinGame(selected_room.game_uid);
            }
        }

        public void WaitForCreate()
        {
            joining = true;
        }

        public void OnClickBack()
        {
            Hide();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static LobbyPanel Get()
        {
            return instance;
        }
    }
}