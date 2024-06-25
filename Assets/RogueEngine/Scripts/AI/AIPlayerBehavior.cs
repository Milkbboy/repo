using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Client;
using RogueEngine.Gameplay;

namespace RogueEngine.AI
{
    /// <summary>
    /// AI player making decisions based on a coded behavior BerhaviorData
    /// </summary>
    
    public class AIPlayerBehavior : AIPlayer
    {
        private bool is_playing = false;
        private bool is_selecting = false;
        private int actions = 0;

        private System.Random rand = new System.Random();

        public AIPlayerBehavior(BattleLogic gameplay, int id)
        {
            this.gameplay = gameplay;
            player_id = id;
        }

        public override void Update()
        {
            Battle game_data = gameplay.GetBattleData();
            if (game_data != null && !game_data.IsPlayerTurn(player_id))
            {
                actions = 0;
            }

            if (!CanPlay())
                return;

            BattleCharacter character = game_data.GetActiveCharacter();
            if (character.CharacterData.behavior == null)
                return;

            int action_per_turn = character.CharacterData.behavior.GetActionsPerTurn(game_data, character, character.turn);

            if (character != null && character.player_id == player_id && game_data.IsCharacterTurn(character) && !gameplay.IsResolving())
            {
                if(!is_playing && game_data.selector == SelectorType.None && actions < action_per_turn)
                {
                    is_playing = true;
                    actions++;
                    TimeTool.StartCoroutine(AiTurn(character));
                }

                if (!is_selecting && game_data.selector != SelectorType.None && game_data.selector_player_id == player_id)
                {
                    if (game_data.selector == SelectorType.SelectTarget)
                    {
                        //AI select target
                        is_selecting = true;
                        TimeTool.StartCoroutine(AiSelectTarget());
                    }

                    if (game_data.selector == SelectorType.SelectorCard)
                    {
                        //AI select target
                        is_selecting = true;
                        TimeTool.StartCoroutine(AiSelectCard());
                    }

                    if (game_data.selector == SelectorType.SelectorChoice)
                    {
                        //AI select target
                        is_selecting = true;
                        TimeTool.StartCoroutine(AiSelectChoice());
                    }
                }

                if (!is_playing && !is_selecting && actions >= action_per_turn)
                {
                    EndTurn();
                }
            }
        }

        private IEnumerator AiTurn(BattleCharacter character)
        {
            yield return new WaitForSeconds(0.5f);

            PlayCard();
            yield return new WaitForSeconds(0.4f);
            yield return new WaitUntil(() => { return !IsResolving(); });

            is_playing = false;
        }

        private IEnumerator AiSelectCard()
        {
            yield return new WaitForSeconds(0.5f);

            SelectCard();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            is_selecting = false;
        }

        private IEnumerator AiSelectTarget()
        {
            yield return new WaitForSeconds(0.5f);

            SelectTarget();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            is_selecting = false;
        }

        private IEnumerator AiSelectChoice()
        {
            yield return new WaitForSeconds(0.5f);

            SelectChoice();

            yield return new WaitForSeconds(0.5f);

            CancelSelect();
            is_selecting = false;
        }

        //----------

        public void PlayCard()
        {
            if (!CanPlay())
                return;

            Battle game_data = gameplay.GetBattleData();
            BattleCharacter character = game_data.GetActiveCharacter();
            BehaviorData behavior = character.CharacterData.behavior;
            if(character == null || behavior == null || character.cards_hand.Count == 0)
                return;

            List<Card> valid_cards = game_data.GetValidPlayCards(character);
            Card card = behavior.SelectPlayCard(game_data, character, valid_cards, character.turn);
            if (card != null)
            {
                if (card.CardData.IsRequireTarget())
                {
                    ListSwap<BattleCharacter> character_swap = game_data.GetCharacterListSwap();
                    List<BattleCharacter> valid_characters = character_swap.Get();
                    foreach (BattleCharacter tcharacter in game_data.characters)
                    {
                        if (game_data.CanPlayCard(card, tcharacter.slot))
                        {
                            valid_characters.Add(tcharacter);
                        }
                    }

                    BattleCharacter target = behavior.SelectCharacterTarget(game_data, character, card, valid_characters, character.turn);
                    if (target != null)
                    {
                        gameplay.PlayCard(card, target.slot);
                    }
                }
                else if(game_data.CanPlayCard(card, Slot.None))
                {
                    gameplay.PlayCard(card, Slot.None);
                }
            }
        }

