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

        public GameObject cardPrefab;
        public HandDeck handDeck;
        public Transform gravePosition;

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

        public IEnumerator DrawHandDeck(List<BaseCard> handCards)
        {
            for (int i = 0; i < handCards.Count; ++i)
            {
                BaseCard card = handCards[i];

                yield return handDeck.SpawnHandCard(card);
            }
        }
    }
}