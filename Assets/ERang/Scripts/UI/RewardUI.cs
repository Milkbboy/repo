using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;

namespace ERang
{
    public class RewardUI : MonoBehaviour
    {
        public GameObject cardPrefab;

        public void ShowRewardCards(List<int> cardIds, UnityAction<RewardCard> OnSelectRewardCard)
        {
            // 화면 중앙에 카드를 배치
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
            worldCenter.z = 0; // Z축 위치를 0으로 설정하여 2D 평면에 배치

            float cardSpacing = 2f;
            float startX = worldCenter.x - (cardIds.Count - 1) * cardSpacing / 2;

            for (int i = 0; i < cardIds.Count; ++i)
            {
                CardData cardData = CardData.GetCardData(cardIds[i]);

                if (cardData == null)
                {
                    Debug.LogError($"cardId({cardIds[i]}) CardData {Utils.RedText("테이블 데이터 없음")}");
                    continue;
                }

                BaseCard card = Utils.MakeCard(cardData);

                Vector3 cardPosition = new(startX + i * cardSpacing, worldCenter.y, worldCenter.z);

                GameObject cardObject = Instantiate(cardPrefab, cardPosition, transform.rotation, transform);
                cardObject.name = $"RewardCard_{card.Id}";

                RewardCard rewardCard = cardObject.GetComponent<RewardCard>();
                rewardCard.SetCard(card);
                rewardCard.OnClick += OnSelectRewardCard;
            }
        }
    }
}