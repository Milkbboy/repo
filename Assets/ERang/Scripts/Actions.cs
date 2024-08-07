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
        public static UnityAction<string> OnHandCardUsed;

        void OnEnable()
        {
            OnHandCardUsed += HandCardUsed;
        }

        void OnDisable()
        {
            OnHandCardUsed -= HandCardUsed;
        }

        public void HandCardUsed(string cardUid)
        {
        }

        public void ExtinctionDeckCount()
        {
            int count = Master.Instance.extinctionCards.Count;
            Board.Instance.SetExtinctionDeckCount(count);
        }
    }
}