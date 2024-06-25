using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "behavior", menuName = "TcgEngine/Behaviors/Random", order = 20)]
    public class BehaviorRandom : BehaviorData
    {
        public override Card SelectPlayCard(Battle data, BattleCharacter character, List<Card> cards, int turn)
        {
            return GetRandomCard(data, character, cards, turn);
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
            return "Attacks a random character with a random card";
        }
    }
}
