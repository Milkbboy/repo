using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class EventChoicePanel : MapPanel
    {
        public Text text;
        public EventChoiceLine[] lines;

        private bool skip_choice;

        private static EventChoicePanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (EventChoiceLine line in lines)
                line.onClick += OnClick;
        }

        public void ShowText(string text)
        {
            skip_choice = true;
            this.text.text = text;

            foreach (EventChoiceLine line in lines)
                line.Hide();

            lines[0].SetText(0, "OK");
        }

        public void ShowChoices(EventChoice evt)
        {
            skip_choice = false;
            text.text = evt.GetText();

            foreach (EventChoiceLine line in lines)
                line.Hide();

            World world = GameClient.Get().GetWorld();
            Champion champ = GameClient.Get().GetChampion();

            int index = 0;
            foreach (ChoiceElement choice in evt.choices)
            {
                if (index < lines.Length && choice.effect.AreEventsConditionMet(world, champ))
                {
                    lines[index].SetLine(index, choice);
                    index++;
                }
            }
        }

        public override void RefreshPanel()
        {
            World world = GameClient.Get().GetWorld();
            if (world.state == WorldState.EventChoice)
            {
                EventChoice choice = EventChoice.Get(world.event_id);
                ShowChoices(choice);
            }
            else if (world.state == WorldState.EventText)
            {
                ShowText(world.event_text);
            }
        }

        public override bool ShouldShow()
        {
            World world = GameClient.Get().GetWorld();
            EventChoice choice = EventChoice.Get(world.event_id);
            return (world.state == WorldState.EventChoice && choice != null) || world.state == WorldState.EventText;
        }

        public override bool ShouldRefresh()
        {
            return false;
        }

        public override bool IsAutomatic()
        {
            return true;
        }

        private void OnClick(EventChoiceLine line)
        {
            Champion champ = GameClient.Get().GetChampion();
            if (champ != null)
            {
                if(skip_choice)
                    GameClient.Get().MapEventContinue();
                else
                    GameClient.Get().MapSelectChoice(champ, line.GetEvent());
            }
        }

        public static EventChoicePanel Get()
        {
            return instance;
        }
    }
}
