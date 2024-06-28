using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{
    /// <summary>
    /// The choice selector is a box that appears when using an ability with ChoiceSelector as target
    /// it let you choose between different abilities
    /// </summary>

    public class ChoiceSelector : SelectorPanel
    {
        public ChoiceSelectorChoice[] choices;

        private BattleCharacter caster;
        private Card card;
        private AbilityData ability;

        private static ChoiceSelector _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            Hide(true);

            foreach (ChoiceSelectorChoice choice in choices)
                choice.Hide();
            foreach (ChoiceSelectorChoice choice in choices)
                choice.onClick += OnClickChoice;
        }

        protected override void Update()
        {
            base.Update();

            Battle game = GameClient.Get().GetBattle();
            if (game != null && game.selector == SelectorType.None)
                Hide();
        }

        private void RefreshSelector()
        {
            foreach (ChoiceSelectorChoice choice in choices)
                choice.Hide();

            Battle gdata = GameClient.Get().GetBattle();
            Player player = GameClient.Get().GetPlayer();
            BattleCharacter character = gdata.GetActiveCharacter();

            if (ability != null)
            {
                int index = 0;
                foreach (AbilityData choice in ability.chain_abilities)
                {
                    if (choice != null && index < choices.Length)
                    {
                        ChoiceSelectorChoice achoice = choices[index];
                        achoice.SetChoice(index, choice);
                        achoice.SetInteractable(gdata.CanSelectAbility(character, card, choice));
                    }
                    index++;
                }
            }
        }

        public void OnClickChoice(int index)
        {
            Battle data = GameClient.Get().GetBattle();
            if (data.selector == SelectorType.SelectorChoice)
            {
                GameClient.Get().SelectChoice(index);
                Hide();
            }
            else
            {
                Hide();
            }
        }

        public void OnClickCancel()
        {
            GameClient.Get().CancelSelection();
            Hide();
        }

        public override void Show(AbilityData iability, BattleCharacter caster, Card card)
        {
            this.caster = caster;
            this.card = card;
            this.ability = iability;
            Show();
            RefreshSelector();
        }

        public override bool ShouldShow()
        {
            Battle data = GameClient.Get().GetBattle();
            int player_id = GameClient.Get().GetPlayerID();
            return data.selector == SelectorType.SelectorChoice && data.selector_player_id == player_id;
        }

        public static ChoiceSelector Get()
        {
            return _instance;
        }
    }
}
