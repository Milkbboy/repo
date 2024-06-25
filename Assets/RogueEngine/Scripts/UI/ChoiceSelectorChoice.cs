using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    /// <summary>
    /// One choice in the choice selector
    /// Its a button you can click
    /// </summary>

    public class ChoiceSelectorChoice : MonoBehaviour
    {
        public Text title;
        public Text subtitle;
        public Image highlight;

        [HideInInspector]
        public int choice;

        public UnityAction<int> onClick;

        private Button button;
        private bool focus = false;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Update()
        {
            if (highlight != null)
                highlight.enabled = focus;
        }

        public void SetChoice(int index, AbilityData choice)
        {
            this.choice = index;
            this.title.text = choice.title;
            this.subtitle.text = choice.desc;
            button.interactable = true;
            gameObject.SetActive(true);

            //if (choice.mana_cost > 0)
            //    this.title.text += " (" + choice.mana_cost + ")";
        }

        public void SetInteractable(bool interact)
        {
            button.interactable = interact;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnClick()
        {
            onClick?.Invoke(choice);
        }

        public void MouseEnter()
        {
            if (button.interactable)
                focus = true;
        }

        public void MouseExit()
        {
            focus = false;
        }
    }
}
