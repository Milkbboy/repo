using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class LanPanel : UIPanel
    {
        public UIPanel main_panel;
        public UIPanel join_panel;
        public InputField host_input;

        private static LanPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public void OnClickHost()
        {
            MainMenu.Get().CreateGame(GameType.MultiHost);
        }

        public void OnClickHostLoad()
        {
            LoadPanel.Get().ShowMultiplayer();
            //MainMenu.Get().LoadGame(GameType.MultiHost, World.GetLastSaveMulti());
        }

        public void OnClickJoin()
        {
            main_panel.Hide();
            join_panel.Show();
        }

        public void OnClickConnect()
        {
            string ip = host_input.text;
            if (string.IsNullOrWhiteSpace(ip))
                return;

            MainMenu.Get().JoinGame(GameType.MultiJoin, "", ip);
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            main_panel.Show();
            join_panel.Hide();
        }

        public static LanPanel Get()
        {
            return instance;
        }
    }
}
