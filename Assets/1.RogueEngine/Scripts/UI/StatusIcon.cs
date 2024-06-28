using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueEngine.UI
{

    public class StatusIcon : MonoBehaviour
    {
        public Image icon_img;
        public Text icon_txt;

        private HoverTargetUI hover_ui;

        void Awake()
        {
            hover_ui = GetComponent<HoverTargetUI>();
        }

        public void SetStatus(StatusData status, int value)
        {
            if (status.icon != null)
            {
                if (icon_img != null)
                    icon_img.sprite = status.icon;
                if (icon_txt != null)
                    icon_txt.text = value.ToString();
                if (icon_txt != null)
                    icon_txt.enabled = value > 0;

                string value_text = (value > 1) ? (" " + value) : ""; 
                if (hover_ui != null)
                    hover_ui.text = "<size=" + (hover_ui.text_size + 4) + ">" + status.title + value_text + "</size>\n" + status.desc;

                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
            }
        }

        public void SetTrait(TraitData trait, int value)
        {
            if (trait.icon != null)
            {
                if (icon_img != null)
                    icon_img.sprite = trait.icon;
                if (icon_txt != null)
                    icon_txt.text = value.ToString();
                if (icon_txt != null)
                    icon_txt.enabled = value > 0;

                string value_text = (value > 1) ? (" " + value) : "";
                if (hover_ui != null)
                    hover_ui.text = "<size=" + (hover_ui.text_size + 4) + ">" + trait.title + value_text + "</size>\n" + trait.desc;

                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if(gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}