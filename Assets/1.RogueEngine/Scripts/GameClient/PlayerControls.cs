using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Client;
using UnityEngine.Events;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    /// <summary>
    /// Script that contain main controls for clicking on cards, attacking, activating abilities
    /// Will send action to GameClient on click release
    /// </summary>

    public class PlayerControls : MonoBehaviour
    {
        private BoardCharacter selected_character = null;

        private static PlayerControls instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            if (Input.GetMouseButtonDown(1))
                MouseRightClick();

            if (selected_character != null)
            {
                if (Input.GetMouseButtonUp(0))
                    ReleaseClick();
            }
        }

        public void SelectCharacter(BoardCharacter bcard)
        {
            Battle gdata = GameClient.Get().GetBattle();
            if (gdata == null)
                return;

            Player player = GameClient.Get().GetPlayer();
            BattleCharacter character = bcard.GetCharacter();

            if (gdata.IsPlayerSelectorTurn(player.player_id) && gdata.selector == SelectorType.SelectTarget)
            {
                GameClient.Get().SelectCharacter(character);
            }
            else if (gdata.IsPlayerActionTurn(player.player_id) && character.player_id == player.player_id)
            {
                selected_character = bcard;
            }
        }

        public void MouseRightClick()
        {
            UnselectAll();
        }

        private void ReleaseClick()
        {
            UnselectAll();
        }

        public void UnselectAll()
        {
            selected_character = null;
        }

        public BoardCharacter GetSelected()
        {
            return selected_character;
        }

        public static PlayerControls Get()
        {
            return instance;
        }
    }
}