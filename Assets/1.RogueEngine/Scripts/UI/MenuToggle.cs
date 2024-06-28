using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace RogueEngine.UI
{

    public class MenuToggle : MonoBehaviour
    {
        public string id;
        public string group;

        [Header("UI")]
        public Image highlight;

        public UnityAction<MenuToggle> onToggle;

        private bool selected = false;

        private static List<MenuToggle> toggle_list = new List<MenuToggle>();

        private void Awake()
        {
            toggle_list.Add(this);
        }

        private void OnDestroy()
        {
            toggle_list.Remove(this);
        }

        void Start()
        {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
        }

        void Update()
        {
            if (highlight != null)
                highlight.enabled = selected;
        }

        public void SetSelected(bool sel)
        {
            selected = sel;
        }

        public bool IsSelected()
        {
            return selected;
        }

        public void OnClick()
        {
            bool was_select = selected;
            UnselectAll(group);
            SetSelected(!was_select);
            onToggle?.Invoke(this);
        }

        public static string GetSelectedID(string group)
        {
            foreach (MenuToggle toggle in GetAll(group))
            {
                if (toggle.IsSelected())
                    return toggle.id;
            }
            return "";
        }

        public static bool IsSelected(string group)
        {
            foreach (MenuToggle toggle in GetAll(group))
            {
                if (toggle.IsSelected())
                    return true;
            }
            return false;
        }

        public static void UnselectAll(string group)
        {
            foreach (MenuToggle toggle in GetAll(group))
            {
                toggle.SetSelected(false);
            }
        }

        public static List<MenuToggle> GetAll(string group)
        {
            List<MenuToggle> valid = new List<MenuToggle>();
            foreach (MenuToggle toggle in GetAll())
            {
                if (toggle.group == group)
                    valid.Add(toggle);
            }
            return valid;

        }

        public static List<MenuToggle> GetAll()
        {
            return toggle_list;
        }
    }
}
