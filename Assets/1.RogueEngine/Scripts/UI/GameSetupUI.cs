using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class GameSetupUI : MonoBehaviour
    {
        public Text scenario_txt;
        public Text player_count;
        public InputField input_seed;
        public GameObject host_group;

        public Text[] players;

        private int scenario_index = 1;
        private string scenario_id = "";

        void Awake()
        {

        }

        private void Start()
        {
            player_count.text = "";
            input_seed.text = GameTool.GenerateRandomID(8).ToUpper();

            foreach (Text player in players)
                player.text = "";
        }

        void Update()
        {
            LoadingPanel.Get().SetVisible(!GameClient.Get().IsReady());
            
            if (!GameClient.Get().IsReady())
                return;

            World world = GameClient.Get().GetWorld();
            ScenarioData scenario = ScenarioData.Get(world.scenario_id);
            if (scenario == null)
                return;

            player_count.text = world.champions.Count + "/" + scenario.champions;
            scenario_txt.text = scenario.title;

            if (host_group.activeSelf != GameClient.connect_settings.file_host)
                host_group.SetActive(GameClient.connect_settings.file_host);

            if (scenario.id != scenario_id)
            {
                scenario_id = scenario.id;
                scenario_index = GetScenarioIndex(scenario_id);
            }

            foreach (Text player in players)
                player.text = "";

            int index = 0;
            foreach (Player player in world.players)
            {
                if (index < players.Length && player.IsConnected())
                {
                    players[index].text = player.username;
                    index++;
                }
            }
        }

        public void OnClickScenarioPrev()
        {
            int nb_scenario = GameplayData.Get().scenarios.Length;
            scenario_index -= 1;
            scenario_index = (scenario_index + nb_scenario) % nb_scenario;

            CreateScenario();
        }

        public void OnClickScenarioNext()
        {
            int nb_scenario = GameplayData.Get().scenarios.Length;
            scenario_index += 1;
            scenario_index = (scenario_index + nb_scenario) % nb_scenario;

            CreateScenario();
        }

        private void CreateScenario()
        {
            ScenarioData scenario = GameplayData.Get().scenarios[scenario_index];
            if (scenario != null && !string.IsNullOrWhiteSpace(input_seed.text))
            {
                GameClient.Get().CreateGame(scenario);
            }
        }

        private int GetScenarioIndex(string id)
        {
            int index = 0;
            foreach (ScenarioData scenario in GameplayData.Get().scenarios)
            {
                if (scenario.id == id)
                    return index;
                index++;
            }
            return -1;
        }

        public void OnClickStart()
        {
            World world = GameClient.Get().GetWorld();
            ScenarioData scenario = ScenarioData.Get(world.scenario_id);
            if (scenario != null && !string.IsNullOrWhiteSpace(input_seed.text) && world.champions.Count > 0 && world.champions.Count <= scenario.champions)
            {
                int seed = input_seed.text.ToUpper().GetHashCode();
                GameClient.Get().StartGame(seed);
            }
        }

    }
}
