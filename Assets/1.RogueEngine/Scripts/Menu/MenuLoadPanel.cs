using System.Collections;
using System.Collections.Generic;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class MenuLoadPanel : UIPanel
    {
        private static MenuLoadPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public void OnClickQuit()
        {
            GameClient.Get()?.Disconnect();
            LobbyClient.Get()?.Disconnect();
            Hide();
        }

        public static MenuLoadPanel Get()
        {
            return instance;
        }
    }
}