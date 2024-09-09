using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using UnityEngine.Events;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }

        public HandDeck handDeck;
        public int turnCount = 1;
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

            StartCoroutine(TurnStart());
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

        public void TestMelee()
        {
            Debug.Log("TestMelee");

            // 근거리 공격
            BoardSlot meleeSlot = Board.Instance.GetBoardSlot(6);
            List<BoardSlot> meleeTargetSlots = Board.Instance.GetBoardSlots(new List<int> { 3, 2 });

            AiData meleeAiData = AiData.GetAiData(1001);
            BoardLogic.Instance.AbilityDamage(meleeAiData, meleeSlot, meleeTargetSlots);
        }

        public void TestRanged()
        {
            Debug.Log("TestRanged");

            // 원거리 공격
            BoardSlot rangedSlot = Board.Instance.GetBoardSlot(7);
            List<BoardSlot> rangedTargetSlots = Board.Instance.GetBoardSlots(new List<int> { 3, 2, 1 });

            AiData rangedAiData = AiData.GetAiData(1004);
            BoardLogic.Instance.AbilityDamage(rangedAiData, rangedSlot, rangedTargetSlots);
        }

        public Master GetMaster()
        {
            return master;
        }

        public Enemy GetEnemy()
        {
            return enemy;
        }

        private IEnumerator TurnStart()
        {
            // 보드 설정
            Board.Instance.SetTurnCount(turnCount);

            // 마나 충전
            EnqueueAction("턴 시작 마나 충전", () => { Board.Instance.ManaCharge(); });

            ToastNotification.Show($"!! TURN START !!({turnCount})");

            // deck 카드 섞기
            ShuffleDeck(master.deckCards);

            // draw staring card
            yield return StartCoroutine(DrawHandDeckCard(handCardCount));

            // 핸드 카드 HandOn 어빌리티 액션
            HandOnCardAbilityAction(master.handCards);

            // 지속 시간 종료 어빌리티 실행
            DurationAbilityAction();

            // 턴 시작시 실행되는 카드의 Reaction 을 확인
            TurnStartReaction();
        }

        public void TurnEnd()
        {
            ToastNotification.Show($"!! TURN END !!({turnCount})");

            if (actionQueue.Count > 0)
            {
                ToastNotification.Show($"actionQueue remain action: {actionQueue.Count}");
                return;
            }

            // 보드 - 턴 카운트
            turnCount += 1;
            Board.Instance.SetTurnCount(turnCount);

            // 핸드덱에 카드 제거
            for (int i = 0; i < master.handCards.Count; ++i)
            {
                HandDeck.Instance.RemoveCard(master.handCards[i].uid);
                Master.Instance.HandCardToGrave(master.handCards[i].uid);
            }

            // 마스터 마나 리셋
            EnqueueAction("턴 종료 마나 리셋", () => { Board.Instance.ManaReset(); });

            // 핸드 온 카드 어빌리티 해제
            HandOnCardAbilityCut();

            // 보드 슬롯 카드 동작
            MasterCreatureAction();
            EnemyMonsterAction();
            BuildingCardAction();

            // 턴 다시 시작
            StartCoroutine(TurnStart());
        }

        /// <summary>
        /// 핸드에 카드 추가
        /// </summary>
        IEnumerator DrawHandDeckCard(int cardCount)
        {
            int spawnCount = cardCount - master.handCards.Count;

            Debug.Log($"DrawHandDeckCard. masterHandCardCount: {master.handCards.Count}, spawnCount: {spawnCount}");

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

        /// <summary>
        /// 핸드에 있을때 효과가 발동되는 카드 액션
        /// </summary>
        void HandOnCardAbilityAction(List<Card> handCards)
        {
            // 핸드 온 카드 액션의 주체는 마스터 슬롯
            BoardSlot masterSlot = Board.Instance.GetMasterSlot();
            List<(Card card, AiData aiData, List<AbilityData> abilityDatas)> handOnCards = AiLogic.Instance.GetHandOnCards(handCards);

            foreach (var handOnCard in handOnCards)
            {
                EnqueueAction($"핸드에 있는 카드({handOnCard.card.id}) !! 핸드온 어빌리티 액션 !!", () =>
                {
                    List<BoardSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(handOnCard.aiData, masterSlot);

                    AbilityLogic.Instance.HandOnAbilityAction(handOnCard.aiData, masterSlot, targetSlots, handOnCard.abilityDatas);
                });
            }
        }

        /// <summary>
        /// 지속시간(duration) 이 0되는 카드 어빌리티 실행
        /// </summary>
        void DurationAbilityAction()
        {
            List<AbilityLogic.Ability> abilities = AbilityLogic.Instance.GetDurationAbilities();

            foreach (AbilityLogic.Ability ability in abilities)
            {
                // 지속 시간이 0이 되는 어빌리티 실행
                ability.duration = ability.duration - 1;

                if (ability.whereFrom == AbilityWhereFrom.TurnStarHandOn || ability.duration > 0)
                    continue;

                EnqueueAction($"어빌리티({ability.abilityId}) !! 지속시간 종료 확인 !!", () =>
                {
                    BoardSlot boardSlot = Board.Instance.GetBoardSlot(ability.ownerBoardSlot);

                    AbilityLogic.Instance.AbilityAction(boardSlot, ability);
                });
            }
        }

        void TurnStartReaction()
        {
            List<BoardSlot> reactionSlots = Board.Instance.GetMonsterBoardSlots();
            List<BoardSlot> opponentSlots = Board.Instance.GetCreatureBoardSlots();

            foreach (BoardSlot reactionSlot in reactionSlots)
            {
                if (reactionSlot.Card == null)
                {
                    // Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} 장착된 카드가 없어 리액션 패스 - BattleLogic.TurnStartReaction");
                    return;
                }

                EnqueueAction($"보드 {reactionSlot.Slot} 슬롯. -- 턴 시작 리액션 --", () =>
                {
                    // 현재 턴 보드 슬롯 깜빡임 설정
                    flashingCard(reactionSlot);

                    Card card = reactionSlot.Card;

                    if (card == null)
                    {
                        Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} 장착된 카드가 없어 리액션 패스 - BattleLogic.TurnStartReaction");
                        return;
                    }

                    List<(AiGroupData.Reaction, ConditionData)> reactionPairs = card.GetCardReactionPairs(ConditionCheckPoint.TurnStart);

                    if (reactionPairs.Count == 0)
                    {
                        Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} AiGroupData({card.aiGroupId})에 해당하는 <color=red>리액션 데이터 없음</color> - BattleLogic.TurnStartReaction");
                        return;
                    }

                    foreach (var (reaction, condition) in reactionPairs)
                    {
                        //reactionTargetSlots 은 화면 표시를 위해 사용 - 실제 타겟은 AiData 에서 얻음
                        var (aiDataId, reactionTargetSlots) = ConditionLogic.Instance.GetReactionConditionAiDataId((reaction, condition), reactionSlot, opponentSlots);

                        if (aiDataId == 0)
                        {
                            Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} 리액션 컨디션({condition.id}) 없음 - BattleLogic.TurnStartReaction");
                            continue;
                        }

                        AiData aiData = AiData.GetAiData(aiDataId);

                        string aiGroupDataTableLog = $"{Utils.BoardSlotLog(reactionSlot)} <color=#78d641>AiData</color> 테이블 {aiDataId} 데이터 얻기";

                        if (aiData == null)
                        {
                            Debug.LogWarning($"{aiGroupDataTableLog} - 실패. <color=red>테이블에 데이터 없음</color> - BattleLogic.TurnStartReaction");
                            continue;
                        }

                        // Debug.Log($"{aiGroupDataTableLog} 성공 - {Utils.BoardSlotLog(reactionSlot)}. 리액션 컨디션({condition.id}) AiData({aiDataId}) 작동 - BattleLogic.TurnStartReaction");

                        // 리액션하는 슬롯 표시
                        foreach (int targetSlot in reactionTargetSlots)
                        {
                            // Debug.Log($"{targetSlot}번 타겟 슬롯 깜박임 시작 - BattleLogic.TurnStartReaction");
                            BoardSlot targetBoardSlot = Board.Instance.GetBoardSlot(targetSlot);

                            targetBoardSlot.StartFlashing(Color.red);
                            flashingSlots.Add(targetBoardSlot);
                        }

                        // 리액션 실행되면 다음 리액션은 패스
                        // AiData 에 설정된 타겟 얻기
                        List<BoardSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, reactionSlot);

                        if (aiTargetSlots.Count == 0)
                        {
                            Debug.LogWarning($"{Utils.BoardSlotLog(reactionSlot)} 설정 타겟({aiData.target}) 없음 - BattleLogic.TurnStartReaction");
                            return;
                        }

                        // 어빌리티 적용
                        AbilityLogic.Instance.SetBoardSlotAbility(AbilityWhereFrom.TurnStartReaction, aiData, reactionSlot, aiTargetSlots);
                        break;
                    }
                });
            }
        }

        void HandOnCardAbilityCut()
        {
            List<AbilityLogic.Ability> handOnAbilities = AbilityLogic.Instance.GetHandOnAbilities();

            if (handOnAbilities.Count > 0)
            {
                foreach (var handOnAbility in handOnAbilities)
                {
                    EnqueueAction($"카드({handOnAbility.ownerCardId}) !! 핸드온 어빌리티 액션 해제!!", () =>
                    {
                        AbilityLogic.Instance.HandOnAbilityCut(handOnAbility);
                    });
                }
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
                if (actorSlot.Card == null)
                {
                    // Debug.LogWarning($"{actorSlot.Slot}번 슬롯 장착된 카드가 없어 액션 패스 - BattleLogic.BoardCardAction");
                    return;
                }

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
                    int aiDataId = card.GetCardAiDataId();

                    if (aiDataId == 0)
                    {
                        Debug.LogWarning($"{actorSlot.Slot}번 슬롯 카드({card.id}). AiGroupData({card.aiGroupId})에 해당하는 <color=red>액션 데이터 없음</color> - BattleLogic.BoardCardAction");
                        return;
                    }

                    // ai 실행
                    AiData aiData = AiData.GetAiData(aiDataId);

                    // AiData 에 설정된 타겟 얻기
                    List<BoardSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, actorSlot);

                    if (aiTargetSlots.Count == 0)
                    {
                        Debug.LogWarning($"{Utils.BoardSlotLog(actorSlot)} 설정 타겟({aiData.target}) 없음 - BattleLogic.BoardCardAction");
                        return;
                    }

                    // Debug.Log($"{Utils.BoardSlotLog(actorSlot)} AiData 에 설정된 어빌리티({string.Join(", ", aiData.ability_Ids)}) 타겟({aiData.target}) Slots: <color=yellow>{string.Join(", ", aiTargetSlots.Select(slot => slot.Slot))}</color>번에 적용 - BattleLogic.BoardCardAction");

                    // 어빌리티 적용
                    AbilityLogic.Instance.SetBoardSlotAbility(AbilityWhereFrom.TurnEndBoardSlot, aiData, actorSlot, aiTargetSlots);
                });
            }
        }

        void BuildingCardAction()
        {
            // Debug.Log($"BuildingCardAction: {Board.Instance.GetBuildingSlots().Count}");
            foreach (BoardSlot buildingSlot in Board.Instance.GetBuildingSlots())
            {
                if (buildingSlot.Card == null)
                {
                    Debug.LogWarning($"건물 {buildingSlot.Slot}번 슬롯 장착된 카드가 없어 패스 - BattleLogic.BuildingCardAction");
                    return;
                }

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
                    int aiDataId = card.GetCardAiDataId();

                    if (aiDataId == 0)
                    {
                        Debug.LogWarning($"건물 {buildingSlot.Slot}번 슬롯 카드({card.id}). AiGroupData({card.aiGroupId})에 해당하는 <color=red>액션 데이터 없음<color> - BattleLogic.BuildingCardAction");
                        return;
                    }

                    // ai 실행
                    AiData aiData = AiData.GetAiData(aiDataId);

                    // AiData 에 설정된 타겟 얻기
                    List<BoardSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, buildingSlot);

                    if (aiTargetSlots.Count == 0)
                    {
                        Debug.LogWarning($"{Utils.BoardSlotLog(buildingSlot)} 설정 타겟({aiData.target}) 없음 - BattleLogic.BuildingCardAction");
                        return;
                    }

                    // Debug.Log($"{Utils.BoardSlotLog(buildingSlot)} AiData 에 설정된 어빌리티({string.Join(", ", aiData.ability_Ids)}) 타겟({aiData.target}) Slots: <color=yellow>{string.Join(", ", aiTargetSlots.Select(slot => slot.Slot))}</color>번에 적용 - BattleLogic.BuildingCardAction");

                    // 어빌리티 적용
                    AbilityLogic.Instance.SetBoardSlotAbility(AbilityWhereFrom.TurnEndBuilding, aiData, buildingSlot, aiTargetSlots);
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

        /// <summary>
        /// 핸드 카드 사용 가능 확인
        /// </summary>
        public bool CanHandCardUse(string cardUid)
        {
            Card card = master.GetHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"CanUseCard: card is null({card.id})");
                return false;
            }

            // 필요 마나 확인
            if (card.costMana != 0 && master.Mana < card.costMana)
            {
                ToastNotification.Show($"mana is not enough({master.Mana})");
                Debug.LogError($"CanUseCard: mana is not enough({card.id}, {master.Mana} < {card.costMana})");
                return false;
            }

            // 필요 골드 확인
            if (card.costGold != 0 && master.Gold < card.costGold)
            {
                ToastNotification.Show($"gold is not enough({master.Gold})");
                Debug.LogError($"CanUseCard: gold is not enough({card.id}, {master.Gold} < {card.costGold})");
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

            // Master handCards => boardCreatureCards or boardBuildingCards 로 이동
            master.HandCardToBoard(cardUid);

            // 마스터 mana 감소
            if (card.costMana > 0)
                master.DecreaseMana(card.costMana);

            // 마스터 gold 감소
            if (card.costGold > 0)
                master.AddGold(-card.costGold);

            Board.Instance.SetMasterMana(master.Mana);
            Board.Instance.SetGold(master.Gold);

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