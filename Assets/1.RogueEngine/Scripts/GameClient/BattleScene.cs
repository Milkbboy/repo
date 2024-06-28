using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    public class BattleScene : MonoBehaviour
    {
        private bool game_ended = false;

        private static BattleScene instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            GameClient.Get().onConnectedGame += OnConnectGame;
            GameClient.Get().Connect();
        }

        private void OnDestroy()
        {
            GameClient.Get().onConnectedGame -= OnConnectGame;
        }

        //Will run only if started from scene directly, to initialize test game
        private void OnConnectGame()
        {
            //Start in test mode
            if (!GameClient.Get().IsGameStarted())
            {
                GameClient.Get().NewScenario(GameplayData.Get().test_scenario, "test");
                GameClient.Get().CreateChampion(GameplayData.Get().test_champion, 1);
                GameClient.Get().StartTest(WorldState.Battle);
            }
        }

        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            //--- End Game ----
            Battle data = GameClient.Get().GetBattle();
            if (!game_ended && data.phase == BattlePhase.Ended)
            {
                game_ended = true;
                EndGame();
            }
        }

        private void EndGame()
        {
            StartCoroutine(EndGameRun());
        }

        private IEnumerator EndGameRun()
        {
            World world = GameClient.Get().GetWorld();
            Battle data = GameClient.Get().GetBattle();
            bool win = false; // Change this

            AudioTool.Get().FadeOutMusic("music");

            yield return new WaitForSeconds(1f);

            if (win && AssetData.Get().win_fx != null)
                Instantiate(AssetData.Get().win_fx, Vector3.zero, Quaternion.identity);
            else if(AssetData.Get().lose_fx != null)
                Instantiate(AssetData.Get().lose_fx, Vector3.zero, Quaternion.identity);

            if (win)
                AudioTool.Get().PlaySFX("ending_sfx", AssetData.Get().win_audio);
            else
                AudioTool.Get().PlaySFX("ending_sfx", AssetData.Get().defeat_audio);

            if (win)
                AudioTool.Get().PlayMusic("music", AssetData.Get().win_music, 0.4f, false);
            else
                AudioTool.Get().PlayMusic("music", AssetData.Get().defeat_music, 0.4f, false);

            yield return new WaitForSeconds(1f);

            BlackPanel.Get().Show();

            yield return new WaitForSeconds(1f);

            SceneNav.GoTo(world);

            //EndGamePanel.Get().ShowWinner(win);
        }

        public static BattleScene Get()
        {
            return instance;
        }
    }

    [System.Serializable]
    public struct BattleBG
    {
        public MapData planet;
        public Sprite bg;
    }
}
