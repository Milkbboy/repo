using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueEngine.UI
{

    public class LevelTab : MonoBehaviour
    {
        public int level;
        public Color select_color;
        public Color normal_color;

        public UnityAction<int> onClick;

        private Text text;
        private bool is_selected = false;

        void Awake()
        {
            text = GetComponentInChildren<Text>();

            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
        }

        public void SetSelected(bool selected)
        {
            is_selected = selected;
            text.color = selected ? select_color : normal_color;
        }

        public bool IsSelected()
        {
            return is_selected;
        }

        public void OnClick()
        {
            onClick?.Invoke(level);
        }
    }
}
