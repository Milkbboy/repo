using System.Collections;
using System.Collections.Generic;
using RogueEngine.Client;
using UnityEngine;

namespace RogueEngine.UI
{
    public class PowerBar : MonoBehaviour
    {
        public BoxUI[] lines;

        void Start()
        {
            foreach(BoxUI slot in lines)
                slot.Hide();
        }

        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter character = battle.GetActiveCharacter();

            int index = 0;
            if (character != null)
            {
                foreach (Card card in character.cards_power)
                {
                    if (card != null && index < lines.Length)
                    {
                        lines[index].SetCard(card);
                        lines[index].Show();
                        index++;
                    }
                }
            }

            while (index < lines.Length)
            {
                lines[index].Hide();
                index++;
            }
        }
    }
}
