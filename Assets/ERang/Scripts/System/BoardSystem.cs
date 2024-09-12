using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ERang
{
    public class BoardSystem : MonoBehaviour
    {
        public static BoardSystem Instance { get; private set; }
        public readonly CardType[] boardSlotCardTypes = { CardType.Master, CardType.Creature, CardType.Creature, CardType.Creature, CardType.None, CardType.None, CardType.Monster, CardType.Monster, CardType.Monster, CardType.EnemyMaster };
        public readonly CardType[] buildingSlotCardTypes = { CardType.Building, CardType.Building, CardType.None, CardType.None };

        public BoardSlot boardSlot;

        private BoardUI boardUI;

        private readonly List<BoardSlot> boardSlots = new();
        private readonly List<BoardSlot> buildingSlots = new();
        private readonly List<BoardSlot> leftSlots = new(); // 왼쪽 보드 슬롯. 마스터, 크리쳐
        private readonly List<BoardSlot> rightSlots = new(); // 오른쪽 보드 슬롯. 몬스터, 적 마스터

        void Awake()
        {
            Instance = this;

            boardUI = GetComponent<BoardUI>();
        }

        /// <summary>
        /// 보드 슬롯 구성
        /// [ 0: Master, 1: Creature, 2: Creature, 3: Creature, 4: None, 
        ///   5: None, 6: Creature, 7: Creature, 8: Creature, 9: Master ]
        /// </summary>
        /// <param name="master"></param>
        /// <param name="enemy"></param>
        public void CreateBoardSlots(Master master, Enemy enemy)
        {
            float boardSpacing = 0.2f;
            float boardWidth = boardSlot.GetComponent<BoxCollider>().size.x * boardSlot.transform.localScale.x;
            float totalWidth = (boardSlotCardTypes.Length - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            int leftSlotStartIndex = 3;
            int rightSlotStartIndex = 6;

            // 크리쳐, 몬스터 보드 슬롯 구성
            for (int i = 0; i < boardSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new(xPosition, boardSlot.transform.position.y, boardSlot.transform.position.z);

                BoardSlot slot = Instantiate(boardSlot, slotPosition, Quaternion.identity);
                slot.CreateSlot(i, boardSlotCardTypes[i]);

                boardSlots.Add(slot);

                switch (boardSlotCardTypes[i])
                {
                    case CardType.Master:
                        slot.SetMasterSlot(master);
                        slot.SetIndex(leftSlotStartIndex - i);
                        leftSlots.Add(slot);
                        break;
                    case CardType.Creature:
                        slot.SetIndex(leftSlotStartIndex - i);
                        leftSlots.Add(slot);
                        break;
                    case CardType.EnemyMaster:
                        slot.SetIndex(i - rightSlotStartIndex);
                        rightSlots.Add(slot);
                        break;
                    case CardType.Monster:
                        slot.SetIndex(i - rightSlotStartIndex);
                        rightSlots.Add(slot);
                        break;
                }
            }

            // 빌딩 보드 슬롯 구성
            startX = -5f;
            float startY = 2f;

            for (int i = 0; i < buildingSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new(xPosition, startY, boardSlot.transform.position.z);

                BoardSlot slot = Instantiate(boardSlot, slotPosition, Quaternion.identity);
                slot.CreateSlot(i, buildingSlotCardTypes[i]);

                buildingSlots.Add(slot);
            }
        }

        /// <summary>
        /// 몬스터 카드 보드 슬롯에 생성
        /// </summary>
        /// <param name="monsterCards"></param>
        public void CreateMonsterBoardSlots(List<Card> monsterCards)
        {
            for (int i = 0; i < monsterCards.Count; i++)
            {
                Card monsterCard = monsterCards[i];
                BoardSlot slot = rightSlots[i];

                slot.EquipCard(monsterCard);
            }
        }

        /// <summary>
        /// 턴 카운트 설정
        /// </summary>
        /// <param name="count"></param>
        public void SetTurnCount(int count)
        {
            boardUI.SetTurnCount(count);
        }

        /// <summary>
        /// 마스터 마나 충전
        /// </summary>
        /// <param name="master"></param>
        public void ChargeMana(Master master)
        {
            master.ChargeMana();

            boardSlots[0].SetMana(master.Mana);
        }

        /// <summary>
        /// 마스터 마나 초기화
        /// </summary>
        /// <param name="master"></param>
        public void ResetMana(Master master)
        {
            master.ResetMana();

            boardSlots[0].SetMana(master.Mana);
        }

        /// <summary>
        /// 마스터 마나 설정
        /// </summary>
        /// <param name="master"></param>
        /// <param name="mana"></param>
        public void AddMana(Master master, int mana)
        {
            master.AddMana(mana);

            boardSlots[0].SetMana(master.Mana);
        }

        /// <summary>
        /// 마스터 골드 설정
        /// </summary>
        /// <param name="master"></param>
        /// <param name="gold"></param>
        public void AddGold(Master master, int gold)
        {
            master.AddGold(gold);
            boardUI.SetGold(master.Gold);
        }

        /// <summary>
        /// 카드 비용 소모
        /// </summary>
        /// <param name="master"></param>
        /// <param name="card"></param>
        public void CardCost(Master master, Card card)
        {
            if (card.costMana > 0)
                AddMana(master, -card.costMana);

            if (card.costGold > 0)
                AddGold(master, -card.costGold);
        }

        /// <summary>
        /// 보드 슬롯에서 카드 제거
        /// </summary>
        public void RemoveCard(string cardUid)
        {
            foreach (var boardSlot in boardSlots)
            {
                if (boardSlot.Card != null && boardSlot.Card.Uid == cardUid)
                {
                    Debug.Log($"boardSlot: {boardSlot.Slot} RemoveCard: {boardSlot.Card.Id}");
                    boardSlot.RemoveCard();
                    break;
                }
            }

            // foreach (var boardSlot in leftSlots)
            //     Debug.Log($"leftSlot: {boardSlot.Slot} 확인 카드: {boardSlot?.Card?.id ?? 0}");

            // foreach (var boardSlot in rightSlots)
            //     Debug.Log($"leftSlot: {boardSlot.Slot} 확인 카드: {boardSlot?.Card?.id ?? 0}");
        }

        public BoardSlot GetBoardSlot(int slot)
        {
            return boardSlots.Find(x => x.Slot == slot);
        }

        public List<BoardSlot> GetBoardSlots(List<int> slots)
        {
            return boardSlots.FindAll(x => slots.Contains(x.Slot));
        }

        public BoardSlot GetBoardSlot(string cardUid)
        {
            return boardSlots.Find(x => x.Card != null && x.Card.Uid == cardUid);
        }

        /// <summary>
        /// 크리쳐 보드 슬롯 인덱스로 정렬
        /// - 슬롯 3, 2, 1, 0 순서
        /// </summary>
        public List<BoardSlot> GetCreatureBoardSlots()
        {
            return leftSlots.OrderBy(slot => slot.Index).ToList();
        }

        /// <summary>
        /// - 슬롯 6, 7, 8, 9 순서
        /// </summary>
        public List<BoardSlot> GetMonsterBoardSlots()
        {
            return rightSlots.OrderBy(slot => slot.Index).ToList();
        }

        public List<BoardSlot> GetBuildingBoardSlots()
        {
            return buildingSlots;
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
        public List<Card> GetOpponetCards(Card self)
        {
            if (self.Type == CardType.Creature || self.Type == CardType.Master)
                return GetOccupiedMonsterCards();

            if (self.Type == CardType.Monster || self.Type == CardType.EnemyMaster)
                return GetOccupiedCreatureCards();

            return null;
        }

        /// <summary>
        /// 슬롯으로 상대 슬롯 리스트 얻기
        /// </summary>
        public List<BoardSlot> GetOpponentSlots(BoardSlot self)
        {
            if (self.CardType == CardType.Creature || self.CardType == CardType.Master)
                return GetMonsterBoardSlots();

            if (self.CardType == CardType.Monster || self.CardType == CardType.EnemyMaster)
                return GetCreatureBoardSlots();

            return new List<BoardSlot>();
        }

        /// <summary>
        /// 슬롯으로 친구 슬롯 리스트 얻기
        /// </summary>
        public List<BoardSlot> GetFriendlySlots(BoardSlot self)
        {
            if (self.CardType == CardType.Creature || self.CardType == CardType.Master)
                return GetCreatureBoardSlots();

            if (self.CardType == CardType.Monster || self.CardType == CardType.EnemyMaster)
                return GetMonsterBoardSlots();

            return new List<BoardSlot>();
        }

        /// <summary>
        /// 가장 가까운 보드 슬롯 찾기
        /// </summary>
        /// <param name="position"></param>
        public BoardSlot NeareastBoardSlot(Vector3 position)
        {
            BoardSlot nearestSlot = null;
            float minDistance = float.MaxValue;

            List<BoardSlot> boardSlots = leftSlots.Concat(rightSlots).Concat(buildingSlots).ToList();

            foreach (BoardSlot slot in boardSlots)
            {
                if (slot.IsOverlapCard == false)
                    continue;

                float distance = Vector3.Distance(position, slot.transform.position);

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                nearestSlot = slot;
            }

            return nearestSlot;
        }
    }
}