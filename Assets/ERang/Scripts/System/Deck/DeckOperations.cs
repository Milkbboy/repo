using UnityEngine;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class DeckOperations
    {
        private readonly DeckData deckData;
        private readonly DeckEvents events;
        private readonly MonoBehaviour owner; // STartCoroutine을 위한 Monbehaviour 참조
        private readonly int maxHandCardCount = 5;
        private bool isCreatedStartCard = false;

        public DeckOperations(DeckData deckData, DeckEvents events, MonoBehaviour owner)
        {
            this.deckData = deckData;
            this.events = events;
            this.owner = owner;
        }

        /// 마스터 덱 카드 생성
        public void CreateMasterCards(Player player)
        {
            if (isCreatedStartCard == true)
            {
                Debug.LogError("이미 마스터 시작 카드 생성됨");
                deckData.Clear();
                foreach (var card in Player.Instance.AllCards)
                    deckData.AddToDeck(card);

                events.TriggerCardCountChanged(CreateCountInfo());
                Debug.Log($"CreateMasterCards. deckData.DeckCardCount: {deckData.DeckCardCount}, {string.Join(", ", deckData.DeckCards.Select(card => card.Id))}");
                return;
            }

            // ✅ AiLogic.Instance null 체크 추가
            if (AiLogic.Instance == null)
            {
                Debug.LogError("AiLogic.Instance가 아직 초기화되지 않았습니다. BattleController의 초기화 순서를 확인해주세요.");
                return;
            }

            CardFactory cardFactory = new(AiLogic.Instance);

            foreach (int cardId in player.CardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);
                if (cardData == null)
                {
                    Debug.LogError($"DeckOperations - CreateMasterCards. CardData({cardId}) 데이터 없음");
                    continue;
                }

                BaseCard card = cardFactory.CreateCard(cardData);

                if (card is MagicCard magicCard)
                    magicCard.SetSelectAttackType(AiLogic.Instance.IsSelectAttackType(card));

                Player.Instance.AllCards.Add(card);
                deckData.AddToDeck(card);
            }

            isCreatedStartCard = true;
            events.TriggerCardCountChanged(CreateCountInfo());
            Debug.Log($"CreateMasterCards. deckData.DeckCardCount: {deckData.DeckCardCount}, {string.Join(", ", deckData.DeckCards.Select(card => card.Id))}");
        }

        // 핸드 카드 생성
        public void MakeHandCards()
        {
            // 덱 카드 섞기
            deckData.ShuffleDeck();

            // 1 단계: NextTurnSelect 카드를 덱에서 찾아서 핸드에 우선 추가
            var nextTurnSelectCards = deckData.ExtractNextTurnSelectCards();
            foreach (var card in nextTurnSelectCards)
            {
                deckData.AddToHand(card);
                Debug.Log($"MakeHandCards. NextTurnSelect 카드 핸드 보장: {card.ToCardLogInfo()}");
            }

            // 2 단계: 나머지 슬롯을 일반 카드로 채우기
            int spawnCount = maxHandCardCount - deckData.HandCardCount;

            for (int i = 0; i < spawnCount; ++i)
            {
                // 덱에 카드가 없으면 무덤 카드를 덱으로 옮김
                if (deckData.DeckCardCount == 0)
                {
                    deckData.MoveGraveToDeck();
                    deckData.ShuffleDeck();
                }

                BaseCard card = deckData.DrawToCard();
                if (card != null)
                {
                    deckData.AddToHand(card);
                    Debug.Log($"MakeHandCards. 일반 카드 추가: {card.ToCardLogInfo()}");
                }
            }

            // 핸드 카드 어빌리티 설정
            foreach (var card in deckData.HandCards)
            {
                // Traits를 None으로 설정 (해당 턴에 효과 발동하고 리셋)
                if ((card.Traits & CardTraits.NextTurnSelect) == CardTraits.NextTurnSelect)
                {
                    card.SetCardTraits(CardTraits.None);
                }

                // Debug.Log($"MakeHandCards. card: {card}, {card.LogText}, handAbilities.Count: {card.AbilitySystem.HandAbilities.Count}");
                owner.StartCoroutine(AbilityLogic.Instance.HandCardAbilityAction(card));
            }

            events.TriggerCardCountChanged(CreateCountInfo());
        }

        // 핸드 카드를 덱으로 이동
        public void HandCardToDeck(BaseCard card, HandDeck handDeck)
        {
            if (card == null)
            {
                Debug.LogError($"핸드덱에 카드 없음");
                return;
            }

            deckData.RemoveFromHand(card);
            handDeck.RemoveHandCard(card.Uid);

            deckData.AddToDeck(card);

            events.TriggerCardCountChanged(CreateCountInfo());
        }

        // 핸드 카드를 보드로 이동
        public void HandCardToBoard(BaseCard card, HandDeck handDeck)
        {
            if (card == null)
            {
                Debug.LogError($"핸드덱에 카드 없음");
                return;
            }

            deckData.RemoveFromHand(card);
            handDeck.RemoveHandCard(card.Uid);

            if (card is BuildingCard)
                deckData.AddToBuilding(card);

            events.TriggerCardCountChanged(CreateCountInfo());
        }

        public void RemoveHandCard(string cardUid, HandDeck handDeck)
        {
            BaseCard card = deckData.FindHandCard(cardUid);
            if (card == null)
            {
                Debug.LogError($"핸드에 {cardUid} 카드 없음");
                return;
            }

            deckData.RemoveFromHand(card);
            handDeck.RemoveHandCard(cardUid);

            // 핸드 카드 어빌리티 해제
            owner.StartCoroutine(AbilityLogic.Instance.HandCardAbilityRelease(card));

            if (card.IsExtinction)
                deckData.AddToExtinction(card);
            else
                deckData.AddToGrave(card);

            events.TriggerCardCountChanged(CreateCountInfo());
        }

        // 핸드 카드를 모두 무덤으로 이동
        public void TurnEndProcess(HandDeck handDeck)
        {
            foreach (var card in deckData.HandCards.ToList())
            {
                // 핸드 카드 어빌리티 해제
                owner.StartCoroutine(AbilityLogic.Instance.HandCardAbilityRelease(card));

                deckData.RemoveFromHand(card);
                deckData.AddToGrave(card);
            }

            handDeck.TurnEndRemoveHandCard();
            events.TriggerCardCountChanged(CreateCountInfo());
        }

        public DeckCountInfo CreateCountInfo()
        {
            return new DeckCountInfo
            {
                DeckCardCount = deckData.DeckCardCount,
                HandCardCount = deckData.HandCardCount,
                GraveCardCount = deckData.GraveCardCount,
                ExtinctionCardCount = deckData.ExtinctionCardCount
            };
        }
    }
}