using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class BoardSystem : MonoBehaviour
    {
        public static BoardSystem Instance { get; private set; }

        public readonly CardType[] leftSlotCardTypes = { CardType.Master, CardType.Creature, CardType.Creature, CardType.Creature, CardType.None };
        public readonly CardType[] rightSlotCardTypes = { CardType.None, CardType.Monster, CardType.Monster, CardType.Monster, CardType.Master };
        public readonly CardType[] buildingSlotCardTypes = { CardType.Building, CardType.Building, CardType.None, CardType.None };

        public BSlot bSlotPrefab;

        private BoardUI boardUI;
        private readonly List<BSlot> bSlots = new();
        private readonly List<BSlot> leftBSlots = new();
        private readonly List<BSlot> rightBSlots = new();
        private readonly List<BSlot> buildingSlots = new();

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

            // 마스터 보드 슬롯 구성
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
                BSlot bSlot = Instantiate(bSlotPrefab);
                bSlot.name = $"Slot_{i}_{leftSlotStartIndex - i}";
                bSlot.CreateSlot(i, leftSlotStartIndex - i, cardType);

                bSlots.Add(bSlot);
                leftBSlots.Add(bSlot);
            }

            // 몬스터 보드 슬롯 구성
            for (int i = 0; i < rightSlotCardTypes.Length; ++i)
            {
                int slotNum = i + rightSlotCardTypes.Length;
                CardType cardType = rightSlotCardTypes[i];

                BSlot bSlot = Instantiate(bSlotPrefab);
                bSlot.name = $"Slot_{slotNum}_{slotNum - rightSlotStartIndex}";
                bSlot.CreateSlot(slotNum, slotNum - rightSlotStartIndex, cardType);

                bSlots.Add(bSlot);
                rightBSlots.Add(bSlot);
            }

            float boardSpacing = 0.2f;
            float boardWidth = bSlotPrefab.GetComponent<BoxCollider>().size.x * bSlotPrefab.transform.localScale.x;
            float totalWidth = (totalBoardSlotCount - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            // 보드 슬롯 위치 설정
            for (int i = 0; i < bSlots.Count; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new(xPosition, bSlotPrefab.transform.position.y, bSlotPrefab.transform.position.z);

                bSlots[i].transform.position = slotPosition;
                // Debug.Log($"Slot: {boardSlots[i].Slot} Index: {boardSlots[i].Index} Type: {boardSlots[i].CardType}");
            }

            // 빌딩 보드 슬롯 구성
            startX = -5f;
            float startY = 2f;

            for (int i = 0; i < buildingSlotCardTypes.Length; i++)
            {
                float xPosition = startX + i * (boardWidth + boardSpacing);
                Vector3 slotPosition = new(xPosition, startY, bSlotPrefab.transform.position.z);

                BSlot bSlot = Instantiate(bSlotPrefab, slotPosition, Quaternion.identity);
                bSlot.CreateSlot(i, i, buildingSlotCardTypes[i]);

                buildingSlots.Add(bSlot);
            }
        }

        public IEnumerator CreateMasterCard(Master master)
        {
            MasterCard card = new(master);

            yield return null;

            bSlots[0].EquipCard(card);
        }

        /// <summary>
        /// 몬스터 카드 보드 슬롯에 생성
        /// </summary>
        /// <param name="monsterCards"></param>
        public IEnumerator CreateMonsterCards(List<int> cardIds)
        {
            Debug.Log($"몬스터 카드들 {string.Join(", ", cardIds)} 생성");

            List<(int, BaseCard)> monsterCards = new();

            for (int i = 0; i < cardIds.Count; ++i)
            {
                int cardId = cardIds[i];

                if (cardId == 0)
                    continue;

                CardData cardData = MonsterCardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음");
                    continue;
                }

                BaseCard card = Utils.MakeCard(cardData);

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
                BSlot slot = rightBSlots.Find(slot => slot.Index == slotNum);

                if (slot == null)
                {
                    Debug.LogWarning($"몬스터 카드 슬롯 없음. SlotNum: {slotNum}");
                    continue;
                }

                Debug.Log($"몬스터 카드 장착. Slot: {slot.SlotNum}, Index: {slot.Index}, CardId: {card.Id}");

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

        /// <summary>
        /// 마스터 마나 충전
        /// </summary>
        /// <param name="master"></param>
        public void ChargeMana(Master master)
        {
            master.ChargeMana();

            // if (bSlots.Count > 0)
            //     bSlots[0]?.SetMana(master.Mana);
        }

        /// <summary>
        /// 마스터 마나 초기화
        /// </summary>
        /// <param name="master"></param>
        public void ResetMana(Master master)
        {
            master.ResetMana();

            // bSlots[0].SetMana(master.Mana);
        }

        /// <summary>
        /// 마스터 마나 설정
        /// </summary>
        /// <param name="master"></param>
        /// <param name="mana"></param>
        public void AddMana(Master master, int mana)
        {
            // master.AddMana(mana);

            // if (bSlots.Count > 0)
            //     bSlots[0]?.SetMana(master.Mana);
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
        public void CardCost(Master master, BaseCard card)
        {
            switch (card)
            {
                case CreatureCard creatureCard:
                    AddMana(master, -creatureCard.Mana);
                    break;
                case MagicCard magicCard:
                    AddMana(master, -magicCard.Mana);
                    break;
                case BuildingCard buildingCard:
                    AddGold(master, -buildingCard.Gold);
                    break;
                default:
                    Debug.LogWarning("Unhandled card type");
                    break;
            }

            // if (card.costMana > 0)
            //     AddMana(master, -card.costMana);

            // if (card.costGold > 0)
            //     AddGold(master, -card.costGold);
        }

        /// <summary>
        /// 보드 슬롯에서 카드 제거
        /// </summary>
        public void RemoveCard(string cardUid)
        {
            foreach (var boardSlot in bSlots)
            {
                if (boardSlot.Card != null && boardSlot.Card.Uid == cardUid)
                {
                    Debug.Log($"boardSlot: {boardSlot.SlotNum} RemoveCard: {boardSlot.Card.Id}");
                    // boardSlot.RemoveCard();
                    break;
                }
            }

            // foreach (var boardSlot in leftSlots)
            //     Debug.Log($"leftSlot: {boardSlot.SlotNum} 확인 카드: {boardSlot?.Card?.id ?? 0}");

            // foreach (var boardSlot in rightSlots)
            //     Debug.Log($"leftSlot: {boardSlot.SlotNum} 확인 카드: {boardSlot?.Card?.id ?? 0}");
        }

        public BSlot GetBoardSlot(int slotNum)
        {
            return bSlots.Find(x => x.SlotNum == slotNum);
        }

        public List<BSlot> GetBoardSlots(List<int> slots)
        {
            return bSlots.FindAll(x => slots.Contains(x.SlotNum));
        }

        public BSlot GetBoardSlot(string cardUid)
        {
            return bSlots.Find(x => x.Card != null && x.Card.Uid == cardUid);
        }

        /// <summary>
        /// 크리쳐 보드 슬롯 인덱스로 정렬
        /// - 슬롯 3, 2, 1, 0 순서
        /// </summary>
        public List<BSlot> GetCreatureBoardSlots()
        {
            return leftBSlots.OrderBy(slot => slot.Index).ToList();
        }

        /// <summary>
        /// - 슬롯 6, 7, 8, 9 순서
        /// </summary>
        public List<BSlot> GetMonsterBoardSlots()
        {
            return rightBSlots.OrderBy(slot => slot.Index).ToList();
        }

        public List<BSlot> GetBuildingBoardSlots()
        {
            return buildingSlots;
        }

        /// <summary>
        /// 크리쳐 카드가 장착된 보드 슬롯 인덱스를 정렬한 카드 반화
        /// - 슬롯 인덱스 3, 2, 1 순서
        /// </summary>
        /// <returns></returns>
        public List<BaseCard> GetOccupiedCreatureCards()
        {
            return GetCreatureBoardSlots()
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
            return GetMonsterBoardSlots()
                .Where(slot => slot.Card != null)
                .Select(slot => slot.Card)
                .ToList();
        }

        /// <summary>
        /// 카드로 상대 카드 리스트 얻기
        /// </summary>
        /// <param name="self"></param>
        public List<BaseCard> GetOpponetCards(Card self)
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
        public List<BSlot> GetOpponentSlots(BSlot self)
        {
            if (self.SlotCardType == CardType.Creature || self.SlotCardType == CardType.Master)
                return GetMonsterBoardSlots();

            if (self.SlotCardType == CardType.Monster || self.SlotCardType == CardType.EnemyMaster)
                return GetCreatureBoardSlots();

            return new List<BSlot>();
        }

        /// <summary>
        /// 슬롯으로 친구 슬롯 리스트 얻기
        /// </summary>
        public List<BSlot> GetFriendlySlots(BSlot self)
        {
            if (self.SlotCardType == CardType.Creature || self.SlotCardType == CardType.Master)
                return GetCreatureBoardSlots();

            if (self.SlotCardType == CardType.Monster || self.SlotCardType == CardType.EnemyMaster)
                return GetMonsterBoardSlots();

            return new List<BSlot>();
        }

        public List<BSlot> GetAllSlots()
        {
            return bSlots;
        }
    }
}