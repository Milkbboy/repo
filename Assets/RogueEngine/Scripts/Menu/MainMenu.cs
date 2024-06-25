using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    /// <summary>
    /// Main script for the main menu scene
    /// </summary>

    public class MainMenu : MonoBehaviour
    {
        public AudioClip music;
        public AudioClip ambience;

        [Header("Player UI")]
        public Text username_txt;
        public AvatarUI avatar;
        public GameObject loader;

        [Header("UI")]
        public Text version_text;

        private bool starting = false;

        private static MainMenu instance;

        void Awake()
        {
            instance = this;

            //Set default settings
            Application.targetFrameRate = 120;
            GameClient.connect_settings = ConnectSettings.Default;
            World.Unload();
        }

        private void Start()
        {
            BlackPanel.Get().Show(true);
            AudioTool.Get().PlayMusic("music", music);
            AudioTool.Get().PlaySFX("ambience", ambience, 0.5f, true, true);

            username_txt.text = "";
            version_text.text = "Version " + Application.version;

            if (Authenticator.Get().IsConnected())
                AfterLogin();
            else
                RefreshLogin();
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if (success)
                AfterLogin();
            else
                SceneNav.GoToLoginMenu();
        }

        private void AfterLogin()
        {
            BlackPanel.Get().Hide();
            RefreshUser();
        }

        public async void RefreshUser()
        {
            await Authenticator.Get().LoadUserData();

            username_txt.text = Authenticator.Get().Username;

            UserData udata = Authenticator.Get().GetUserData();
            if (udata == null)
                return;

            AvatarData avatard = AvatarData.Get(udata.avatar);
            avatar.SetAvatar(avatard);
        }

        void Update()
        {
            bool matchmaking = LobbyClient.Get().IsMatchmaking();
            if (loader.activeSelf != matchmaking)
                loader.SetActive(matchmaking);
            bool loading = LobbyClient.Get().IsLoading();
            if(MenuLoadPanel.Get().IsVisible() != loading)
                MenuLoadPanel.Get().SetVisible(loading);
        }

        public void CreateGame(GameType type)
        {
            string user_id = Authenticator.Get().UserID;
            string file = user_id + "_" + (type == GameType.MultiHost ? "lan": "solo");
            CreateGame(type, file, GameTool.GenerateRandomID());
        }

        public void CreateGame(GameType type, string filename, string game_uid)
        {
            GameClient.connect_settings.game_type = type;
            GameClient.connect_settings.file_host = true;
            GameClient.connect_settings.load = false;
            GameClient.connect_settings.server_url = "";
            GameClient.connect_settings.game_uid = game_uid;
            GameClient.connect_settings.filename = filename;
            StartGame(); 
        }

        public void LoadGame(GameType type, string filename)
        {
            string user_id = Authenticator.Get().UserID;
            World game = World.Load(filename);
            if (game != null && game.GetPlayer(user_id) != null)
            {
                GameClient.connect_settings.game_type = type;
                GameClient.connect_settings.file_host = true;
                GameClient.connect_settings.load = true;
                GameClient.connect_settings.server_url = "";
                GameClient.connect_settings.game_uid = game.game_uid;
                GameClient.connect_settings.filename = filename;

                StartGame();
            }
        }

        public void JoinGame(GameType type, string game_uid, string server_url)
        {
            GameClient.connect_settings.game_type = type;
            GameClient.connect_settings.file_host = false;
            GameClient.connect_settings.load = false;
            GameClient.connect_settings.server_url = server_url; //If Empty server_url, will use the default one in NetworkData
            GameClient.connect_settings.game_uid = game_uid;
            GameClient.connect_settings.filename = "";
            StartGame();
        }

        public void StartGame()
        {
            if (!starting)
            {
                starting = true;
                LobbyClient.Get().Disconnect();
                StartCoroutine(FadeToGame());
            }
        }

        public void StartMathmaking(string group)
        {
            GameClient.connect_settings.game_type = GameType.MultiJoin;
            LobbyClient.Get().StartMatchmaking(group, 2);
        }

        public void OnClickSoloNew()
        {
            CreateGame(GameType.Solo);
        }

        public void OnClickSoloLoad()
        {
            string user_id = Authenticator.Get().UserID;
            LoadGame(GameType.Solo, World.GetLastSave(user_id));
        }

        public void OnClickMultiLobby()
        {
            LobbyPanel.Get().Connect();
            //StartMathmaking("");
        }

        public void OnClickMultiLAN()
        {
            LanPanel.Get().Show();
        }

        public void OnClickCancelMatch()
        {
            LobbyClient.Get().StopMatchmaking();
        }

        public void OnClickAvatar()
        {
            AvatarPanel.Get().Show();
        }

        public void OnClickSettings()
        {
            SettingsPanel.Get().Show();
        }

        private IEnumerator FadeToGame()
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            yield return new WaitForSeconds(1f);
            SceneNav.GoToSetup();
        }

        public void OnClickLogout()
        {
            TcgNetwork.Get().Disconnect();
            Authenticator.Get().Logout();
            StartCoroutine(FadeLogout());
        }

        private IEnumerator FadeLogout()
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            yield return new WaitForSeconds(1f);
            SceneNav.GoToLoginMenu();
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }

        public static MainMenu Get()
        {
            return instance;
        }
    }
}
