using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class LobbyCreatePanel : UIPanel
    {
        public UIPanel create_panel;
        public UIPanel load_panel;

        public InputField title_field;
        public OptionSelector scenario_field;
        public GameObject scenario_group;

        public LoadUI[] load_lines;

        private static LobbyCreatePanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (LoadUI ui in load_lines)
                ui.onClick += OnClickLoad;
        }

        protected override void Start()
        {
            base.Start();

        }

        private void RefreshPanel()
        {
            title_field.text = Authenticator.Get().Username + "'s game";
        }

        public void RefreshLoad()
        {
            foreach (LoadUI ui in load_lines)
                ui.Hide();

            string user_id = Authenticator.Get().UserID;

            int index = 0;
            List<World> saves = World.GetAllMultiplayer(user_id);
            foreach (World game in saves)
            {
                if (game != null && index < load_lines.Length)
                {
                    load_lines[index].SetLine(game);
                    index++;
                }
            }
        }

        public void OnClickCreate()
        {
            if (title_field.text.Length == 0)
                return; //Title/save invaild

            Hide();

            UserData udata = Authenticator.Get().UserData;
            string file = udata.id + "_lobby_" + title_field.text;

            LobbyPanel.Get().WaitForCreate();
            World.Unload();

            LobbyClient.Get().CreateGame(title_field.text, "", file);
        }

        public void OnClickLoad(LoadUI ui)
        {
            Hide();

            World world = ui.GetWorld();
            
            LobbyPanel.Get().WaitForCreate();
            World.Unload();

            ScenarioData scenario = ScenarioData.Get(world.scenario_id);
            string subtitle = scenario != null ? scenario.title : "";
            LobbyClient.Get().CreateGame(world.title, subtitle, world.filename, true);
        }

        public void OnClickBack()
        {
            LobbyPanel.Get().Show();
            Hide();
        }

        public void ShowCreate()
        {
            load_panel.Hide(true);
            create_panel.Show();
            Show();
        }

        public void ShowLoad()
        {
            create_panel.Hide(true);
            load_panel.Show();
            RefreshLoad();
            Show();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }


        public static LobbyCreatePanel Get()
        {
            return instance;
        }
    }
}