using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;

namespace ERang
{
    public class RewardUI : MonoBehaviour
    {
        public GameObject cardPrefab;

        public void ShowRewardCards(List<(RewardType, int)> rewards, UnityAction<RewardCard> OnSelectRewardCard)
        {
            // 화면 중앙에 카드를 배치
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
            worldCenter.z = 0; // Z축 위치를 0으로 설정하여 2D 평면에 배치

            float cardSpacing = 2f;
            float startX = worldCenter.x - (rewards.Count - 1) * cardSpacing / 2;

            CardFactory cardFactory = new(AiLogic.Instance);

            for (int i = 0; i < rewards.Count; ++i)
            {
                (RewardType rewardType, int value) = rewards[i];
                BaseCard card = null;

                if (rewardType == RewardType.Card)
                {
                    CardData cardData = CardData.GetCardData(value);

                    if (cardData == null)
                    {
                        Debug.LogError($"RewardUI - ShowRewardCards. CardData({value}) 데이터 없음");
                        continue;
                    }

                    card = cardFactory.CreateCard(cardData);
                }
                else if (rewardType == RewardType.HP)
                {
                    card = cardFactory.CreateHpCard(CardData.GetHpCardData(), value);
                }
                else if (rewardType == RewardType.Gold)
                {
                    card = cardFactory.CreateGoldCard(CardData.GetGoldCardData(), value);
                }

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