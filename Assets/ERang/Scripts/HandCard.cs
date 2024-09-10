using UnityEngine;

namespace ERang
{
    public class HandCard : MonoBehaviour
    {
        [SerializeField] private int cardId;
        [SerializeField] private CardType cardType;
        private string cardUid;
        private CardUI cardUI;
        private Vector3 originalPosition;
        private bool drag = false;

        // Start is called before the first frame update
        void Awake()
        {
            cardUI = GetComponent<CardUI>();
        }

        // 카드 hp, atk, def, costMana, costGold 등은 cardData 의 기본 값에서 해당 카드의 ability 로 최종 값을 결정하자.

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnMouseDown()
        {
            // Debug.Log("OnMouseDown card: " + cardId);
            drag = true;
        }

        void OnMouseDrag()
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = new Vector3(objPosition.x, objPosition.y, originalPosition.z - 0.15f);
            drag = false;
        }

        void OnMouseUp()
        {
            // 마스터 마나 확인
            if (BattleLogic.Instance.CanHandCardUse(cardUid) == false)
            {
                // 원래 위치로 돌아가게 함
                transform.position = originalPosition;
                return;
            }

            BoardSlot boardSlot = Board.Instance.NeareastBoardSlot(transform.position);

            // CardType 별 동작
            switch (cardType)
            {
                case CardType.Creature:
                case CardType.Building:
                    // 보드 슬롯에 이미 카드가 장착되어 있는 경우
                    if (boardSlot == null || boardSlot.IsOccupied == true)
                    {
                        break;
                    }

                    // 보드 슬롯에 카드를 장착
                    if (boardSlot != null && boardSlot.CardType == this.cardType)
                    {
                        BattleLogic.Instance.BoardSlotEquipCard(boardSlot, cardUid);
                    }
                    break;
                case CardType.Curse:
                case CardType.Charm:
                case CardType.Magic:
                case CardType.Individuality:
                    Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);

                    // 카드 사용
                    BattleLogic.Instance.HandCardUse(cardUid, boardSlot);
                    break;
            }

            // 원래 위치로 돌아가게 함
            transform.position = originalPosition;
        }

        public void SetCard(Card card)
        {
            cardUid = card.uid;
            cardId = card.id;
            cardType = card.type;

            cardUI.SetCard(card);
        }

        public void SetDrawPostion(Vector3 position)
        {
            transform.localPosition = position;
            originalPosition = transform.position;
        }

        public string GetUID()
        {
            return cardUid;
        }

        public bool IsDrag()
        {
            return drag;
        }

        private bool IsUpperCenter(Vector3 screenPosition)
        {
            float screenHeight = Screen.height;

            // Define the upper center area (e.g., top 50% of the screen and middle 50% width)
            float upperHeight = screenHeight * 0.5f;

            Debug.Log($"screenPosition.y({screenPosition.y}) > screenHeight({screenHeight}) - upperHeight({upperHeight}): {screenPosition.y > screenHeight - upperHeight}");

            return screenPosition.y > screenHeight - upperHeight;
        }
    }
}