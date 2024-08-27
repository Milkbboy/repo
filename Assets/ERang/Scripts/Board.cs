using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;
using System.Linq;
using RogueEngine;

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

        private List<BoardSlot> boardSlots = new List<BoardSlot>();

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
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void CreateBoardSlots()
        {
            // 크리쳐 보드 슬롯 구성
            // [ 0: Master, 1: Creature, 2: Creature, 3: Creature, 4: None, 
            //   5: None, 6: Creature, 7: Creature, 8: Creature, 9: Master ]
            float boardWidth = boardSlot.GetComponent<BoxCollider>().size.x * boardSlot.transform.localScale.x;

            float totalWidth = (BoardSlotCardTypes.Length - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            int creatureSlotStartIndex = 3;
            int monsterSlotStartIndex = 6;

            // 보드 슬롯 타입 별로 색상 다르게 설정
            for (int i = 0; i < BoardSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new Vector3(xPosition, boardSlot.transform.position.y, boardSlot.transform.position.z);

                BoardSlot slot = Instantiate(boardSlot, slotPosition, Quaternion.identity) as BoardSlot;
                slot.CreateSlot(i, BoardSlotCardTypes[i]);
                boardSlots.Add(slot);

                switch (BoardSlotCardTypes[i])
                {
                    case CardType.Master:
                        masterSlot = slot;
                        // todo: 이 부분 수정 필요.
                        Master master = BattleLogic.Instance.GetMaster();
                        slot.CreateMasterSlot(i, master);
                        slot.SetIndex(creatureSlotStartIndex - i);
                        break;
                    case CardType.EnemyMaster:
                        enemyMasterSlot = slot;
                        slot.SetIndex(i - monsterSlotStartIndex);
                        break;
                    case CardType.Creature:
                        slot.SetIndex(creatureSlotStartIndex - i);
                        creatureSlots.Add(slot);
                        break;
                    case CardType.Monster:
                        slot.SetIndex(i - monsterSlotStartIndex);
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

        public void CreateMonsterCard()
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

            // Debug.Log($"cardType: {cardType}, nearestSlot: {nearestSlot?.Slot}");

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

        public List<BoardSlot> GetCreatureSlots()
        {
            return creatureSlots;
        }

        public List<BoardSlot> GetMonsterSlots()
        {
            return monsterSlots;
        }

        public BoardSlot GetTargetMonsterBoardSlot()
        {
            // 몬스터 슬롯 중 가장 먼저 등장하는 몬스터 슬롯을 반환
            foreach (BoardSlot slot in monsterSlots)
            {
                if (slot.IsOccupied)
                {
                    return slot;
                }
            }

            return null;
        }

        public void ResetCreatureSlot(int slot)
        {
            BoardSlot creatureSlot = creatureSlots.Find(x => x.Slot == slot);

            if (creatureSlot == null)
            {
                Debug.LogError($"ResetCreatureSlot Invalid slot: {slot}");
                return;
            }

            creatureSlot.RemoveCard();
        }

        public void ResetMonsterSlot(int slot)
        {
            BoardSlot monsterSlot = monsterSlots.Find(x => x.Slot == slot);

            if (monsterSlot == null)
            {
                Debug.LogError($"ResetMonsterSlot Invalid slot: {slot}");
                return;
            }

            monsterSlot.RemoveCard();
        }

        /// <summary>
        /// 보드 슬롯으로 보드 슬롯 반환
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public BoardSlot GetBoardSlot(int slot)
        {
            return boardSlots.Find(x => x.Slot == slot);
        }

        /// <summary>
        /// 크리쳐 보드 슬롯 인덱스로 정렬
        /// - 인덱스 3, 2, 1
        /// </summary>
        /// <returns></returns>
        public List<BoardSlot> GetCreatureBoardSlots()
        {
            return creatureSlots.OrderBy(slot => slot.Index).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BoardSlot> GetMonsterBoardSlots()
        {
            return monsterSlots.OrderBy(slot => slot.Index).ToList();
        }

        /// <summary>
        /// 크리쳐 카드가 장착된 보드 슬롯 인덱스를 정렬한 카드 반화
        /// - 슬롯 인덱스 3, 2, 1 순서
        /// </summary>
        /// <returns></returns>
        public List<Card> GetOccupiedCreatureCards()
        {
            return GetCreatureBoardSlots()
                .Where(slot => slot.IsOccupied)
                .Select(slot => slot.Card)
                .ToList();
        }

        /// <summary>
        /// 몬스터 카드가 장착된 보드 슬롯 인덱스를 정렬한 카드 반환
        /// - 슬롯 인덱스 6, 7, 8 순서
        /// </summary>
        /// <returns></returns>
        public List<Card> GetOccupiedMonsterCards()
        {
            return GetMonsterBoardSlots()
                .Where(slot => slot.IsOccupied)
                .Select(slot => slot.Card)
                .ToList();
        }

        /// <summary>
        /// 카드로 상대 카드 리스트 얻기
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public List<Card> GetOpponetCards(Card self)
        {
            if (self.type == CardType.Creature || self.type == CardType.Master)
                return GetOccupiedMonsterCards();

            if (self.type == CardType.Monster || self.type == CardType.EnemyMaster)
                return GetOccupiedCreatureCards();

            return null;
        }

        /// <summary>
        /// 슬롯으로 상대 슬롯 리스트 얻기
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public List<BoardSlot> GetOpponentSlots(BoardSlot self)
        {
            if (self.CardType == CardType.Creature || self.CardType == CardType.Master)
                return GetMonsterBoardSlots();

            if (self.CardType == CardType.Monster || self.CardType == CardType.EnemyMaster)
                return GetCreatureBoardSlots();

            return null;
        }
    }
}