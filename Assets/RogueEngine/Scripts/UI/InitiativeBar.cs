using System.Collections;
using System.Collections.Generic;
using RogueEngine.Client;
using UnityEngine;

namespace RogueEngine.UI
{
    /// <summary>
    /// History bar shows all the previous moved perform by a player this turn
    /// </summary>

    public class InitiativeBar : MonoBehaviour
    {
        public InitiativeLine[] lines;
        public RectTransform turn_line;

        private float half;

        void Start()
        {
            half = (lines[2].RectTransform.anchoredPosition.y - lines[1].RectTransform.anchoredPosition.y) / 2;
        }

        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            Battle battle = GameClient.Get().GetBattle();
            int index = 0;
            foreach (string character_uid in battle.initiatives)
            {
                BattleCharacter character = battle.GetCharacter(character_uid);
                if (character != null && index < lines.Length)
                {
                    lines[index].SetLine(character);
                    index++;
                }
            }

            InitiativeLine last_turn = index < lines.Length ? lines[index] : lines[lines.Length - 1];
            Vector3 line_pos = new Vector3(turn_line.anchoredPosition.x, last_turn.RectTransform.anchoredPosition.y - half, 0f);
            turn_line.anchoredPosition = line_pos;

            foreach (string character_uid in battle.initiatives_next)
            {
                BattleCharacter character = battle.GetCharacter(character_uid);
                if (character != null && index < lines.Length)
                {
                    lines[index].SetLine(character);
                    index++;
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
