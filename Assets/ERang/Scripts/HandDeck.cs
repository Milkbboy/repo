using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class HandDeck : MonoBehaviour
    {
        // 핸드 카드 생성을 위한 프리팹
        public GameObject cardPrefab;

        // 핸드 카드 리스트
        private readonly List<HCard> hCards = new();

        private float cardWidth = 0f;
        private float cardSpacing = 1f;

        private AudioSource audioSource;
        private AudioClip flipSound;

        void Awake()
        {
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
        /// 카드 생성
        /// </summary>
        /// <param name="card"></param>
        public IEnumerator SpawnHandCard(BaseCard card)
        {
            GameObject cardObject = Instantiate(cardPrefab, transform);

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
        public void TurnEndRemoveHandCard(Transform discardPos)
        {
            foreach (HCard handCard in hCards)
            {
                handCard.DiscardAnimation(discardPos);
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
    }
}