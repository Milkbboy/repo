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

        void OnEnable()
        {
            OnBoardSlotEquipCard += BoardSlotEquipCard;
            OnHandCardUsed += HandCardUsed;
        }

        void OnDisable()
        {
            OnBoardSlotEquipCard -= BoardSlotEquipCard;
            OnHandCardUsed -= HandCardUsed;
        }

        public void BoardSlotEquipCard(BoardSlot boardSlotRef, string cardUid)
        {
            Card card = Master.Instance.GetHandCard(cardUid);

            // BoardSlot 에 카드 장착
            boardSlotRef.EquipCard(card);

            // HandDeck 에서 카드 제거
            HandDeck.Instance.RemoveCard(cardUid);

            // Master 의 HandCards => BoardCards 로 이동
            Master.Instance.HandCardToBoard(cardUid);
        }

        public void HandCardUsed(string cardUid)
        {
            Debug.Log($"HandCardUsed: {cardUid}");

            // HandDeck 에서 카드 제거
            HandDeck.Instance.RemoveCard(cardUid);

            // Master 의 HandCards => GraveCards 로 이동
            Master.Instance.HandCardToGrave(cardUid);
        }
    }
}