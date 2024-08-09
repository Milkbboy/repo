using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;

namespace ERang
{
    public class Board : MonoBehaviour
    {
        // Start is called before the first frame update
        public static Board Instance { get; private set; }

        public readonly CardType[] BoardSlotCardTypes = { CardType.Master, CardType.Creature, CardType.Creature, CardType.Creature, CardType.None, CardType.None, CardType.Monster, CardType.Monster, CardType.Monster, CardType.EnemyMaster };
        public readonly CardType[] BuildingSlotCardTypes = { CardType.Building, CardType.Building, CardType.None, CardType.None };
        public BoardSlot boardSlot;
        public DeckUI deckUI;
        public DeckUI graveDeckUI;
        public DeckUI extinctionDeckUI;
        public TurnUI turnUI;

        private BoardSlot masterSlot;
        private List<BoardSlot> creatureSlots = new List<BoardSlot>();
        private List<BoardSlot> buildingSlots = new List<BoardSlot>();

        private BoardSlot enemyMasterSlot;
        private List<BoardSlot> monsterSlots = new List<BoardSlot>();

        private float boardSpacing = 0.2f;

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            CreateBoardSlots();
            CreateMonsterCard();
        }

        // Update is called once per frame
        void Update()
        {
        }

        void CreateBoardSlots()
        {
            // 크리쳐 보드 슬롯 구성
            // [ 0: Master, 1: Creature, 2: Creature, 3: Creature, 4: None, 
            //   5: None, 6: Creature, 7: Creature, 8: Creature, 9: Master ]
            float boardWidth = boardSlot.GetComponent<BoxCollider>().size.x * boardSlot.transform.localScale.x;

            float totalWidth = (BoardSlotCardTypes.Length - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            for (int i = 0; i < BoardSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new Vector3(xPosition, boardSlot.transform.position.y, boardSlot.transform.position.z);

                BoardSlot slot = Instantiate(boardSlot, slotPosition, Quaternion.identity) as BoardSlot;
                slot.CreateSlot(i, BoardSlotCardTypes[i]);

                switch (BoardSlotCardTypes[i])
                {
                    case CardType.Master:
                        masterSlot = slot;
                        // todo: 이 부분 수정 필요.
                        Master master = BattleLogic.Instance.GetMaster();
                        slot.CreateMasterSlot(i, master);
                        break;
                    case CardType.EnemyMaster:
                        enemyMasterSlot = slot;
                        break;
                    case CardType.Creature:
                        creatureSlots.Add(slot);
                        break;
                    case CardType.Monster:
                        monsterSlots.Add(slot);
                        break;
                }

                // 액션 등록해서 설정하는 방법인 듯
                // if (BoardSlotCardTypes[i] == CardType.Creature)
                // {
                //     slot.OnCardDrop += (card) =>
                //     {
                //         HandCard handCard = card.GetComponent<HandCard>();
                //         BoardSlot nearestSlot = NeareastBoardSlotSlot(handCard.transform.position, handCard.CardData.cardType);

                //         if (nearestSlot != null)
                //         {
                //             nearestSlot.SetCard(handCard);
                //         }
                //     };
                // }
            }

            // 빌딩 보드 슬롯 구성
            startX = -5f;
            float startY = 2f;

            for (int i = 0; i < BuildingSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new Vector3(xPosition, startY, boardSlot.transform.position.z);

                BoardSlot slot = Instantiate(boardSlot, slotPosition, Quaternion.identity);
                slot.CreateSlot(i, BuildingSlotCardTypes[i]);

                buildingSlots.Add(slot);
            }
        }

        void CreateMonsterCard()
        {
            Enemy enemy = BattleLogic.Instance.GetEnemy();

            for (int i = 0; i < monsterSlots.Count; i++)
            {
                BoardSlot slot = monsterSlots[i];
                slot.EquipCard(enemy.monsterCards[i]);
            }
        }


        public BoardSlot NeareastBoardSlotSlot(Vector3 position, CardType cardType)
        {
            BoardSlot nearestSlot = null;
            float minDistance = float.MaxValue;

            List<BoardSlot> boardSlots = cardType == CardType.Creature ? creatureSlots : buildingSlots;

            foreach (BoardSlot slot in boardSlots)
            {
                // Debug.Log($"slot: {BoardSlot.slot}, isOverlapCard: {BoardSlot.isOverlapCard}");

                if (slot.IsOverlapCard == false)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, slot.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSlot = slot;
                }
            }

            Debug.Log($"cardType: {cardType}, nearestSlot: {nearestSlot?.GetSlot()}");

            return nearestSlot;
        }

        public void SetDeckCount(int count)
        {
            deckUI.SetCount(count);
        }

        public void SetGraveDeckCount(int count)
        {
            graveDeckUI.SetCount(count);
        }

        public void SetExtinctionDeckCount(int count)
        {
            extinctionDeckUI.SetCount(count);
        }

        public void SetTurnCount(int turn)
        {
            turnUI.SetTurn(turn);
        }

        public void SetMasterStat(Master master)
        {
            masterSlot.SetMasterStat(master);
        }
    }
}