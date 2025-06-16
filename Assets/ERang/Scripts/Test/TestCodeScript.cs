using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class TestCodeScript : MonoBehaviour
    {
        public GameObject cardPrefab;
        public List<BaseCard> cards = new();

        public TargetingArrow targetingArrow;

        private float cardWidth = 0f;
        private float cardSpacing = 1f;
        private List<HandCard> handCards = new();
        private bool flip = false;

        void Awake()
        {
            // BoxCollider의 size는 로컬 공간에서의 크기를 나타내서 Transform의 localScale은 객체의 스케일을 곱한 값
            BoxCollider boxCollider = cardPrefab.GetComponent<BoxCollider>();
            cardWidth = boxCollider.size.x * cardPrefab.transform.localScale.x;
        }

        public void Test()
        {
            flip = !flip;

            CardData cardData = CardData.GetCardData(1006);

            if (cardData == null)
            {
                Debug.LogError($"TestCodeScript - Test. CardData({1006}) 데이터 없음");
                return;
            }

            handCards[0].SetCard(new MagicCard(cardData));
        }

        // Start is called before the first frame update
        void Start()
        {
            // 마스터
            MasterData masterData = MasterData.GetMasterData(1001);

            foreach (int cardId in masterData.startCardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"TestCodeScript - Start. CardData({cardId}) 데이터 없음");
                    continue;
                }

                BaseCard card = cardData.cardType switch
                {
                    CardType.Creature => new CreatureCard(cardData),
                    CardType.Building => new BuildingCard(cardData),
                    CardType.Charm => new MagicCard(cardData),
                    CardType.Curse => new MagicCard(cardData),
                    CardType.Magic => new MagicCard(cardData),
                    _ => null
                };

                if (card == null)
                {
                    Debug.LogError($"TestCodeScript - Start. CardData({cardId}) 데이터 없음");
                    continue;
                }

                // 카드 타입별로 생성
                cards.Add(card);
            }

            foreach (BaseCard card in cards)
            {
                // Debug.Log($"CardType: {card.CardType}, class type: {card.GetType()}");
                if (card is CreatureCard creatureCard)
                {
                    Debug.Log($"CreatureCard: {creatureCard.Hp}, {creatureCard.MaxHp}, {creatureCard.Def}, {creatureCard.Mana}");
                }

                if (card is BuildingCard buildingCard)
                {
                    Debug.Log($"BuildingCard: {buildingCard.Gold}");
                }

                if (card is MagicCard magicCard)
                {
                    Debug.Log($"MagicCard: {magicCard.Mana}");
                }

                GameObject cardObject = Instantiate(cardPrefab, transform);

                HandCard handCard = cardObject.GetComponent<HandCard>();
                handCard.SetCard(card);

                handCards.Add(handCard);
            }

            DrawCards();
        }

        public void DrawCards()
        {
            // 겹치는 정도를 조절하기 위해 cardWidth의 일부를 사용
            float overlap = cardWidth * 0.05f; // 20% 겹치도록 설정
            cardSpacing = cardWidth + overlap; // 카드 간격 재조정

            // 카드 정렬 로직 시작
            float totalWidth = (cards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2;

            for (int i = 0; i < handCards.Count; i++)
            {
                // 카드의 X 위치 설정, Y와 Z 위치는 고정
                float xPosition = startX + i * cardSpacing;
                // 카드의 Z 위치를 인덱스에 따라 조정하여 위에 있는 카드가 앞으로 오도록 설정
                // float zPosition = i * zOffset;

                Debug.Log($"xPosition: {xPosition}");

                handCards[i].SetDrawPostion(new Vector3(xPosition, 0, 0));
            }
        }
    }
}