        public void SelectTarget()
        {
            if (!CanPlay())
                return;

            Battle game_data = gameplay.GetBattleData();

            BattleCharacter character = game_data.GetActiveCharacter();
            BehaviorData behavior = character.CharacterData.behavior;
            Card card = game_data.GetCard(game_data.selector_card_uid);
            AbilityData ability = AbilityData.Get(game_data.selector_ability_id);
            if (ability == null || character == null || card == null || behavior == null)
                return;

            ListSwap<BattleCharacter> character_swap = game_data.GetCharacterListSwap();
            List<BattleCharacter> valid_characters = character_swap.Get();
            foreach (BattleCharacter tcharacter in game_data.characters)
            {
                if (ability.CanTarget(game_data, character, card, tcharacter))
                {
                    valid_characters.Add(tcharacter);
                }
            }

            ListSwap<Slot> slot_swap = game_data.GetSlotListSwap();
            List<Slot> valid_slots = slot_swap.Get();
            foreach (Slot tslot in Slot.GetAll())
            {
                if (ability.CanTarget(game_data, character, card, tslot))
                {
                    valid_slots.Add(tslot);
                }
            }

            if (valid_characters.Count > 0)
            {
                BattleCharacter tcharacter = behavior.SelectCharacterTarget(game_data, character, card, valid_characters, character.turn);
                if (tcharacter != null)
                {
                    gameplay.SelectCharacter(tcharacter);
                }
            }
            else if (valid_slots.Count > 0)
            {
                Slot tslot = behavior.SelectSlotTarget(game_data, character, card, valid_slots, character.turn);
                if (tslot != Slot.None)
                {
                    gameplay.SelectSlot(tslot);
                }
            }
        }

        public void SelectCard()
        {
            if (!CanPlay())
                return;

            Battle game_data = gameplay.GetBattleData();
            BattleCharacter character = game_data.GetActiveCharacter();
            BehaviorData behavior = character.CharacterData.behavior;
            Card card = game_data.GetCard(game_data.selector_card_uid);
            AbilityData ability = AbilityData.Get(game_data.selector_ability_id);
            if (ability == null || character == null || card == null || behavior == null)
                return;

            List<Card> valid_cards = new List<Card>();
            foreach (BattleCharacter tcharacter in game_data.characters)
            {
                ability.AddValidCards(game_data, character, card, tcharacter.cards_hand, valid_cards);
                ability.AddValidCards(game_data, character, card, tcharacter.cards_item, valid_cards);
                ability.AddValidCards(game_data, character, card, tcharacter.cards_deck, valid_cards);
                ability.AddValidCards(game_data, character, card, tcharacter.cards_discard, valid_cards);
            }

            if (valid_cards.Count > 0)
            {
                Card tcard = behavior.SelectCardTarget(game_data, character, card, valid_cards, character.turn);
                if (tcard != null)
                {
                    gameplay.SelectCard(tcard);
                }
            }
        }

        public void SelectChoice()
        {
            if (!CanPlay())
                return;

            Battle game_data = gameplay.GetBattleData();
            if (game_data.selector != SelectorType.None)
            {
                AbilityData ability = AbilityData.Get(game_data.selector_ability_id);
                if (ability != null && ability.chain_abilities.Length > 0)
                {
                    int choice = rand.Next(0, ability.chain_abilities.Length);
                    gameplay.SelectChoice(choice);
                }
            }
        }

        public void CancelSelect()
        {
            if (CanPlay())
            {
                gameplay.CancelSelection();
            }
        }

        public void EndTurn()
        {
            if (CanPlay())
            {
                actions = 0;
                gameplay.EndTurn();
            }
        }
    }

}