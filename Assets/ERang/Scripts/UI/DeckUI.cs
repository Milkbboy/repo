using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ERang
{
    public class DeckUI : MonoBehaviour
    {
        // 덱 카드 수, 무덤 카드 수, 소멸 카드 수 표시
        public TextMeshProUGUI deckCardCountText;
        public TextMeshProUGUI graveCardCountText;
        public TextMeshProUGUI extinctionCardCountText;
        public Transform gravePosition;

        private HandDeck handDeck;

        void Awake()
        {
            handDeck = GetComponent<HandDeck>();
        }

        /// <summary>
        /// 덱 카드 개수 표시
        /// </summary>
        /// <param name="count"></param>
        public void SetDeckCardCount(int count)
        {
            deckCardCountText.text = count.ToString();
        }

        /// <summary>
        /// 그레이브 카드 개수 표시
        /// </summary>
        /// <param name="count"></param>
        public void SetGraveCardCount(int count)
        {
            graveCardCountText.text = count.ToString();
        }

        /// <summary>
        /// 소멸 카드 개수 표시
        /// </summary>
        /// <param name="count"></param>
        public void SetExtinctionCardCount(int count)
        {
            extinctionCardCountText.text = count.ToString();
        }

        public void RemoveTurnEndHandCard()
        {
            handDeck.TurnEndRemoveHandCard(gravePosition);
        }

        public void RemoveHandCard(string cardUid)
        {
            handDeck.RemoveHandCard(cardUid);
        }
    }
}