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

        // 핸드 카드 생성을 위한 프리팹
        public GameObject cardPrefab;
        public GameObject handDeckObject;

        private float cardWidth = 0f;
        private float cardSpacing = 1f;

        private AudioSource audioSource;
        private AudioClip flipSound;

        // 핸드 카드 리스트
        private readonly List<HandCard> handCards = new();

        void Awake()
        {
            // 카드의 너비를 얻기 위해 cardPrefab의 BoxCollider 컴포넌트에서 size.x 값을 사용
            cardWidth = cardPrefab.GetComponent<BoxCollider>().size.x;
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
        /// 카드 생성
        /// </summary>
        /// <param name="card"></param>
        public IEnumerator SpawnHandCard(Card card)
        {
            GameObject cardObject = Instantiate(cardPrefab, handDeckObject.transform);

            HandCard handCard = cardObject.GetComponent<HandCard>();
            handCard.SetCard(card);

            handCards.Add(handCard);

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
            float overlap = cardWidth * 0.05f; // 20% 겹치도록 설정
            cardSpacing = cardWidth + overlap; // 카드 간격 재조정

            // 카드 정렬 로직 시작
            float totalWidth = (handCards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2;

            for (int i = 0; i < handCards.Count; i++)
            {
                // 카드의 X 위치 설정, Y와 Z 위치는 고정
                float xPosition = startX + i * cardSpacing;
                // 카드의 Z 위치를 인덱스에 따라 조정하여 위에 있는 카드가 앞으로 오도록 설정
                // float zPosition = i * zOffset;

                handCards[i].SetDrawPostion(new Vector3(xPosition, 0, 0));
            }
        }

        /// <summary>
        /// 턴 종료 핸드 카드 모두 제거
        /// </summary>
        public void TurnEndRemoveHandCard()
        {
            foreach (HandCard handCard in handCards)
            {
                handCard.DiscardAnimation(extinctionCardCountText.rectTransform);
            }

            handCards.Clear();
        }

        /// <summary>
        /// 핸드 카드 제거
        /// </summary>
        /// <param name="cardUid"></param>
        public void RemoveHandCard(string cardUid)
        {
            HandCard handCard = handCards.Find(x => x.CardUid == cardUid);

            if (handCard == null)
                return;

            handCards.Remove(handCard);
            Destroy(handCard.gameObject);

            DrawHandCards();
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
    }
}