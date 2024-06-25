using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class GameMenuPanel : UIPanel
    {

        private static GameMenuPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Update()
        {
            base.Update();

        }

        private void RefreshPanel()
        {

        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static GameMenuPanel Get()
        {
            return instance;
        }
    }
}