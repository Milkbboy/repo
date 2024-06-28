using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueEngine.UI
{

    public class EventChoiceLine : MonoBehaviour
    {
        public Text text;
        public Text subtext;
        public Image highlight;

        public UnityAction<EventChoiceLine> onClick;

        private EventData evt;
        private int index;

        public void SetText(int index, string text, string subtext = "")
        {
            this.index = index;
            this.text.text = text;
            this.subtext.text = subtext;
            highlight.enabled = false;
            gameObject.SetActive(true);
        }

        public void SetLine(int index, ChoiceElement choice)
        {
            this.index = index;
            evt = choice.effect;
            text.text = choice.text;
            subtext.text = choice.subtext;
            highlight.enabled = false;
            gameObject.SetActive(true);
        }

        public void SetHighlight(bool active)
        {
            highlight.enabled = active;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnClick()
        {
            onClick?.Invoke(this);
        }

        public EventData GetEvent()
        {
            return evt;
        }

    }
}
