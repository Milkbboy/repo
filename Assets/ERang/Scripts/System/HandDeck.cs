using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class HandDeck : MonoBehaviour
    {
        public static HandDeck Instance { get; private set; }

        public HCard DraggingCard => draggingCard;

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

        private HCard draggingCard;

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

        /// <summary>
        /// 드래깅 카드 설정
        /// - 다른 핸드 카드의 OnMouseEnter, OnMouseExit 이벤트를 방지하기 위해 설정
        /// </summary>
        public void SetDraggingCard(HCard hCard)
        {
            draggingCard = hCard;
        }

        public void MagicCardUse(HCard hCard)
        {
            if (hCard.Card != null && hCard.Card is not MagicCard)
            {
                Debug.LogWarning($"{hCard.Card.LogText}. 마법 카드가 아닙니다.");
                return;
            }

            // 공격 타입이 Select가 아닌 경우 (즉시 발동 마법)
            if (hCard.IsSelectAttackTypeCard() == false)
            {
                Debug.Log($"🎉 {hCard.Card.LogText}. 즉시 발동 마법 카드 사용!");
                BattleLogic.Instance.HandCardUse(hCard, null);
                return;
            }

            // 타겟팅 화살표 확인
            if (targetingArrow == null)
            {
                Debug.LogError($"{hCard.Card.LogText}. 타겟팅 화살표가 없습니다.");
                return;
            }

            // 🔧 중요: 선택된 타겟을 미리 저장 (나중에 화살표가 꺼져도 사용할 수 있도록)
            int selectedSlot = targetingArrow.SelectedSlotNum;

            Debug.Log($"🔍 MagicCardUse: selectedSlot={selectedSlot}");

            if (selectedSlot == -1)
            {
                Debug.LogWarning($"{hCard.Card.LogText}. 타겟이 선택되지 않았습니다.");
                return;
            }

            // 타겟 슬롯 가져오기
            BSlot targetSlot = BoardSystem.Instance.GetBoardSlot(selectedSlot);

            if (targetSlot == null)
            {
                Debug.LogError($"{hCard.Card.LogText}. 타겟 슬롯({selectedSlot})을 찾을 수 없습니다.");
                return;
            }

            // 🔧 화살표를 먼저 비활성화 (타겟 정보는 이미 저장했으므로)
            targetingArrow.EnableArrow(false);

            // 마법 카드 사용 성공!
            Debug.Log($"🎉 {hCard.Card.LogText}. 타겟: {targetSlot.LogText} - 마법 카드 사용 성공!");
            BattleLogic.Instance.HandCardUse(hCard, targetSlot);
        }

        public void SetTargettingArraow(bool isShow)
        {
            targetingArrow.EnableArrow(isShow);
        }

        /// <summary>
        /// 드래깅 핸드 카드 타겟에 해당하는 보드 슬롯인지 확인
        /// </summary>
        /// <summary>
        /// 드래깅 핸드 카드 타겟에 해당하는 보드 슬롯인지 확인
        /// </summary>
        public bool IsTargetSlot(int slotNum)
        {
            Debug.Log($"🔍 IsTargetSlot 호출: slotNum={slotNum}");

            HCard dragginCard = hCards.Find(x => x.IsDragging());

            Debug.Log($"🔍 IsTargetSlot: dragginCard={dragginCard?.Card?.LogText ?? "null"}");

            if (dragginCard == null)
            {
                Debug.Log($"🔍 IsTargetSlot: dragginCard가 null이므로 false 반환");
                return false;
            }

            bool result = dragginCard.IsContainsSlotNum(slotNum);
            Debug.Log($"🔍 IsTargetSlot: dragginCard.IsContainsSlotNum({slotNum}) = {result}");

            if (dragginCard.Card is MagicCard magicCard)
            {
                Debug.Log($"🔍 IsTargetSlot: MagicCard.TargetSlotNumbers = [{string.Join(", ", magicCard.TargetSlotNumbers)}]");
            }

            return result;
        }

        /// <summary>
        /// 카드 생성
        /// </summary>
        /// <param name="card"></param>
        public IEnumerator SpawnHandCard(BaseCard card)
        {
            GameObject cardObject = Instantiate(cardPrefab, handDeckTransform);
            cardObject.name = $"HandCard_{card.Id}";

            HCard handCard = cardObject.GetComponent<HCard>();
            handCard.SetCard(card);

            hCards.Add(handCard);

            DrawHandCards();

            // 오디오를 재생합니다.
            if (flipSound != null)
            {
                audioSource.pitch = 3f; // 재생 속도를 1.5배로 설정
                audioSource.PlayOneShot(flipSound);
                yield return new WaitForSeconds(flipSound.length / audioSource.pitch);
            }
            else
            {
                Debug.LogWarning("flipcard.mp3 파일을 찾을 수 없습니다.");
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
            HCard handCard = hCards.Find(x => x.CardUid == cardUid);

            if (handCard == null)
                return;

            hCards.Remove(handCard);
            Destroy(handCard.gameObject);

            DrawHandCards();
        }

        /// <summary>
        /// 턴 종료 핸드 카드 모두 제거
        /// </summary>
        public void TurnEndRemoveHandCard()
        {
            foreach (HCard handCard in hCards)
            {
                handCard.DiscardAnimation(GraveTransform);
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
                Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음");
                yield break;
            }

            CardFactory cardFactory = new(AiLogic.Instance);

            BaseCard card = cardFactory.CreateCard(cardData);

            GameObject cardObject = Instantiate(summonCardPrefab, transform);
            cardObject.name = $"SummonCard_{card.Id}";

            SummonCard summonCard = cardObject.GetComponent<SummonCard>();
            summonCard.SetCard(card);

            yield return new WaitForSeconds(.5f);

            DiscardAnimation discardAnimation = summonCard.GetComponent<DiscardAnimation>();

            switch (deckKind)
            {
                case DeckKind.Hand:
                    DeckManager.Instance.Data.AddToHand(card);
                    discardAnimation.PlaySequence(handDeckTransform, SpawnHandCard(card));
                    break;

                case DeckKind.Grave:
                    DeckManager.Instance.Data.AddToGrave(card);
                    discardAnimation.PlaySequence(GraveTransform, null);
                    break;

                case DeckKind.Deck:
                    DeckManager.Instance.Data.AddToDeck(card);
                    discardAnimation.PlaySequence(DeckTransform, null);
                    break;
            }
        }
    }
}