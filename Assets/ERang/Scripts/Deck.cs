using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class Deck : MonoBehaviour
    {
        public static Deck Instance { get; private set; }

        public List<GameCard> HandCards => deckSystem.HandCards;
        public List<GameCard> ExtinctionCards => deckSystem.ExtinctionCards;

        private DeckSystem deckSystem;
        private DeckUI deckUI;
        private HandDeck handDeck;
        private ShowAllCard showAllCard;

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            deckSystem = GetComponent<DeckSystem>();
            deckUI = GetComponent<DeckUI>();
            handDeck = GetComponent<HandDeck>();
            showAllCard = GetComponent<ShowAllCard>();
        }

        public void CreateMasterCards(Master master)
        {
            deckSystem.CreateMasterCards(master);

            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
        }

        public IEnumerator MakeHandCards()
        {
            deckSystem.MakeHandCards();

            for (int i = 0; i < deckSystem.HandCards.Count; ++i)
            {
                GameCard card = deckSystem.HandCards[i];

                yield return handDeck.SpawnHandCard(card);
            }

            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
            deckUI.SetGraveCardCount(deckSystem.GraveCardCount);
        }

        public void TrunEndProcess()
        {
            deckSystem.RemoveTurnEndHandCard();

            deckUI.RemoveTurnEndHandCard();
            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
            deckUI.SetGraveCardCount(deckSystem.GraveCardCount);
        }

        public void HandCardToBaord(HCard hCard)
        {
            deckSystem.HandCardToBoard(hCard.Card);

            deckUI.RemoveHandCard(hCard.Card.Uid);
            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
        }

        public void HandCardToDeck(SelectCard selectCard)
        {
            deckSystem.HandCardToDeck(selectCard.Card);

            deckUI.RemoveHandCard(selectCard.Card.Uid);
            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
        }

        public GameCard FindHandCard(string cardUid)
        {
            return deckSystem.FindHandCard(cardUid);
        }

        public void RemoveHandCard(string cardUid)
        {
            deckSystem.RemoveUsedHandCard(cardUid);
            deckUI.RemoveHandCard(cardUid);

            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
            deckUI.SetGraveCardCount(deckSystem.GraveCardCount);
            deckUI.SetExtinctionCardCount(deckSystem.ExtinctionCardCount);
        }

        public void AddHandCard(GameCard card)
        {
            deckSystem.AddHandCard(card);

            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
        }

        public void AddGraveCard(GameCard card)
        {
            deckSystem.AddGraveCard(card);

            deckUI.SetGraveCardCount(deckSystem.GraveCardCount);
        }

        public void AddDeckCard(GameCard card)
        {
            deckSystem.AddDeckCard(card);

            deckUI.SetDeckCardCount(deckSystem.DeckCardCount);
        }

        public void ShowDeckCards()
        {
            showAllCard.ToggleShowCards(deckSystem.DeckCards);
        }

        public void ShowGraveCards()
        {
            showAllCard.ToggleShowCards(deckSystem.GraveCards);
        }
    }
}