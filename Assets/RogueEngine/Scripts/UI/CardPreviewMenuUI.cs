using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using RogueEngine;

namespace RogueEngine.UI
{
    /// <summary>
    /// In the game scene, the CardPreviewMenuUI is what shows the card in big with extra info when hovering a card
    /// </summary>

    public class CardPreviewMenuUI : MonoBehaviour
    {
        public UIPanel ui_panel;
        public CardUI card_ui;
        public Text desc;
        public float hover_delay = 0.7f;
        public float hover_delay_mobile = 0.1f;

        private float preview_timer = 0f;

        private void Start()
        {
            
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            CardUIHover hover_ui = CardUIHover.GetFocus();

            float delay = hover_delay;
            if (GameTool.IsMobile())
                delay = hover_delay_mobile;

            CardData icard = hover_ui?.GetCardData();
            bool hover_only = !Input.GetMouseButton(0);
            bool should_show_preview = hover_only && icard != null;

            if (should_show_preview)
                preview_timer += Time.deltaTime;
            else
                preview_timer = 0f;

            bool show_preview = should_show_preview && preview_timer >= delay;
            ui_panel.SetVisible(show_preview);

            if (show_preview)
            {
                card_ui.SetCard(icard, 1);

                //string cdesc = icard.GetDesc();
                string adesc = icard.GetTips();
                //if (!string.IsNullOrWhiteSpace(cdesc))
                //    this.desc.text = cdesc + "\n\n" + adesc;
                //else
                    this.desc.text = adesc;

            }

        }
    }
}
