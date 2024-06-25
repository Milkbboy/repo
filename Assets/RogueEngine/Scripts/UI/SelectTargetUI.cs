using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using RogueEngine;

namespace RogueEngine.UI
{
    /// <summary>
    /// Box that appears when using the SelectTarget ability target
    /// </summary>

    public class SelectTargetUI : UIPanel
    {
        public Text title;
        public Text desc;

        private static SelectTargetUI _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            Hide(true);
        }

        protected override void Update()
        {
            base.Update();

            Battle game = GameClient.Get().GetBattle();
            if (game != null && game.selector == SelectorType.None)
                Hide();
        }

        public void ShowMsg(string title, string desc)
        {
            this.title.text = title;
            //this.desc.text = desc;
        }

        public void OnClickClose()
        {
            GameClient.Get().CancelSelection();
        }

        public static SelectTargetUI Get()
        {
            return _instance;
        }
    }
}