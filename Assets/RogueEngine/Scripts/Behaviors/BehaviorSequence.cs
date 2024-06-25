using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Enemy with this behavior will always play all his cards in order: card 1, card 2, card 3, then back to card 1....
    /// </summary>

    [CreateAssetMenu(fileName = "behavior", menuName = "TcgEngine/Behaviors/Sequence", order = 20)]
    public class BehaviorSequence : BehaviorData
    {
        public override Card SelectPlayCard(Battle data, BattleCharacter character, List<Card> cards, int turn)
        {
            if(cards.Count > 0)
                return cards[0];
            return null;
        }

        public override BattleCharacter SelectCharacterTarget(Battle data, BattleCharacter character, Card card, List<BattleCharacter> characters, int turn)
        {
            return GetRandomCharacter(data, character, characters, turn);
        }

        public override Card SelectCardTarget(Battle data, BattleCharacter character, Card card, List<Card> cards, int turn)
        {
            return GetRandomCard(data, character, cards, turn);
        }

        public override Slot SelectSlotTarget(Battle data, BattleCharacter character, Card card, List<Slot> slots, int turn)
        {
            return GetRandomSlot(data, character, slots, turn);
        }

        public override string GetBehaviorText()
        {
            return "Attacks a random character with the next card";
        }
    }
}
