using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using UnityEngine.Events;

namespace RogueEngine.UI
{

    public class ChampionUI : MonoBehaviour
    {
        public Image portrait;
        public Image highlight;
        public ProgressBar hp_bar;
        public Button level_up_btn;

        public UnityAction<ChampionUI> onClick;
        public UnityAction<ChampionUI> onClickLvlUp;

        private Champion champion;

        void Start()
        {
            Button btn = GetComponent<Button>();
            btn?.onClick.AddListener(OnClick);

            if (level_up_btn != null)
                level_up_btn.gameObject.SetActive(false);

            if (highlight != null)
                highlight.enabled = false;
        }

        void Update()
        {
            if (champion == null)
                return;

            hp_bar.value = champion.GetHP();
            hp_bar.value_max = champion.GetHPMax();
        }

        public void SetChampion(Champion champion)
        {
            this.champion = champion;
            portrait.sprite = champion.ChampionData.art_portrait;
            portrait.enabled = portrait.sprite != null;
            gameObject.SetActive(true);

            if (highlight != null)
                highlight.enabled = false;
        }

        public void SetLevelUp(bool visible)
        {
            if (level_up_btn != null)
                level_up_btn.gameObject.SetActive(visible && IsSelf());
        }

        public void Hide()
        {
            this.champion = null;
            gameObject.SetActive(false);
        }

        public void SetHighlight(bool active)
        {
            if (highlight != null)
                highlight.enabled = active;
        }

        public void OnClick()
        {
            if (champion != null)
            {
                onClick?.Invoke(this);
            }
        }

        public void OnClickLevelUp()
        {
            if (champion != null)
            {
                onClickLvlUp?.Invoke(this);
            }
        }

        public Champion GetChampion()
        {
            return champion;
        }

        public bool IsSelf()
        {
            return champion != null && champion.player_id == GameClient.Get().GetPlayerID();
        }

    }
}
