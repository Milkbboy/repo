using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    /// <summary>
    /// Loading panel that appears at the begining of a match, waiting for players to connect
    /// </summary>

    public class LoadingPanel : UIPanel
    {
        public Text load_txt;

        private static LoadingPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            if (load_txt != null)
                load_txt.text = "";
        }

        protected override void Start()
        {
            base.Start();

            SetLoadText("Connecting to server...");
        }

        protected override void Update()
        {
            base.Update();

            if(!GameClient.Get().IsConnected())
                SetLoadText("Connecting to server...");
            else if(!GameClient.Get().IsReady())
                SetLoadText("Connecting to game...");
            else
                SetLoadText("");
        }

        private void SetLoadText(string text)
        {
            if (IsOnline())
            {
                if (load_txt != null)
                    load_txt.text = text;
            }
        }

        public bool IsOnline()
        {
            return GameClient.connect_settings.IsOnline();
        }

        public void OnClickQuit()
        {
            GameClient.Get().Disconnect();
            SceneNav.GoToMenu();
        }

        public static LoadingPanel Get()
        {
            return instance;
        }
    }
}
