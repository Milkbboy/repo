using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    public class LoadUI : MonoBehaviour
    {
        public Text title;
        public Text scenario_txt;
        public ChampionUI[] champions;

        public UnityAction<LoadUI> onClick;
        public UnityAction<LoadUI> onClickDelete;

        private World world;

        public void SetLine(World game)
        {
            ScenarioData scenario = ScenarioData.Get(game.scenario_id);
            if (scenario != null)
            {
                world = game;
                title.text = game.title;

                MapData map = MapData.Get(game.map_id);
                scenario_txt.text = scenario.title + " - " + map.title + " " + game.GetCurrentLocationDepth();
                gameObject.SetActive(true);

                foreach (ChampionUI ui in champions)
                    ui.Hide();

                int index = 0;
                foreach (Champion champion in game.champions)
                {
                    if (index < champions.Length)
                    {
                        champions[index].SetChampion(champion);
                        index++;
                    }
                }
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public World GetWorld()
        {
            return world;
        }

        public void OnClick()
        {
            onClick?.Invoke(this);
        }

        public void OnClickDelete()
        {
            onClickDelete?.Invoke(this);
        }
    }
}