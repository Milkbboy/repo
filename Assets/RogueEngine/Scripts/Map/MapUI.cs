using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class MapUI : MonoBehaviour
    {
        public Text gold_txt;

        public ChampionUI[] champions_ui;

        private static MapUI instance;

        void Awake()
        {
            instance = this;
            gold_txt.text = "";

            for (int i = 0; i < champions_ui.Length; i++)
                champions_ui[i].Hide();

            for (int i = 0; i < champions_ui.Length; i++)
            {
                champions_ui[i].onClick += OnClickChampion;
                champions_ui[i].onClickLvlUp += OnClickChampionLvlUp;
            }
        }

        private void Start()
        {
            GameClient.Get().onRefreshWorld += OnRefresh;
        }

        void OnDestroy()
        {
            GameClient.Get().onRefreshWorld -= OnRefresh;
        }

        void Update()
        {
            World world = GameClient.Get().GetWorld();
            if (world == null)
                return;

            LoadingPanel.Get().SetVisible(!GameClient.Get().IsReady());

            Player aplayer = GameClient.Get().GetPlayer();
            int gold = aplayer.gold;
            gold_txt.text = gold.ToString();

            int index = 0;
            foreach (Player player in world.players)
            {
                foreach (Champion champion in world.champions)
                {
                    if (champion.player_id == player.player_id && index < champions_ui.Length)
                    {
                        champions_ui[index].SetChampion(champion);
                        champions_ui[index].SetLevelUp(champion.CanLevelUp());
                        index++;
                    }
                }
            }

            for (int i = index; i < champions_ui.Length; i++)
                champions_ui[i].Hide();

            ShowPanels();
        }

        private void ShowPanels()
        {
            foreach (MapPanel panel in MapPanel.GetAll())
            {
                if (!panel.IsVisible() && panel.ShouldShow() && panel.IsAutomatic())
                {
                    panel.Show();
                }

                if (panel.IsVisible() && !panel.ShouldShow() && panel.IsAutomatic())
                {
                    panel.Hide();
                }

                if (panel.IsVisible() && panel.ShouldShow() && panel.ShouldRefresh())
                {
                    panel.RefreshPanel();
                }
            }
        }

        private void OnRefresh()
        {
            foreach (MapPanel panel in MapPanel.GetAll())
            {
                if(panel.IsVisible() && (!panel.IsAutomatic() || panel.ShouldShow()))
                    panel.RefreshPanel();
            }
        }

        private void OnClickChampion(ChampionUI champion)
        {
            ChampionPanel.Get().ShowChampion(champion.GetChampion());
        }

        private void OnClickChampionLvlUp(ChampionUI ui)
        {
            Champion champion = ui.GetChampion();
            if (champion != null && champion.CanLevelUp())
            {
                GameClient.Get().LevelUp(champion);
            }
        }

        public void OnClickMenu()
        {
            GameMenuPanel.Get().Show();
        }

        public void OnClickQuit()
        {
            GameClient.Get().Disconnect();
            SceneNav.GoToMenu();
        }

        public void OnClickSave()
        {
            World world = GameClient.Get().GetWorld();
            world.Save();
        }

        public bool IsPanelOpen()
        {
            return ChampionPanel.Get().IsVisible() || DeckPanel.Get().IsVisible() || RewardPanel.Get().IsVisible();
        }

        public static MapUI Get()
        {
            return instance;
        }
    }
}
