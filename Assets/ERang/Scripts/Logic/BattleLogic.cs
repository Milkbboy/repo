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

        public int turnCount = 1;
        public float turnStartActionDelay = .5f;
        public float boardCardActionDelay = .5f;
        public float abilityReleaseDelay = .5f;

        public Master Master
        { get { return master; } }
        public Enemy Enemy { get { return enemy; } }

        public int masterId = 1001;

        private Master master;
        private Enemy enemy;

        private DeckSystem deckSystem;

        // for test
        private Queue<NamedAction> actionQueue = new Queue<NamedAction>();
        private List<BoardSlot> flashingSlots = new List<BoardSlot>();

        void Awake()
        {
            Instance = this;

            // 시스템 생성
            deckSystem = GetComponent<DeckSystem>();

            // 마스터, 적 객체 생성
            MasterData masterData = MasterData.GetMasterData(masterId);

            if (masterData == null)
                throw new System.Exception($"MasterData 테이블에 {masterId} 마스터 데이터 없음");

            master = new Master(masterData);
            enemy = new Enemy(1002);
        }

        void Start()
        {
            deckSystem.CreateMasterCards(master.StartCardIds);

            BoardSystem.Instance.CreateBoardSlots(master, enemy);
            BoardSystem.Instance.CreateMonsterBoardSlots(enemy.monsterCards);

            StartCoroutine(TurnStart());
        }

        void Update()
        {
            ActionQueueProcess();
        }

        public IEnumerator TurnStart()
        {
            yield return new WaitForSeconds(.3f);

            ToastNotification.Show($"!! TURN START !! ({turnCount})");

            // 턴 카운트 설정
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 마나 충전
            // EnqueueAction("마나 충전", () => { BoardSystem.Instance.ChargeMana(master); });
            BoardSystem.Instance.ChargeMana(master);

            // 핸드 카드 만들기
            yield return StartCoroutine(deckSystem.MakeHandCards());

            // 핸드 카드 HandOn 어빌리티 액션
            HandOnCardAbilityAction(deckSystem.HandCards);

            // 지속 시간 종료 어빌리티 해제
            yield return StartCoroutine(BoardSlotCardAbilityRelease());

            // 턴 시작시 실행되는 카드의 Reaction 을 확인
            StartCoroutine(TurnStartAction());
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
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 핸드덱에 카드 제거
            deckSystem.RemoveTurnEndHandCard();

            // 마스터 마나 리셋
            // EnqueueAction("마나 리셋", () => { BoardSystem.Instance.ResetMana(master); });
            BoardSystem.Instance.ResetMana(master);

            // 핸드 온 카드 어빌리티 해제
            StartCoroutine(HandOnCardAbilityRelease());

            // 카드 액션
            StartCoroutine(CardAction());

            // 턴 다시 시작
            StartCoroutine(TurnStart());
        }

        IEnumerator CardAction()
        {
            List<BoardSlot> creatureSlots = BoardSystem.Instance.GetCreatureBoardSlots();
            yield return StartCoroutine(BoardCardAction(creatureSlots));

            List<BoardSlot> monsterSlots = BoardSystem.Instance.GetMonsterBoardSlots();
            yield return StartCoroutine(BoardCardAction(monsterSlots));

            List<BoardSlot> buildingSlots = BoardSystem.Instance.GetBuildingBoardSlots();
            yield return StartCoroutine(BoardCardAction(buildingSlots));
        }

        /// <summary>
        /// 핸드에 있을때 효과가 발동되는 카드 액션
        /// </summary>
        void HandOnCardAbilityAction(List<Card> handCards)
        {
            // 핸드 온 카드 액션의 주체는 마스터 슬롯
            BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);
            List<(Card card, AiData aiData, List<AbilityData> abilityDatas)> handOnCards = AiLogic.Instance.GetHandOnCards(handCards);

            foreach (var (card, aiData, abilityDatas) in handOnCards)
            {
                List<BoardSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot);

                foreach (AbilityData abilityData in abilityDatas)
                {
                    // 어빌리티 효과 제거를 위한 저장
                    AbilityLogic.Instance.AddHandOnAbility(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.TurnStarHandOn);

                    // 어빌리티 적용
                    StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots));
                }
            }
        }

        /// <summary>
        /// 턴 시작 액션
        /// </summary>
        IEnumerator TurnStartAction()
        {
            List<BoardSlot> reactionSlots = BoardSystem.Instance.GetMonsterBoardSlots();
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetCreatureBoardSlots();

            foreach (BoardSlot boardSlot in reactionSlots)
            {
                Card card = boardSlot.Card;

                if (card == null)
                {
                    Debug.LogWarning($"{boardSlot.Slot}번 슬롯 장착된 카드가 없어 액션 패스 - BattleLogic.TurnStartAction");
                    continue;
                }

                // AiGroupData 액션 AiDtaId 얻기
                int aiDataId = AiLogic.Instance.GetTurnStartActionAiDataId(boardSlot, opponentSlots);

                if (aiDataId == 0)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} AiGroupData({card.AiGroupId})에서 턴 시작 액션 aiDataId 얻기 실패 - BattleLogic.TurnStartAction");
                    continue;
                }

                AiAbilityProcess(aiDataId, boardSlot, AbilityWhereFrom.TurnStartAction);

                yield return new WaitForSeconds(turnStartActionDelay);
            }
        }

        /// <summary>
        /// 턴 종료 보드 슬롯 카드 액션
        /// </summary>
        IEnumerator BoardCardAction(List<BoardSlot> actorSlots)
        {
            foreach (BoardSlot boardSlot in actorSlots)
            {
                Card card = boardSlot.Card;

                if (card == null)
                {
                    Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없어 카드 액션 패스 - BattleLogic.BoardCardAction");
                    continue;
                }

                // 카드의 행동 aiData 설정
                int aiDataId = AiLogic.Instance.GetCardAiDataId(card);

                if (aiDataId == 0)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} AiGroupData({card.AiGroupId})에 해당하는 aiDataId 얻기 실패 - BattleLogic.BoardCardAction");
                    continue;
                }

                AiAbilityProcess(aiDataId, boardSlot, AbilityWhereFrom.TurnEndBoardSlot);

                yield return new WaitForSeconds(boardCardActionDelay);
            }
        }

        /// <summary>
        /// 핸드 카드 사용
        /// </summary>
        public void HandCardUse(string cardUid, BoardSlot targetSlot)
        {
            Card card = deckSystem.FindHandCard(cardUid);

            // 타겟 설정 카드인가 확인
            var (isSelectAttackType, aiData) = AiLogic.Instance.GetAiAttackInfo(card);

            Debug.Log($"핸드 카드({card.Id}) 사용. isSelectAttackType: {isSelectAttackType}, aiDataId: {aiData?.ai_Id ?? 0}, targetSlot: {targetSlot?.Slot ?? -1} - BattleLogic.HandCardUse");

            if (isSelectAttackType)
            {
                if (targetSlot == null)
                {
                    Debug.LogWarning($"핸드 카드({card.Id}) 타겟 슬롯이 없어 카드 사용 불가 - BattleLogic.HandCardUse");
                    return;
                }

                List<BoardSlot> selectTypeTargetSlots = TargetLogic.Instance.GetSelectAttackTypeTargetSlot(aiData.attackType);

                // 타겟 슬롯에 대상 슬롯에 포함되어 있으면 카드 사용
                if (selectTypeTargetSlots.Contains(targetSlot))
                {
                    Debug.Log($"타겟 슬롯 {targetSlot.Slot} 에 핸드 카드({card.Id}) 사용 - BattleLogic.HandCardUse");
                    BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);

                    List<BoardSlot> targetSlots = new() { targetSlot };
                    List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                    foreach (AbilityData abilityData in abilityDatas)
                    {
                        // 핸드 카드로 공격하는 경우 마스터 공격력 설정. 핸드 카드 동작은 마스터 카드로 하는데....이건 아니다.
                        selfSlot.SetCardAtk(aiData.value);

                        // 어빌리티 효과 제거를 위한 저장
                        AbilityLogic.Instance.AddAbility(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.HandUse);

                        // 어빌리티 적용
                        StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots));
                    }
                }
                else
                {
                    Debug.LogWarning($"핸드 카드({card.Id}) 타겟 아님 - BattleLogic.HandCardUse");
                    return;
                }
            }

            Debug.Log($"핸드 카드({card.Id}) 사용 - BattleLogic.HandCardUse");

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, card);

            // 마스터 핸드 카드 제거
            deckSystem.RemoveUsedHandCard(cardUid);
        }

        /// <summary>
        /// 핸드 온 카드 액션 해제
        /// </summary>
        IEnumerator HandOnCardAbilityRelease()
        {
            List<Ability> abilities = AbilityLogic.Instance.GetAbilities();

            for (int i = 0; i < abilities.Count; i++)
            {
                Ability ability = abilities[i];

                if (ability.whereFrom != AbilityWhereFrom.TurnStarHandOn)
                    continue;

                BoardSlot selfBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.selfBoardSlot);
                BoardSlot targetBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.targetBoardSlot);

                AbilityLogic.Instance.AbilityRelease(ability, selfBoardSlot, targetBoardSlot);

                yield return new WaitForSeconds(abilityReleaseDelay);
            }
        }

        /// <summary>
        /// 보드 카드 지속시간(duration) 이 0되는 카드 어빌리티 해제
        /// </summary>
        IEnumerator BoardSlotCardAbilityRelease()
        {
            List<Ability> abilities = AbilityLogic.Instance.GetAbilities();

            for (int i = 0; i < abilities.Count; ++i)
            {
                Ability ability = abilities[i];

                ability.duration = ability.duration - 1;

                if (ability.whereFrom == AbilityWhereFrom.TurnStarHandOn || ability.duration > 0)
                    continue;

                BoardSlot selfBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.selfBoardSlot);
                BoardSlot targetBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.targetBoardSlot);

                AbilityLogic.Instance.AbilityRelease(ability, selfBoardSlot, targetBoardSlot);

                yield return new WaitForSeconds(abilityReleaseDelay);
            }
        }

        /// <summary>
        /// 핸드 카드 사용 가능 확인
        /// </summary>
        public bool CanHandCardUse(string cardUid)
        {
            Card card = deckSystem.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 카드({card.Id}) 없음 - BattleLogic.CanHandCardUse");
                return false;
            }

            if (card.inUse == false)
            {
                ToastNotification.Show($"card({card.Id}) is not in use");
                Debug.LogWarning($"사용할 수 없는 카드({card.Id}) InUse: false 설정 - BattleLogic.CanHandCardUse");
                return false;
            }

            // 필요 마나 확인
            if (card.costMana != 0 && master.Mana < card.costMana)
            {
                ToastNotification.Show($"mana({master.Mana}) is not enough");
                Debug.LogWarning($"핸드 카드({card.Id}) 마나 부족으로 사용 불가능({master.Mana} < {card.costMana}) - BattleLogic.CanHandCardUse");
                return false;
            }

            // 필요 골드 확인
            if (card.costGold != 0 && master.Gold < card.costGold)
            {
                ToastNotification.Show($"gold({master.Gold}) is not enough");
                Debug.LogWarning($"핸드 카드({card.Id}) 골드 부족으로 사용 불가능({master.Gold} < {card.costGold}) - BattleLogic.CanHandCardUse");
                return false;
            }

            Debug.Log($"핸드 카드({card.Id}) 사용 가능 - BattleLogic.CanHandCardUse");

            return true;
        }

        /// <summary>
        /// 보드 슬롯에 카드 장착
        /// </summary>
        /// <param name="boardSlot"></param>
        /// <param name="cardUid"></param>
        public void BoardSlotEquipCard(BoardSlot boardSlot, string cardUid)
        {
            Card card = deckSystem.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {cardUid} 카드 없음 - BattleLogic.BoardSlotEquipCard");
                return;
            }

            // BoardSlot 에 카드 장착
            boardSlot.EquipCard(card);

            // Master handCards => boardCreatureCards or boardBuildingCards 로 이동
            deckSystem.HandCardToBoard(cardUid);

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, card);

            Debug.Log($"보드 슬롯 {boardSlot.Slot} 에 카드({card.Id}) 장착 - BattleLogic.BoardSlotEquipCard");
        }

        public void RemoveBoardCard(BoardSlot boardSlot)
        {
            if (boardSlot == null)
            {
                Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없음 - BattleLogic.RemoveBoardCard");
                return;
            }

            Debug.Log($"{boardSlot.Slot}번 슬롯 카드 제거 - BattleLogic.RemoveBoardCard");

            // 카드 어빌리티 제거
            AbilityLogic.Instance.RemoveAbility(boardSlot.Card.Uid);

            // 마스터에서 카드 제거
            if (boardSlot.Card.Type != CardType.Monster)
                deckSystem.RemoveBoardCard(boardSlot.Card.Uid);

            // 보드에서 카드 제거 - 제일 마지막에 실행
            BoardSystem.Instance.RemoveCard(boardSlot.Card.Uid);

            // 배틀 종료 확인. 마스터 카드 제거. 몬스터 카드 + 적 마스터 카드 제거
            int monsterCount = BoardSystem.Instance.GetMonsterBoardSlots().Count(slot => slot.Card != null);

            if (monsterCount == 0 || boardSlot.Slot == 0)
            {
                Debug.Log($"배틀 종료. monsterCount: {monsterCount}, Slot: {boardSlot.Slot} - BattleLogic.RemoveBoardCard");
                GameObject nextScene = GameObject.Find("Scene Manager");
                nextScene.GetComponent<NextScene>().Play();
            }
        }

        /// <summary>
        /// 소멸 카드 확인
        /// </summary>
        public void ExtinctionCards()
        {
            string extinctionCardIds = string.Join(", ", deckSystem.ExtinctionCards.Select(card => card.Id));
            ToastNotification.Show($"Extinction Card Ids: {extinctionCardIds}");
        }

        /// <summary>
        /// 테스트용 근거리
        /// </summary>
        public void TestMelee()
        {
            BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(6);
            List<BoardSlot> targetSlots = BoardSystem.Instance.GetBoardSlots(new List<int> { 3, 2 });

            AiData aiData = AiData.GetAiData(1001);
            AbilityData abilityData = AbilityData.GetAbilityData(70001); // 기본 근거리 공격

            StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots));
        }

        /// <summary>
        /// 테스트용 원거리
        /// </summary>
        public void TestRanged()
        {
            BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(7);
            List<BoardSlot> targetSlots = BoardSystem.Instance.GetBoardSlots(new List<int> { 3, 2, 1 });

            AiData aiData = AiData.GetAiData(1004);
            AbilityData abilityData = AbilityData.GetAbilityData(70004); // 기본 원거리 공격

            StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots));
        }

        /// <summary>
        /// AI 어빌리티 실행
        /// </summary>
        /// <param name="aiDataId"></param>
        /// <param name="boardSlot"></param>
        /// <param name="whereFrom"></param>
        private void AiAbilityProcess(int aiDataId, BoardSlot boardSlot, AbilityWhereFrom whereFrom)
        {
            AiData aiData = AiData.GetAiData(aiDataId);

            if (aiData == null)
            {
                Debug.LogError($"{Utils.BoardSlotLog(boardSlot)} AiData({aiDataId}) <color=red>테이블 데이터 없음</color> ");
                return;
            }

            // AiData 에 설정된 타겟 얻기
            List<BoardSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, boardSlot);

            if (targetSlots.Count == 0)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} 설정 타겟({aiData.target}) 없음 ");
                return;
            }

            List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

            foreach (AbilityData abilityData in abilityDatas)
            {
                // 어빌리티 효과 제거를 위한 저장
                AbilityLogic.Instance.AddAbility(aiData, abilityData, boardSlot, targetSlots, whereFrom);

                // 어빌리티 적용
                StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, boardSlot, targetSlots));
            }
        }

        /// <summary>
        /// 테스트 카드 깜빡임
        /// </summary>
        /// <param name="boardSlot"></param>
        private void FlashingCard(BoardSlot boardSlot)
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
            Debug.Log($"<color=#257dca>EnqueueAction</color>: {name}");
            actionQueue.Enqueue(new NamedAction(name, action));
        }

        private void ActionQueueProcess()
        {
            if (Input.GetKeyDown(KeyCode.Space) == false)
                return;

            Debug.Log($"액션 큐 개수: {actionQueue.Count}");

            if (flashingSlots.Count > 0)
            {
                foreach (BoardSlot slot in flashingSlots)
                    slot.StopFlashing();

                flashingSlots.Clear();
            }

            if (actionQueue.Count > 0)
            {
                var namedAction = actionQueue.Dequeue();
                Debug.Log($"<color=#dd3333>Execution action</color>: {namedAction.Name}");
                namedAction.Action?.Invoke();
            }
        }
    }
}