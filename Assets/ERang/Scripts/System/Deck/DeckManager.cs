using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

        private DeckUI deckUI;
        private HandDeck handDeck;
        private ShowAllCard showAllCard;

        // 기능별 컴포넌트들 - 구체 클래스 사용
        public DeckData Data { get; private set; }
        public DeckOperations Operations { get; private set; }
        public DeckEvents Events { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            deckUI = GetComponent<DeckUI>();
            handDeck = GetComponent<HandDeck>();
            showAllCard = GetComponent<ShowAllCard>();

            Data = new DeckData();
            Events = new DeckEvents();  // Events를 먼저 초기화
            Operations = new DeckOperations(Data, Events, this);

            // 이벤트 구독
            Events.OnCardCountChanged += UpdateUI;
        }

        void OnDestroy()
        {
            Events.OnCardCountChanged -= UpdateUI;
        }

        public void CreateMasterCards(Player player) => Operations.CreateMasterCards(player);

        public IEnumerator MakeHandCards()
        {
            // 1단계: 데이터 처리 (순수 로직)
            Operations.MakeHandCards();

            // 2단계: UI 처리 (정렬된 순서대로 스폰)
            for (int i = 0; i < Data.HandCards.Count; i++)
            {
                BaseCard card = Data.HandCards[i];
                yield return handDeck.SpawnHandCard(card);
            }

            UpdateUI(Operations.CreateCountInfo());
        }

        public void RemoveHandCard(string cardUid) => Operations.RemoveHandCard(cardUid, handDeck);
        public void HandCardToBoard(BaseCard card) => Operations.HandCardToBoard(card, handDeck);
        public void TurnEndProcess() => Operations.TurnEndProcess(handDeck);

        // 기존 API 호환성 유지
        public BaseCard FindHandCard(string cardUid) => Data.FindHandCard(cardUid);
        public void ShowDeckCards() => showAllCard.ToggleShowCards(Data.DeckCards);
        public void ShowGraveCards() => showAllCard.ToggleShowCards(Data.GraveCards);

        private void UpdateUI(DeckCountInfo countInfo)
        {
            deckUI.SetDeckCardCount(countInfo.DeckCardCount);
            deckUI.SetGraveCardCount(countInfo.GraveCardCount);
            deckUI.SetExtinctionCardCount(countInfo.ExtinctionCardCount);
        }
    }
}