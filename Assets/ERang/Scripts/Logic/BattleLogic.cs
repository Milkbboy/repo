using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ERang.Data;
using UnityEngine.Events;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }

        public HandDeck handDeck;
        public int turn = 1;
        public int masterId = 1001;
        public int handCardCount = 5;

        public Master master { get; private set; }
        public Enemy enemy { get; private set; }

        private System.Random random = new System.Random();

        // for test
        private Queue<NamedAction> actionQueue = new Queue<NamedAction>();
        private List<BoardSlot> flashingSlots = new List<BoardSlot>();

        void Awake()
        {
            Instance = this;

            // 마스터 설정 - 시작 카드 생성
            master = new Master(masterId);

            // 적 설정 - 더미
            enemy = new Enemy(1002);
            // Debug.Log($"enemyId: {enemy.enemyId}, hp: {enemy.hp}, atk: {enemy.atk}, cards: {enemy.monsterCards.Count}");
        }

        // Start is called before the first frame update
        void Start()
        {
            // 보드 설정 - 덱 카운트
            Board.Instance.SetDeckCount(master.deckCards.Count);
            Board.Instance.CreateBoardSlots();
            Board.Instance.CreateMonsterCard();

            // TurnStart();
            TurnStart();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"Space key down. 큐 개수: {actionQueue.Count}");

                if (flashingSlots.Count > 0)
                {
                    foreach (BoardSlot slot in flashingSlots)
                        slot.StopFlashing();

                    flashingSlots.Clear();
                }

                if (actionQueue.Count > 0)
                {
                    var namedAction = actionQueue.Dequeue();
                    Debug.Log($"<color=#dd3333>Execution action</color>: {namedAction.Name} 실행");
                    namedAction.Action?.Invoke();
                }
            }
        }

        public Master GetMaster()
        {
            return master;
        }

        public Enemy GetEnemy()
        {
            return enemy;
        }

        void TurnStart()
        {
            // deck 카드 섞기
            ShuffleDeck(master.deckCards);

            // draw staring card
            StartCoroutine(DrawHandDeckCard(handCardCount));

            // 보드 설정
            Board.Instance.SetTurnCount(turn);

            // 카드 어빌리티 실행
            AbilityAction();

            // 턴 시작시 실행되는 카드의 Reaction 을 확인
            // EnqueueAction("TurnStartCardReaction", () => TurnStartCardReaction());
            TurnStartReaction();
        }

        public void TurnEnd()
        {
            if (actionQueue.Count > 0)
            {
                ToastNotification.Show($"actionQueue remain action: {actionQueue.Count}");
                return;
            }

            turn += 1;

            // 핸드덱에 카드 제거
            for (int i = 0; i < master.handCards.Count; ++i)
            {
                HandDeck.Instance.RemoveCard(master.handCards[i].uid);
                Master.Instance.HandCardToGrave(master.handCards[i].uid);
            }

            // 마스터 데이타 설정 - 마나 증가
            master.chargeMana();

            // 보드 - 마스터 마나 설정
            Board.Instance.SetMasterMana(master.Mana);
            // 보드 - 턴 카운트
            Board.Instance.SetTurnCount(turn);

            // 보드 슬롯 카드 동작
            MasterCreatureAction();
            EnemyMonsterAction();
            BuildingCardAction();

            // 턴 다시 시작
            TurnStart();
        }

        /// <summary>
        /// 보드에 있는 카드들 어빌리티 실행
        /// </summary>
        void AbilityAction()
        {
            // 내 카드 어빌리티
            List<Card> creatureCards = Board.Instance.GetOccupiedCreatureCards();
            CardAbilityAction(creatureCards);

            // 몬스터 카드 어빌리티
            List<Card> monsterCards = Board.Instance.GetOccupiedMonsterCards();
            CardAbilityAction(monsterCards);
        }

        /// <summary>
        /// // 카드 어빌리티 실행
        /// </summary>
        /// <param name="cards"></param>
        void CardAbilityAction(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                EnqueueAction($"카드({card.id}) !! 어빌리티 액션 !!", () =>
                {
                    // 현재 카드 보드 슬롯 깜빡임 설정
                    flashingCard(card.uid);

                    AiLogic.Instance.AbilityAction(card);
                });
            }
        }

        void TurnStartReaction()
        {
            List<BoardSlot> reactionSlots = Board.Instance.GetMonsterBoardSlots();
            List<BoardSlot> opponentSlots = Board.Instance.GetCreatureBoardSlots();

            foreach (BoardSlot reactionSlot in reactionSlots)
            {
                EnqueueAction($"보드 {reactionSlot.Slot} 슬롯. -- 턴 시작 리액션 --", () =>
                {
                    // 현재 턴 보드 슬롯 깜빡임 설정
                    flashingCard(reactionSlot);

                    Card card = reactionSlot.Card;

                    if (card == null)
                    {
                        Debug.LogWarning($"{reactionSlot.Slot}번 슬롯. 장착된 카드가 없어 리액션 패스 - BattleLogic.TurnStartReaction");
                        return;
                    }

                    List<(AiGroupData.Reaction, ConditionData)> reactionPairs = card.GetCardReactionPairs(ConditionCheckPoint.TurnStart);

                    if (reactionPairs.Count == 0)
                    {
                        Debug.LogWarning($"{reactionSlot.Slot}번 슬롯 카드({card.id}). AiGroupData({card.aiGroupId})에 해당하는 <color=red>리액션 데이터 없음</color> - BattleLogic.TurnStartReaction");
                        return;
                    }

                    foreach (var (reaction, condition) in reactionPairs)
                    {
                        // Debug.Log($"{reactionSlot.Slot}번 슬롯. 카드({card.id}) 리액션 컨디션({condition.id}) 확인 함수 호출 - BattleLogic.TurnStartReaction");

                        var (aiDataId, targetSlots) = ConditionLogic.Instance.GetReactionConditionAiDataId((reaction, condition), reactionSlot, opponentSlots);

                        if (aiDataId == 0)
                        {
                            // Debug.LogWarning($"{reactionSlot.Slot}번 슬롯. 카드({card.id}) 리액션 컨디션({condition.id}) 없음 - BattleLogic.TurnStartReaction");
                            continue;
                        }

                        AiData aiData = AiData.GetAiData(aiDataId);

                        string aiGroupDataTableLog = $"{reactionSlot.Slot}번 슬롯 {card.id} 카드. <color=#78d641>AiData</color> 테이블 {aiDataId} 데이터 얻기";

                        if (aiData == null)
                        {
                            Debug.LogWarning($"{aiGroupDataTableLog} - 실패. <color=red>테이블에 데이터 없음</color> - BattleLogic.TurnStartReaction");
                            continue;
                        }

                        Debug.Log($"{aiGroupDataTableLog} 성공 - {reactionSlot.Slot}번 슬롯 카드({card.id}). 리액션 컨디션({condition.id}) AiData({aiDataId}) 작동 - BattleLogic.TurnStartReaction");

                        // 타겟 슬롯 표시를 하기 위함인데 원하는대로 되지 않아 수정할 예정
                        foreach (int targetSlot in targetSlots)
                        {
                            Debug.Log($"{targetSlot}번 타겟 슬롯 깜박임 시작 - BattleLogic.TurnStartReaction");

                            BoardSlot targetBoardSlot = Board.Instance.GetBoardSlot(targetSlot);

                            targetBoardSlot.StartFlashing(Color.red);
                            flashingSlots.Add(targetBoardSlot);
                        }

                        // 리액션 실행되면 다음 리액션은 패스
                        AiLogic.Instance.AiDataAction(aiData, reactionSlot);
                        break;
                    }
                });
            }
        }

        void MasterCreatureAction()
        {
            List<BoardSlot> creatureSlots = Board.Instance.GetCreatureBoardSlots();
            List<BoardSlot> monsterSlots = Board.Instance.GetMonsterBoardSlots();

            BoardCardAction(creatureSlots, monsterSlots);
        }

        void EnemyMonsterAction()
        {
            List<BoardSlot> monsterSlots = Board.Instance.GetMonsterBoardSlots();
            List<BoardSlot> creatureSlots = Board.Instance.GetCreatureBoardSlots();

            BoardCardAction(monsterSlots, creatureSlots);
        }

        void BoardCardAction(List<BoardSlot> actorSlots, List<BoardSlot> opponentSlots)
        {
            foreach (BoardSlot actorSlot in actorSlots)
            {
                EnqueueAction($"{actorSlot.Slot}번 슬롯 ** 턴 종료 액션 **", () =>
                {
                    // 현재 턴 보드 슬롯 깜빡임 설정
                    flashingCard(actorSlot);

                    Card card = actorSlot.Card;

                    if (card == null)
                    {
                        Debug.LogWarning($"{actorSlot.Slot}번 슬롯 장착된 카드가 없어 액션 패스 - BattleLogic.BoardCardAction");
                        return;
                    }

                    // 카드의 행동 aiData 설정
                    int aiDataId = card.GetCardAiDataId(actorSlot.Slot);

                    if (aiDataId == 0)
                    {
                        Debug.LogWarning($"{actorSlot.Slot}번 슬롯 카드({card.id}). AiGroupData({card.aiGroupId})에 해당하는 <color=red>액션 데이터 없음<color> - BattleLogic.BoardCardAction");
                        return;
                    }

                    // ai 실행
                    AiData aiData = AiData.GetAiData(aiDataId);

                    AiLogic.Instance.AiDataAction(aiData, actorSlot);
                });
            }
        }

        void BuildingCardAction()
        {
            // Debug.Log($"BuildingCardAction: {Board.Instance.GetBuildingSlots().Count}");
            foreach (BoardSlot buildingSlot in Board.Instance.GetBuildingSlots())
            {
                EnqueueAction($"<color=#dd9933>건물</color> {buildingSlot.Slot}번 슬롯 ** 턴 종료 액션 **", () =>
                {
                    // 현재 턴 보드 슬롯 깜빡임 설정
                    flashingCard(buildingSlot);

                    Card card = buildingSlot.Card;

                    if (card == null)
                    {
                        Debug.LogWarning($"건물 {buildingSlot.Slot}번 슬롯 장착된 카드가 없어 패스 - BattleLogic.BuildingCardAction");
                        return;
                    }

                    // 카드의 행동 aiData 설정
                    int aiDataId = card.GetCardAiDataId(buildingSlot.Slot);

                    if (aiDataId == 0)
                    {
                        Debug.LogWarning($"건물 {buildingSlot.Slot}번 슬롯 카드({card.id}). AiGroupData({card.aiGroupId})에 해당하는 <color=red>액션 데이터 없음<color> - BattleLogic.BuildingCardAction");
                        return;
                    }

                    // ai 실행
                    AiData aiData = AiData.GetAiData(aiDataId);

                    AiLogic.Instance.AiDataAction(aiData, buildingSlot);
                });
            }
        }

        // 카드 섞기
        void ShuffleDeck(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; ++i)
            {
                Card temp = cards[i];
                int randomIdex = random.Next(i, cards.Count);
                cards[i] = cards[randomIdex];
                cards[randomIdex] = temp;
            }
        }

        // 카드 그리기
        IEnumerator DrawHandDeckCard(int cardCount)
        {
            int spawnCount = cardCount - master.handCards.Count;

            // Debug.Log($"DrawHandDeckCard. masterHandCardCount: {master.handCards.Count}, spawnCount: {spawnCount}");

            for (int i = 0; i < spawnCount; ++i)
            {
                if (master.deckCards.Count == 0)
                {
                    // 덱에 카드가 없으면 무덤에 있는 카드를 덱으로 옮김
                    master.deckCards.AddRange(master.graveCards);
                    master.graveCards.Clear();

                    // 덱 카드 섞기
                    ShuffleDeck(master.deckCards);
                }

                if (master.deckCards.Count > 0)
                {
                    Card card = master.deckCards[0];

                    // 덱에서 카드를 뽑아 손에 추가
                    master.deckCards.RemoveAt(0);
                    master.handCards.Add(card);

                    HandDeck.Instance.SpawnNewCard(card);
                    HandDeck.Instance.DrawCards();
                }

                yield return new WaitForSeconds(.2f);
            }

            // 보드 설정 - 덱 카운트
            Board.Instance.SetDeckCount(master.deckCards.Count);
            // 보드 설정 - 덱 카운트
            Board.Instance.SetGraveDeckCount(master.graveCards.Count);
        }

        public bool CanHandCardUse(string cardUid)
        {
            Card card = master.GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"CanUseCard: card is null({card.id})");
                return false;
            }

            if (master.Mana < card.costMana)
            {
                ToastNotification.Show($"mana is not enough({master.Mana})");
                Debug.LogError($"CanUseCard: mana is not enough({card.id}, {master.Mana} < {card.costMana})");
                return false;
            }

            return true;
        }

        public void BoardSlotEquipCard(BoardSlot boardSlotRef, string cardUid)
        {
            Card card = master.GetHandCard(cardUid);

            // BoardSlot 에 카드 장착
            boardSlotRef.EquipCard(card);

            // HandDeck 에서 카드 제거
            HandDeck.Instance.RemoveCard(cardUid);

            // Master 의 handCards => boardCreatureCards or boardBuildingCards 로 이동
            master.HandCardToBoard(cardUid, card.type);

            // Master 의 mana 감소
            master.DecreaseMana(card.costMana);

            Board.Instance.SetMasterMana(master.Mana);

            Debug.Log($"BoardSlotEquipCard: {card.id}, BoardSlot: {boardSlotRef.Slot}");
        }

        public void HandCardUse(string cardUid)
        {
            // HandDeck 에서 카드 제거
            HandDeck.Instance.RemoveCard(cardUid);

            Card card = master.GetHandCard(cardUid);

            if (card.isExtinction)
            {
                // Master 의 handCards => extinctionCards 로 이동
                master.HandCardToExtinction(cardUid);

                // 보드 설정 - 소멸덱 카운트
                Board.Instance.SetExtinctionDeckCount(master.extinctionCards.Count);
            }
            else
            {
                // Master 의 handCards => graveCards 로 이동
                master.HandCardToGrave(cardUid);

                // 보드 설정 - 그레이브덱 카운트
                Board.Instance.SetGraveDeckCount(master.graveCards.Count);
            }

            // Master 의 mana 감소
            master.DecreaseMana(card.costMana);

            Board.Instance.SetMasterMana(master.Mana);

            Debug.Log($"HandCardUsed: {cardUid}");
        }

        private void flashingCard(string cardUid)
        {
            BoardSlot boardSlot = Board.Instance.GetBoardSlot(cardUid);
            flashingCard(boardSlot);
        }

        private void flashingCard(BoardSlot boardSlot)
        {
            if (boardSlot == null)
                return;

            boardSlot.StartFlashing();
            flashingSlots.Add(boardSlot);
        }

        /// <summary>
        /// 이름이 있는 액션을 저장하는 클래스
        /// </summary>
        private class NamedAction
        {
            public string Name { get; }
            public UnityAction Action { get; }

            public NamedAction(string name, UnityAction action)
            {
                Name = name;
                Action = action;
            }
        }

        private void EnqueueAction(string name, UnityAction action)
        {
            Debug.Log($"<color=#257dca>EnqueueAction</color>: {name} 추가");
            actionQueue.Enqueue(new NamedAction(name, action));
        }
    }
}