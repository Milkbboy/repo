using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using RogueEngine;

namespace RogueEngine.UI
{
    /// <summary>
    /// In the game scene, the CardPreviewUI is what shows the card in big with extra info when hovering a card
    /// </summary>

    public class CardPreviewUI : MonoBehaviour
    {
        public UIPanel ui_panel;
        public CardUI card_ui;
        public Text desc;
        public float hover_delay_board = 0.7f;
        public float hover_delay_hand = 0.4f;
        public float hover_delay_mobile = 0.1f;

        public StatusLine[] status_lines;

        private float preview_timer = 0f;

        private void Start()
        {
            
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            foreach (StatusLine line in status_lines)
                line.Hide();

            Battle bdata = GameClient.Get().GetBattle();
            HandCard hcard = HandCard.GetFocus();
            CardUIHover hover_ui = CardUIHover.GetFocus();
            BoxUI hover_slot = BoxUI.GetHoverCard();
            //Card histcard = TurnHistoryLine.GetHoverCard();


            float delay = hcard != null ? hover_delay_hand : hover_delay_board;
            if (GameTool.IsMobile())
                delay = hover_delay_mobile;

            Card pcard = hcard?.GetCard();
            if (pcard == null && hover_ui != null)
                pcard = hover_ui.GetCard();
            if (pcard == null && hover_slot != null)
                pcard = hover_slot.GetCard();

            bool hover_only = !Input.GetMouseButton(0) && !HandCardArea.Get().IsDragging();
            bool should_show_preview = hover_only && !BattleUI.IsUIOpened() && pcard != null;

            if (should_show_preview)
                preview_timer += Time.deltaTime;
            else
                preview_timer = 0f;

            bool show_preview = should_show_preview && preview_timer >= delay;
            ui_panel.SetVisible(show_preview);

            if (show_preview)
            {
                BattleCharacter character = bdata.GetCharacter(pcard.owner_uid);
                CardData icard = pcard.CardData;

                if (character != null)
                    card_ui.SetCard(character, pcard);
                else
                    card_ui.SetCard(icard, pcard.level);

                //string cdesc = icard.GetDesc();
                string adesc = icard.GetTips();
                //if (!string.IsNullOrWhiteSpace(cdesc))
                //    this.desc.text = cdesc + "\n\n" + adesc;
                //else
                    this.desc.text = adesc;

                //Abilities
                int index = 0;
                foreach (AbilityData ability in pcard.GetAbilities())
                {
                    if (index < status_lines.Length)
                    {
                        //Dont display default ability (GetAbilitiesDesc does that already)
                        if (!pcard.CardData.HasAbility(ability) && !string.IsNullOrWhiteSpace(ability.desc))
                        {
                            status_lines[index].SetLine(pcard, ability);
                            index++;
                        }
                    }
                }

                //Status
                foreach (CardStatus status in pcard.GetAllStatus())
                {
                    if (index < status_lines.Length)
                    {
                        StatusData istatus = StatusData.Get(status.id);
                        if (istatus != null && !string.IsNullOrWhiteSpace(istatus.desc))
                        {
                            status_lines[index].SetLine(istatus, status.value);
                            index++;
                        }
                    }
                }
            }

        }
    }
}
