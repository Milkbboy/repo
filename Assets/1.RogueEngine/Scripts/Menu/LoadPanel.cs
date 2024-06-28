using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class LoadPanel : UIPanel
    {
        public LoadUI[] lines;

        private bool multiplayer = false;

        private static LoadPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (LoadUI ui in lines)
            {
                ui.onClick += OnClickLoad;
                ui.onClickDelete += OnClickDelete;
            }
        }

        public virtual void RefreshPanel()
        {
            foreach (LoadUI load in lines)
                load.Hide();

            string user_id = Authenticator.Get().UserID;

            int index = 0;
            List<World> saves = multiplayer ? World.GetAllMultiplayer(user_id) : World.GetAllSolo(user_id);
            foreach (World game in saves)
            {
                if (game != null && index < lines.Length)
                {
                    lines[index].SetLine(game);
                    index++;
                }
            }
        }

        private void OnClickLoad(LoadUI ui)
        {
            World game = ui.GetWorld();
            GameType type = multiplayer ? GameType.MultiHost : GameType.Solo;
            MainMenu.Get().LoadGame(type, game.filename);
        }

        private void OnClickDelete(LoadUI ui)
        {
            World game = ui.GetWorld();
            World.Delete(game.filename);
            RefreshPanel();
        }

        public void ShowSolo()
        {
            multiplayer = false;
            Show();
        }

        public void ShowMultiplayer()
        {
            multiplayer = true;
            Show();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static LoadPanel Get()
        {
            return instance;
        }
    }
}
