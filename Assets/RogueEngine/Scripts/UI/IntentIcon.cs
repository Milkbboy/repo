using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueEngine.UI
{

    public class IntentIcon : MonoBehaviour
    {
        public Image intent_img;
        public Text intent_value;
        public HoverTargetUI intent_txt;

        public void SetIntent(IntentData intent, int value)
        {
            intent_img.enabled = true;
            intent_img.sprite = intent.icon;
            intent_value.text = value.ToString();
            intent_value.enabled = intent.show_value;
            intent_txt.text = "<size=" + (intent_txt.text_size + 4) + ">" + intent.GetTitle() + "</size>\n" + intent.GetText();
        }

        public void Hide()
        {
            intent_img.enabled = false;
            intent_value.text = "";
            intent_txt.text = "";
        }
    }
}
