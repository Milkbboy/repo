using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using System;

namespace ERang
{
    public class BoardSystem : MonoBehaviour
    {
        public static BoardSystem Instance { get; private set; }

        public readonly CardType[] leftSlotCardTypes = { CardType.Master, CardType.Creature, CardType.Creature, CardType.Creature, CardType.None };
        public readonly CardType[] rightSlotCardTypes = { CardType.None, CardType.Monster, CardType.Monster, CardType.Monster, CardType.Master };
        public readonly CardType[] buildingSlotCardTypes = { CardType.Building, CardType.Building, CardType.None, CardType.None };

        public List<BoardSlot> AllSlots => boardSlots;

        public MasterCard MasterCard => masterCard;
        public BoardSlot bSlotPrefab;

        private BoardUI boardUI;
        private readonly List<BoardSlot> boardSlots = new();
        private readonly List<BoardSlot> leftBSlots = new();
        private readonly List<BoardSlot> rightBSlots = new();
        private readonly List<BoardSlot> buildingSlots = new();
        private MasterCard masterCard;

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
        public void CreateBoardSlots(int creatureSlotCount)
        {
            int totalBoardSlotCount = leftSlotCardTypes.Length + rightSlotCardTypes.Length; ;
            int leftSlotStartIndex = 4;
            int rightSlotStartIndex = 5;

            // left 슬롯 구성
            for (int i = 0; i < leftSlotCardTypes.Length; ++i)
            {
                CardType cardType = CardType.None;

                if (i == 0)
                {
                    cardType = CardType.Master;
                }
                else
                {
                    if (creatureSlotCount > 0)
                        cardType = CardType.Creature;

                    creatureSlotCount--;
                }

                // 새로운 슬롯
                BoardSlot bSlot = Instantiate(bSlotPrefab);
                bSlot.name = $"Slot_{i}_{leftSlotStartIndex - i}";
                bSlot.CreateSlot(i, leftSlotStartIndex - i, cardType);

                boardSlots.Add(bSlot);
                leftBSlots.Add(bSlot);
            }

            // right 슬롯 구성
            for (int i = 0; i < rightSlotCardTypes.Length; ++i)
            {
                int slotNum = i + rightSlotCardTypes.Length;
                CardType cardType = rightSlotCardTypes[i];

                BoardSlot bSlot = Instantiate(bSlotPrefab);
                bSlot.name = $"Slot_{slotNum}_{slotNum - rightSlotStartIndex}";
                bSlot.CreateSlot(slotNum, slotNum - rightSlotStartIndex, cardType);

                boardSlots.Add(bSlot);
                rightBSlots.Add(bSlot);
            }

            float boardSpacing = 0f;
            float boardWidth = bSlotPrefab.GetComponent<BoxCollider>().size.x * bSlotPrefab.transform.localScale.x;
            float totalWidth = (totalBoardSlotCount - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            // 보드 슬롯 위치 설정
            for (int i = 0; i < boardSlots.Count; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new(xPosition, bSlotPrefab.transform.position.y, bSlotPrefab.transform.position.z);

                boardSlots[i].transform.position = slotPosition;
                // Debug.Log($"Slot: {boardSlots[i].Slot} Index: {boardSlots[i].Index} Type: {boardSlots[i].CardType}");
            }

            // 빌딩 보드 슬롯 구성
            startX = -5f;
            float startY = 3f;

            for (int i = 0; i < buildingSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new(xPosition, startY, bSlotPrefab.transform.position.z);

                BoardSlot bSlot = Instantiate(bSlotPrefab, slotPosition, Quaternion.identity);
                bSlot.name = $"Building_{i}";
                bSlot.CreateSlot(i, i, buildingSlotCardTypes[i]);

                buildingSlots.Add(bSlot);
            }
        }

        public IEnumerator CreateMasterCard(Player player)
        {
            masterCard = new(player);

            yield return null;

            boardSlots[0].EquipCard(masterCard);
        }

        /// <summary>
        /// 몬스터 카드 보드 슬롯에 생성
        /// </summary>
        /// <param name="monsterCards"></param>
        public IEnumerator CreateMonsterCards(List<int> cardIds)
        {
            Debug.Log($"몬스터 카드들 {string.Join(", ", cardIds)} 생성");

            List<(int, BaseCard)> monsterCards = new();
            CardFactory cardFactory = new(AiLogic.Instance);

            for (int i = 0; i < cardIds.Count; ++i)
            {
                int cardId = cardIds[i];

                if (cardId == 0)
                    continue;

                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"BoardSystem - CreateMonsterCards. CardData({cardId}) 데이터 없음");
                    continue;
                }

                BaseCard card = cardFactory.CreateCard(cardData);

                // 크리쳐 카드 슬롯 인덱스는 1부터 시자되므로 1을 더함
                monsterCards.Add((i + 1, card));
            }

            // 한 프레임 기다림. slot.EquipCard 함수 내용 중 CardUI 의 cardObject.SetActive(true) 적용을 위한 대기
            // 이렇게 안하면 BSlot 의 card 게임 오브젝트가 활성화되지 않아 카드가 보이지 현상 발생
            // - 타이밍 이슈는 종종 게임 오브젝트가 생성되거나 초기화되는 시점과 관련이 있습니다. 
            //   특히, Unity에서 게임 오브젝트를 생성하고 나서 바로 사용하려고 할 때, 아직 오브젝트가 완전히 초기화되지 않았거나, 
            //   렌더링 시스템이 업데이트되지 않았을 수 있습니다. 이로 인해 오브젝트가 화면에 제대로 표시되지 않을 수 있습니다.
            yield return null;

