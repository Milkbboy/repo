using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.UI;
using RogueEngine.Client;

namespace RogueEngine
{

    public class MapScene : MonoBehaviour
    {
        private bool ended = false;

        void Awake()
        {

        }

        private void Start()
        {
            GameClient.Get().onConnectedGame += OnConnectGame;
            GameClient.Get().Connect();

            if (GameClient.Get().IsReady())
            {
                World world = GameClient.Get().GetWorld();
                world.Save(); //Auto save each time go back to map scene
            }
        }

        private void OnDestroy()
        {
            GameClient.Get().onConnectedGame -= OnConnectGame;
        }

        private void OnConnectGame()
        {
            if (!GameClient.Get().IsGameStarted())
            {
                //Start in test mode, scene loaded directly
                GameClient.Get().NewScenario(GameplayData.Get().test_scenario, "test");
                GameClient.Get().CreateChampion(GameplayData.Get().test_champion, 1);
                GameClient.Get().StartTest(WorldState.Map);
            }
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            SwitchScene();
        }

        private void SwitchScene()
        {
            //Battle
            World world = GameClient.Get().GetWorld();
            if (world.state == WorldState.Battle)
            {
                MapData map = MapData.Get(world.map_id);
                EventBattle battle = EventBattle.Get(world.event_id);
                string scene = !string.IsNullOrEmpty(battle.scene) ? battle.scene : map.battle_scene;
                FadeToScene(scene);
            }

            //Next Map, reload map
            MapViewer viewer = MapViewer.Get();
            if (world.state == WorldState.Map && viewer.HasChanged())
            {
                MapData map = MapData.Get(world.map_id);
                FadeToScene(map.map_scene);
            }

            //End Game
            if (!ended && world.state == WorldState.Ended)
            {
                ended = true;
                EndGame();
            }
        }

        private void EndGame()
        {
            //Unlock rewards and show score panel
            ProgressManager.Get().UnlockNewRewards(2, 1);
            GameOverPanel.Get().Show();

            //Delete save file
            World world = GameClient.Get().GetWorld();
            World.Delete(world.filename);
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
