using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ERang
{
    public class Actions : MonoBehaviour
    {
        // HandDeck => BoardSlot 으로 이동
        public static UnityAction<BoardSlot, string> OnBoardSlotEquipCard;
        public static UnityAction<string> OnHandCardUsed;
        public static UnityAction OnDeckCountChange;
        public static UnityAction OnGraveDeckCountChange;

        void OnEnable()
        {
            OnBoardSlotEquipCard += BoardSlotEquipCard;
            OnHandCardUsed += HandCardUsed;
            OnDeckCountChange += DeckCountChanged;
            OnGraveDeckCountChange += GraveDeckCountChange;
        }

        void OnDisable()
        {
            OnBoardSlotEquipCard -= BoardSlotEquipCard;
            OnHandCardUsed -= HandCardUsed;
            OnDeckCountChange -= DeckCountChanged;
            OnGraveDeckCountChange -= GraveDeckCountChange;
        }

        public void BoardSlotEquipCard(BoardSlot boardSlotRef, string cardUid)
        {
            Card card = Master.Instance.GetHandCard(cardUid);

            // BoardSlot 에 카드 장착
            boardSlotRef.EquipCard(card);

            // HandDeck 에서 카드 제거
            HandDeck.Instance.RemoveCard(cardUid);

            // Master 의 handCards => boardCreatureCards or boardBuildingCards 로 이동
            Master.Instance.HandCardToBoard(cardUid, card.type);
        }

        public void HandCardUsed(string cardUid)
        {
            Debug.Log($"HandCardUsed: {cardUid}");

            // HandDeck 에서 카드 제거
            HandDeck.Instance.RemoveCard(cardUid);

            // Master 의 handCards => graveCards 로 이동
            Master.Instance.HandCardToGrave(cardUid);
        }

        public void DeckCountChanged()
        {
            int count = Master.Instance.deckCards.Count;
            Board.Instance.SetDeckCount(count);
        }

        public void GraveDeckCountChange()
        {
            int count = Master.Instance.graveCards.Count;
            Board.Instance.SetGraveDeckCount(count);
        }
    }
}