using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RogueEngine.UI
{

    public class MapPanel : UIPanel
    {

        private static List<MapPanel> map_panels = new List<MapPanel>();

        protected override void Awake()
        {
            base.Awake();
            map_panels.Add(this);
        }

        protected virtual void OnDestroy()
        {
            map_panels.Remove(this);
        }

        public virtual void RefreshPanel()
        {

        }

        public virtual bool ShouldShow()
        {
            return false;
        }

        public virtual bool ShouldRefresh()
        {
            return false;
        }

        public virtual bool IsAutomatic()
        {
            return false;
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static List<MapPanel> GetAll()
        {
            return map_panels;
        }
    }
}
