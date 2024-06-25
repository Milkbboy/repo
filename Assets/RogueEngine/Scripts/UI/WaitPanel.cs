using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    public class WaitPanel : MapPanel
    {
        private static WaitPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        private void OnClickSkip()
        {
            
        }

        public override bool ShouldShow()
        {
            World world = GameClient.Get().GetWorld();
            int player_id = GameClient.Get().GetPlayerID();
            bool is_state = world.state != WorldState.Setup && world.state != WorldState.Map && world.state != WorldState.Battle;
            return is_state && world.AreAllActionsCompleted(player_id) && !world.AreAllActionsCompleted();
        }

        public override bool IsAutomatic()
        {
            return true;
        }

        public static WaitPanel Get()
        {
            return instance;
        }
    }
}
