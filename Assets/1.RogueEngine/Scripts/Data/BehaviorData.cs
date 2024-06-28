using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RogueEngine
{
    /// <summary>
    /// Defines all enemy behaviors 
    /// character is the active character currently making decisions
    /// turn is the turn number of the character
    /// </summary>

    public class BehaviorData : ScriptableObject
    {

        public virtual Card SelectPlayCard(Battle data, BattleCharacter character, List<Card> cards, int turn)
        {
            return null; //Override this function to select which card to play among cards
        }

        public virtual BattleCharacter SelectCharacterTarget(Battle data, BattleCharacter character, Card card, List<BattleCharacter> characters, int turn)
        {
            return null; //Override this function to select which target to select when a card ability requires selecting a character target
        }

        public virtual Card SelectCardTarget(Battle data, BattleCharacter character, Card card, List<Card> cards, int turn)
        {
            return null; //Override this function to select which target to select when a card ability requires selecting a card target
        }

        public virtual Slot SelectSlotTarget(Battle data, BattleCharacter character, Card card, List<Slot> slots, int turn)
        {
            return Slot.None; //Override this function to select which target to select when a card ability requires selecting a character target
        }

        public virtual int GetActionsPerTurn(Battle data, BattleCharacter character, int turn)
        {
            return 1; //Override to select the number of actions per turn, each action increments the index
        }

        public virtual string GetBehaviorText()
        {
            return ""; //Returns a quick description of this behavior
        }
        

        // --- Generic functions to use in children class ----

        protected virtual BattleCharacter GetRandomCharacter(Battle data, BattleCharacter active_character, List<BattleCharacter> characters, int turn)
        {
            ListSwap<BattleCharacter> list_swap = data.GetCharacterListSwap();
            List<BattleCharacter> list = GameTool.PickXRandom(characters, list_swap.GetOther(characters), 1, active_character.Hash + turn);
            if (list.Count > 0)
                return list[0];
            return null;
        }

        protected virtual Card GetRandomCard(Battle data, BattleCharacter active_character, List<Card> cards, int turn)
        {
            ListSwap<Card> list_swap = data.GetCardListSwap();
            List<Card> list = GameTool.PickXRandom(cards, list_swap.GetOther(cards), 1, active_character.Hash + turn);
            if (list.Count > 0)
                return list[0];
            return null;
        }

        protected virtual Slot GetRandomSlot(Battle data, BattleCharacter active_character, List<Slot> slots, int turn)
        {
            ListSwap<Slot> list_swap = data.GetSlotListSwap();
            List<Slot> list = GameTool.PickXRandom(slots, list_swap.GetOther(slots), 1, active_character.Hash + turn);
            if (list.Count > 0)
                return list[0];
            return Slot.None;
        }

        protected virtual Card FindCard(List<Card> cards, CardData to_find)
        {
            foreach (Card card in cards)
            {
                if (card.card_id == to_find.id)
                    return card;
            }
            return null;
        }

        protected virtual void RemoveCard(List<Card> cards, CardData to_remove)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].card_id == to_remove.id)
                    cards.RemoveAt(i);
            }
        }
    }
}
