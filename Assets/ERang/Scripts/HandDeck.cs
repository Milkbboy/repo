using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class HandDeck : MonoBehaviour
    {
        public static HandDeck Instance { get; private set; }

        public HCard DraggingCard { get; private set; }

        // 핸드 카드 생성을 위한 프리팹
        public GameObject cardPrefab;
        // 소환 카드 생성을 위한 프리펩
        public GameObject summonCardPrefab;
        public TargetingArrow targetingArrow;

        public Transform handDeckTransform;
        public Transform DeckTransform;
        public Transform GraveTransform;

        // 핸드 카드 리스트
        private readonly List<HCard> hCards = new();

        private float cardWidth = 0f;
        private float cardSpacing = 1f;

        private AudioSource audioSource;
        private AudioClip flipSound;

        void Awake()
        {
            Instance = this;

            // 카드의 너비를 얻기 위해 cardPrefab의 BoxCollider 컴포넌트에서 size.x 값을 사용
            BoxCollider boxCollider = cardPrefab.GetComponent<BoxCollider>();
            cardWidth = boxCollider.size.x * cardPrefab.transform.localScale.x;
        }

        void Start()
        {
            // AudioSource 컴포넌트를 추가하고 숨김니다.
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // 오디오 클립을 로드합니다.
            flipSound = Resources.Load<AudioClip>("Audio/flipcard");
        }

        void OnEnable()
        {
            CardEvents.OnDraggingCardChanged += HandleDraggingCardChanged;
            CardEvents.OnTargetingArrowVisibilityChanged += HandleTargetingArrowVisibilityChanged;
        }

        void OnDisable()
        {
            CardEvents.OnDraggingCardChanged -= HandleDraggingCardChanged;
            CardEvents.OnTargetingArrowVisibilityChanged -= HandleTargetingArrowVisibilityChanged;
        }

        private void HandleDraggingCardChanged(GameObject card)
        {
            DraggingCard = card?.GetComponent<HCard>();
        }

        private void HandleTargetingArrowVisibilityChanged(bool isVisible)
        {
            SetTargettingArraow(isVisible);
        }

        /// <summary>
        /// 드래깅 카드 설정
        /// - 다른 핸드 카드의 OnMouseEnter, OnMouseExit 이벤트를 방지하기 위해 설정
        /// </summary>
        public void SetDraggingCard(HCard hCard)
        {
            DraggingCard = hCard;
        }

        public void MagicCardUse(HCard hCard)
        {
            if (hCard.Card != null && hCard.Card is not MagicCard)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {hCard.Card.Name}는 마법 카드가 아닙니다.");
                return;
            }

            GameLogger.LogCardChain(hCard.Card.Name, "마법 카드 사용 시작");

            // 공격 타입이 Select가 아닌 경우
            if (hCard.IsSelectAttackTypeCard() == false)
            {
                GameLogger.LogCardChain(hCard.Card.Name, "즉시 발동", "", "타겟 선택 불필요");
                BattleLogic.Instance.HandCardUse(hCard, null);
                return;
            }

            if (targetingArrow == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {hCard.Card.LogText} - 타겟팅 화살표가 없습니다.");
            }

            if (targetingArrow.SelectedSlotNum == -1)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {hCard.Card.LogText} - 타겟이 선택되지 않음");
                return;
            }

            BSlot targetSlot = BoardSystem.Instance.GetBoardSlot(targetingArrow.SelectedSlotNum);

            if (targetSlot == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {hCard.Card.LogText} - 유효하지 않은 슬롯 {targetingArrow.SelectedSlotNum}");
                return;
            }

            string targetInfo = targetSlot.Card?.Name ?? $"빈 슬롯 {targetingArrow.SelectedSlotNum}";
            GameLogger.LogCardChain(hCard.Card.Name, "타겟 선택", targetInfo);

            targetingArrow.EnableArrow(false);

            GameLogger.LogCardChain(hCard.Card.Name, "효과 발동 요청", "", "BattleLogic으로 전달");
            BattleLogic.Instance.HandCardUse(hCard, targetSlot);
        }

        public void SetTargettingArraow(bool isShow)
        {
            targetingArrow.EnableArrow(isShow);
        }

        /// <summary>
        /// 드래깅 핸드 카드 타겟에 해당하는 보드 슬롯인지 확인
        /// </summary>
        public bool IsTargetSlot(int slotNum)
        {
            if (DraggingCard == null)
                return false;

            return DraggingCard.IsContainsSlotNum(slotNum);
        }

        /// <summary>
        /// 카드 생성
        /// </summary>
        /// <param name="card"></param>
        public IEnumerator SpawnHandCard(GameCard card)
        {
            GameLogger.LogCardChain(card.Name, "핸드에 추가");

            GameObject cardObject = Instantiate(cardPrefab, handDeckTransform);
            cardObject.name = $"HandCard_{card.Id}";

            HCard handCard = cardObject.GetComponent<HCard>();
            handCard.SetCard(card);

            hCards.Add(handCard);
            DrawHandCards();

            // 현재 핸드 상태 로그
            string handCards = string.Join(", ", hCards.Select(h => h.Card.Name));
            GameLogger.Log(LogCategory.CARD, $"🃏 현재 핸드: {handCards}");

            // 오디오를 재생합니다.
            if (flipSound != null)
            {
                GameLogger.Log(LogCategory.AUDIO, "카드 뒤집기 사운드 재생");
                audioSource.pitch = 3f; // 재생 속도를 1.5배로 설정
                audioSource.PlayOneShot(flipSound);
                yield return new WaitForSeconds(flipSound.length / audioSource.pitch);
            }
            else
            {
                GameLogger.Log(LogCategory.ERROR, "❌ flipcard.mp3 파일을 찾을 수 없습니다.");
                yield return null;
            }
        }

        /// <summary>
        /// 핸드 카드 그리기
        /// </summary>
        public void DrawHandCards()
        {
            // 겹치는 정도를 조절하기 위해 cardWidth의 일부를 사용
            float overlap = cardWidth * 0.05f;
            cardSpacing = cardWidth + overlap;

            // 카드 정렬 로직 시작
            float totalWidth = (hCards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2;

            for (int i = 0; i < hCards.Count; i++)
            {
                // 카드의 X 위치 설정, Y와 Z 위치는 고정
                float xPosition = startX + i * cardSpacing;
                // 카드의 Z 위치를 인덱스에 따라 조정하여 위에 있는 카드가 앞으로 오도록 설정
                // float zPosition = i * zOffset;

                hCards[i].SetDrawPostion(new Vector3(xPosition, 0, 0));
            }
        }

        /// <summary>
        /// 핸드 카드 제거
        /// </summary>
        /// <param name="cardUid"></param>
        public void RemoveHandCard(string cardUid)
        {
            HCard handCard = hCards.Find(x => x.Card.Uid == cardUid);

            if (handCard == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ 핸드에서 카드({cardUid})를 찾을 수 없음");
                return;
            }

            GameLogger.LogCardChain(handCard.Card.Name, "핸드에서 제거");

            hCards.Remove(handCard);
            Destroy(handCard.gameObject);

            DrawHandCards();

            // 제거 후 핸드 상태 로그
            if (hCards.Count > 0)
            {
                string remainingCards = string.Join(", ", hCards.Select(h => h.Card.Name));
                GameLogger.Log(LogCategory.CARD, $"🃏 남은 핸드: {remainingCards}");
            }
            else
            {
                GameLogger.Log(LogCategory.CARD, "🃏 핸드가 비었습니다");
            }
        }

        /// <summary>
        /// 턴 종료 핸드 카드 모두 제거
        /// </summary>
        public void TurnEndRemoveHandCard(Transform discardPos)
        {
            if (hCards.Count > 0)
            {
                string discardedCards = string.Join(", ", hCards.Select(h => h.Card.Name));
                GameLogger.Log(LogCategory.CARD, $"🗑️ 턴 종료 - 핸드 카드 폐기: {discardedCards}");
                foreach (HCard handCard in hCards)
                {
                    handCard.DiscardAnimation(discardPos);
                }
            }

            hCards.Clear();
        }

        public void UpdateHandCardUI()
        {
            foreach (HCard handCard in hCards)
            {
                handCard.UpdateCardUI();
            }
        }

        public IEnumerator SummonCardToDeck(int cardId, DeckKind deckKind)
        {
            CardData cardData = CardData.GetCardData(cardId);

            if (cardData == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ CardData 테이블에 {cardId} 카드 없음");
                yield break;
            }

            GameCard card = Utils.MakeCard(cardData);
            GameLogger.LogCardChain(card.Name, "카드 생성", deckKind.ToString());

            GameObject cardObject = Instantiate(summonCardPrefab, transform);
            cardObject.name = $"SummonCard_{card.Id}";

            SummonCard summonCard = cardObject.GetComponent<SummonCard>();
            summonCard.SetCard(card);

            yield return new WaitForSeconds(.5f);

            DiscardAnimation discardAnimation = summonCard.GetComponent<DiscardAnimation>();

            switch (deckKind)
            {
                case DeckKind.Hand:
                    Deck.Instance.AddHandCard(card);
                    GameLogger.LogCardChain(card.Name, "핸드로 이동");
                    discardAnimation.PlaySequence(handDeckTransform, SpawnHandCard(card));
                    break;

                case DeckKind.Grave:
                    Deck.Instance.AddGraveCard(card);
                    GameLogger.LogCardChain(card.Name, "무덤으로 이동");
                    discardAnimation.PlaySequence(GraveTransform, null);
                    break;

                case DeckKind.Deck:
                    Deck.Instance.AddDeckCard(card);
                    GameLogger.LogCardChain(card.Name, "덱으로 이동");
                    discardAnimation.PlaySequence(DeckTransform, null);
                    break;
            }
        }
    }
}