            foreach (var (slotNum, card) in monsterCards)
            {
                BoardSlot slot = rightBSlots.Find(slot => slot.Index == slotNum);

                if (slot == null)
                {
                    Debug.LogWarning($"몬스터 카드 슬롯 없음. SlotNum: {slotNum}");
                    continue;
                }

                slot.EquipCard(card);
            }
        }

        public void RefreshBoardSlot()
        {
            foreach (var slot in boardSlots)
            {
                BaseCard card = slot.Card;

                if (card == null)
                    continue;

                slot.EquipCard(card);
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

        public void SetHp(int amount)
        {
            boardSlots[0].SetHp(amount);
        }

        public void SetMana(int amount)
        {
            boardSlots[0].SetMana(amount);
        }

        /// <summary>
        /// 마스터 마나 초기화
        /// </summary>
        public void ResetMana()
        {
            boardSlots[0].ResetMana();
        }

        /// <summary>
        /// 마스터 마나 설정
        /// </summary>
        /// <param name="mana"></param>
        public void AddMana(int amount)
        {
            if (amount > 0)
                boardSlots[0].IncreaseMana(amount);
            else
                boardSlots[0].DecreaseMana(Math.Abs(amount));
        }

        /// <summary>
        /// 마스터 골드 설정
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gold"></param>
        public void AddGold(Player player, int gold)
        {
            player.AddGold(gold);
            boardUI.SetGold(player.Gold);
        }

        public void SetGold(int gold)
        {
            boardUI.SetGold(gold);
        }

        /// <summary>
        /// 카드 비용 소모
        /// </summary>
        /// <param name="player"></param>
        /// <param name="card"></param>
        public void CardCost(Player player, BaseCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("카드 비용 소모 처리 불가: 카드가 없음");
                return;
            }

            if (card.Mana > 0)
            {
                AddMana(-card.Mana);
            }

            if (card is BuildingCard buildingCard && buildingCard.Gold > 0)
            {
                AddGold(player, -buildingCard.Gold);
            }
        }

        public BoardSlot GetMasterSlot()
        {
            return boardSlots[Constants.MasterSlotNumber];
        }

        public BoardSlot GetEnemyMasterSlot()
        {
            return boardSlots[Constants.EnemyMasterSlotNumber];
        }

        public BoardSlot GetBoardSlot(int slotNum)
        {
            return boardSlots.Find(x => x.SlotNum == slotNum);
        }

        public List<BoardSlot> GetBoardSlots(List<int> slots)
        {
            return boardSlots.FindAll(x => slots.Contains(x.SlotNum));
        }

        public BoardSlot GetBoardSlot(string cardUid)
        {
            return boardSlots.Find(x => x.Card != null && x.Card.Uid == cardUid);
        }

        /// <summary>
        /// 크리쳐 보드 슬롯 인덱스로 정렬
        /// - 슬롯 3, 2, 1, 0 순서
        /// </summary>
        public List<BoardSlot> GetLeftBoardSlots()
        {
            return leftBSlots.OrderBy(slot => slot.Index).ToList();
        }

        /// <summary>
        /// - 슬롯 6, 7, 8, 9 순서
        /// </summary>
        public List<BoardSlot> GetRightBoardSlots()
        {
            return rightBSlots.OrderBy(slot => slot.Index).ToList();
        }

        public List<BoardSlot> GetBuildingBoardSlots()
        {
            return buildingSlots;
        }

        public BoardSlot GetEnemySlotPos(int pos)
        {
            return boardSlots.Find(x => x.Index == pos);
        }

        /// <summary>
        /// 크리쳐 카드가 장착된 보드 슬롯 인덱스를 정렬한 카드 반화
        /// - 슬롯 인덱스 3, 2, 1 순서
        /// </summary>
        /// <returns></returns>
        public List<BaseCard> GetOccupiedCreatureCards()
        {
            return GetLeftBoardSlots()
                .Where(slot => slot.Card != null)
                .Select(slot => slot.Card)
                .ToList();
        }

        /// <summary>
        /// 몬스터 카드가 장착된 보드 슬롯 인덱스를 정렬한 카드 반환
        /// - 슬롯 인덱스 6, 7, 8 순서
        /// </summary>
        public List<BaseCard> GetOccupiedMonsterCards()
        {
            return GetRightBoardSlots()
                .Where(slot => slot.Card != null)
                .Select(slot => slot.Card)
                .ToList();
        }

        /// <summary>
        /// 슬롯으로 상대 슬롯 리스트 얻기
        /// </summary>
        public List<BoardSlot> GetOpponentSlots(BoardSlot self)
        {
            if (self.SlotCardType == CardType.Creature || self.SlotCardType == CardType.Master)
                return GetRightBoardSlots();

            if (self.SlotCardType == CardType.Monster || self.SlotCardType == CardType.EnemyMaster)
                return GetLeftBoardSlots();

            return new List<BoardSlot>();
        }

        /// <summary>
        /// 슬롯으로 친구 슬롯 리스트 얻기
        /// </summary>
        public List<BoardSlot> GetFriendlySlots(BoardSlot self)
        {
            if (self.SlotCardType == CardType.Creature || self.SlotCardType == CardType.Master)
                return GetLeftBoardSlots();

            if (self.SlotCardType == CardType.Monster || self.SlotCardType == CardType.EnemyMaster)
                return GetRightBoardSlots();

            return new List<BoardSlot>();
        }

        public List<BoardSlot> GetAllSlots()
        {
            return boardSlots;
        }
    }
}