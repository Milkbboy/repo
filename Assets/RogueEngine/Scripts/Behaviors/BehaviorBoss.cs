using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "behavior", menuName = "TcgEngine/Behaviors/Boss", order = 20)]
    public class BehaviorBoss : BehaviorData
    {
        public CardData summon_card;
        public CardData ultimate_card;
        public TraitData phase_trait;

        public override Card SelectPlayCard(Battle data, BattleCharacter character, List<Card> cards, int turn)
        {
            //If no snakes, summon snakes
            if (data.CountAliveTeam(true) <= 1)
            {
                return FindCard(cards, summon_card);
            }

            //If half HP and has not played ultimate yet, play ultimate
            if (character.GetTraitValue(phase_trait) == 0 && character.GetHP() <= (character.GetHPMax() / 2))
            {
                return FindCard(cards, ultimate_card);
            }

            //Otherwise play random card, except summon and ultimate
            RemoveCard(cards, summon_card);
            RemoveCard(cards, ultimate_card);
            return GetRandomCard(data, character, cards, turn);
        }

        public override int GetActionsPerTurn(Battle data, BattleCharacter character, int turn)
        {
            //If half hp, play 2 cards per turn
            if (character.GetHP() <= (character.GetHPMax() / 2))
            {
                return 2;
            }
            return 1;
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
            return "Dragon Boss";
        }

    }
}
