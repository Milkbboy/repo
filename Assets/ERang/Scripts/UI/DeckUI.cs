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
        
        /// <summary>
        /// 카드 생성
        /// </summary>
        /// <param name="card"></param>
        public IEnumerator SpawnHandCard(BaseCard card)
        {
            yield return handDeck.SpawnHandCard(card);
        }

        /// <summary>
        /// 핸드 카드 그리기
        /// </summary>
        public void DrawHandCards()
        {
            handDeck.DrawHandCards();
        }

        /// <summary>
        /// 턴 종료 핸드 카드 모두 제거
        /// </summary>
        public void TurnEndRemoveHandCard()
        {
            handDeck.TurnEndRemoveHandCard(graveCardCountText.rectTransform);
        }

        /// <summary>
        /// 핸드 카드 제거
        /// </summary>
        /// <param name="cardUid"></param>
        public void RemoveHandCard(string cardUid)
        {
            handDeck.RemoveHandCard(cardUid);
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

        public void UpdateHandCardUI()
        {
            handDeck.UpdateHandCardUI();
        }
    }
}