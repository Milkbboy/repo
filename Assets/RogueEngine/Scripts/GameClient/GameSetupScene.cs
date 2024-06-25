using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.UI;
using RogueEngine.Client;

namespace RogueEngine
{

    public class GameSetupScene : MonoBehaviour
    {
        private float connect_timer = 0f;

        void Awake()
        {

        }

        private void Start()
        {
            GameClient.Get().onConnectedGame += OnConnectedGame;
            GameClient.Get().Connect();

            BlackPanel.Get().Show(true);
            BlackPanel.Get().Hide(1f);
        }

        private void OnDestroy()
        {
            if (GameClient.Get() != null)
                GameClient.Get().onConnectedGame -= OnConnectedGame;
        }

        private void OnConnectedGame()
        {
            if (!GameClient.Get().IsGameCreated() && GameClient.connect_settings.IsFileHost())
            {
                //New Game (default to test scenario, can be changed)
                ScenarioData scenario = GameplayData.Get().GetDefaultScenario(GameClient.connect_settings.IsOnline());
                GameClient.Get().CreateGame(scenario);
            }
        }

        void Update()
        {
            SwitchScene();
        }

        private void SwitchScene()
        {
            //Disconnected
            if (!GameClient.Get().IsReady())
                connect_timer += Time.deltaTime;

            if (connect_timer > 15f)
                SceneNav.GoToMenu(); //Cant connect

            //Map
            World world = GameClient.Get().GetWorld();
            if (world != null && world.HasStarted())
            {
                MapData map = MapData.Get(world.map_id);
                FadeToScene(map.map_scene);
            }
        }

        public void FadeToScene(string scene)
        {
            StartCoroutine(RunFade(scene));
        }

        public IEnumerator RunFade(string scene)
        {
            yield return new WaitForSeconds(0.5f);
            BlackPanel.Get().Show();
            yield return new WaitForSeconds(1f);
            SceneNav.GoTo(scene);
        }
    }
}
