using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine.AI
{
    /// <summary>
    /// AI player base class, other AI inherit from this
    /// </summary>

    public abstract class AIPlayer 
    {
        public int player_id;
        public int ai_level;

        protected BattleLogic gameplay;

        public virtual void Update()
        {
            //Script called by game server to update AI
            //Override this to let the AI play
        }

        public bool CanPlay()
        {
            Battle gdata = gameplay.GetBattleData();
            bool can_play = gdata.IsPlayerTurn(player_id);
            return can_play && !gameplay.IsResolving();
        }

        public bool IsResolving()
        {
            return gameplay.IsResolving();
        }

        public static AIPlayer Create(AIType type, BattleLogic gameplay, int id, int level = 0)
        {
            if (type == AIType.Behavior)
                return new AIPlayerBehavior(gameplay, id);
            return null;
        }
    }

    public enum AIType
    {
        Behavior = 0,
    }
}
