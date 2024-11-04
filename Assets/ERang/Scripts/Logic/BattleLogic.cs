using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using UnityEngine.Events;
using TMPro;
using Newtonsoft.Json;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }

        public int turnCount = 1;
        public float turnStartActionDelay = .5f;
        public float boardCardActionDelay = .5f;
        public float abilityReleaseDelay = .5f;
        public TextMeshProUGUI floorText;
        public TextMeshProUGUI resultText;

        public Master Master => master;

        public int masterId;
        public int floor;
        public int levelId;

        private Master master;
        private DeckSystem deckSystem;
        private bool isTruenEndProcessing = false;
        private MapLocation selectLocation;

        // for test
        private Queue<NamedAction> actionQueue = new Queue<NamedAction>();
        private List<BoardSlot> flashingSlots = new List<BoardSlot>();

        void Awake()
        {
            Instance = this;

            masterId = PlayerPrefsUtility.GetInt("MasterId", 1001);
            floor = PlayerPrefsUtility.GetInt("Floor", 1);
            levelId = PlayerPrefsUtility.GetInt("LevelId", 100100101);

            string selectLocationJson = PlayerPrefsUtility.GetString("SelectLocation", null);

            if (selectLocationJson != null)
                selectLocation = JsonConvert.DeserializeObject<MapLocation>(selectLocationJson);

            // 시스템 생성
            deckSystem = GetComponent<DeckSystem>();

            // 마스터
            MasterData masterData = MasterData.GetMasterData(masterId);

            if (masterData == null)
            {
                Debug.LogError($"마스터({masterId}) MasterData {Utils.RedText("테이블 데이터 없음")}");
                return;
            }

            master = new Master(masterData);
        }

        void Start()
        {
            deckSystem.CreateMasterCards(master.StartCardIds);

            // 마스터 생성
            BoardSystem.Instance.CreateBoardSlots(master);

            // 몬스터 카드 생성
            LevelData levelData = LevelGroupData.GetLevelData(levelId);

            if (levelData == null)
            {
                Debug.LogError($"레벨({levelId}) LevelGroupData {Utils.RedText("테이블 데이터 없음")}");
                return;
            }

            Debug.Log($"----------------- BATTLE START {floor} 층 ({levelId}) -----------------");
            BoardSystem.Instance.CreateMonsterBoardSlots(levelData.cardIds);

            floorText.text = $"{floor} 층\n({levelId}) \n{selectLocation?.eventType ?? EventType.None}";

            StartCoroutine(TurnStart());
        }

        void Update()
        {
            ActionQueueProcess();
        }

        public IEnumerator TurnStart()
        {
            yield return new WaitForSeconds(.3f);

            // ToastNotification.Show($"!! TURN START !! ({turnCount})");
            Debug.Log($"----------------- {turnCount} TURN START -----------------");

            // 턴 카운트 설정
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 마나 충전
            // EnqueueAction("마나 충전", () => { BoardSystem.Instance.ChargeMana(master); });
            BoardSystem.Instance.ChargeMana(master);

            // 핸드 카드 만들기
            yield return StartCoroutine(deckSystem.MakeHandCards());

            // 핸드 카드 HandOn 어빌리티 액션
            // HandOnCardAbilityAction(deckSystem.HandCards);

            // 지속 시간 종료 어빌리티 해제
            yield return StartCoroutine(BoardSlotCardAbilityRelease());

            // 턴 시작시 실행되는 카드의 Reaction 을 확인
            StartCoroutine(TurnStartAction());
        }

        public void TurnEnd()
        {
            if (isTruenEndProcessing)
            {
                Debug.LogWarning("이미 턴 종료 처리 중");
                return;
            }

            isTruenEndProcessing = true;

            Debug.Log($"----------------- {turnCount} TURN END -----------------");

            StartCoroutine(TrunEndProcess());
        }

        private IEnumerator TrunEndProcess()
        {
            // 보드 - 턴 카운트
            turnCount += 1;
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 핸드덱에 카드 제거
            deckSystem.RemoveTurnEndHandCard();

            // 마스터 마나 리셋
            // EnqueueAction("마나 리셋", () => { BoardSystem.Instance.ResetMana(master); });
            BoardSystem.Instance.ResetMana(master);

            // 핸드 온 카드 어빌리티 해제
            yield return StartCoroutine(HandOnCardAbilityRelease());

            // 카드 액션
            yield return StartCoroutine(TurnEndCardAction());

            // 턴 다시 시작
            StartCoroutine(TurnStart());

            isTruenEndProcessing = false;
        }

        IEnumerator BattleEnd(int monsterCount, BoardSlot removeCardBoardSlot)
        {
            // 배틀 종료 확인. 마스터 카드 제거. 몬스터 카드 + 적 마스터 카드 제거
            bool isWin = monsterCount == 0;
            resultText.text = isWin ? "YOU WIN" : "YOU LOSE";

            // 이기면 층 증가
            int nextFloor = isWin ? floor + 1 : 0;
            PlayerPrefsUtility.SetInt("Floor", nextFloor);

            int locationId = PlayerPrefsUtility.GetInt("LastLocationId", 0);

            Debug.Log($"배틀 종료 {isWin}, loastLocationId: {locationId}, nextFloor: {nextFloor}");

            // 마지막에 선택한 층 인덱스 저장 (지면 초기화)
            PlayerPrefsUtility.SetInt("LastLocationId", isWin ? locationId : 0);

            if (!isWin)
            {
                PlayerPrefsUtility.SetInt("MasterId", 0);
                PlayerPrefsUtility.SetInt("AreaId", 0);
                PlayerPrefsUtility.SetInt("LevelId", 0);
            }

            yield return new WaitForSeconds(2f);

            GameObject nextSceneObject = GameObject.Find("Scene Manager");

            if (nextSceneObject.TryGetComponent<NextScene>(out NextScene nextScene))
            {
                nextScene.Play(isWin ? "Map" : "Lobby");
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
                    // Debug.LogWarning($"{boardSlot.Slot}번 슬롯 장착된 카드가 없어 액션 패스");
                    continue;
                }

                // AiGroupData 액션 AiDtaId 얻기
                int aiDataId = AiLogic.Instance.GetTurnStartActionAiDataId(boardSlot, opponentSlots);

                if (aiDataId == 0)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} AiGroupData({card.AiGroupId})에 대한 이번 턴({turnCount}) 시작 리액션 안함");
                    continue;
                }

                AiAbilityProcess(aiDataId, boardSlot, AbilityWhereFrom.TurnStartAction);

                yield return new WaitForSeconds(turnStartActionDelay);
            }
        }

        IEnumerator TurnEndCardAction()
        {
            List<BoardSlot> creatureSlots = BoardSystem.Instance.GetCreatureBoardSlots();
            yield return StartCoroutine(BoardCardAction(creatureSlots));

            List<BoardSlot> monsterSlots = BoardSystem.Instance.GetMonsterBoardSlots();
            yield return StartCoroutine(BoardCardAction(monsterSlots));

            List<BoardSlot> buildingSlots = BoardSystem.Instance.GetBuildingBoardSlots();
            yield return StartCoroutine(BoardCardAction(buildingSlots));
        }

        /// <summary>
        /// 턴 종료 보드 슬롯 카드 액션
        /// </summary>
        IEnumerator BoardCardAction(List<BoardSlot> actorSlots)
        {
            foreach (BoardSlot boardSlot in actorSlots)
            {
                BaseCard card = boardSlot.BaseCard;

                if (card == null)
                {
                    // Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없어 카드 액션 패스");
                    continue;
                }

                if (card.AiGroupId == 0)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} AiGroupId 가 {Utils.RedText(card.AiGroupId)}이라서 카드 액션 패스");
                    continue;
                }

                // 카드의 행동 aiData 설정
                int aiDataId = AiLogic.Instance.GetCardAiDataId(card);

                if (aiDataId == 0)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} AiGroupData({card.AiGroupId})에 해당하는 aiDataId 얻기 실패");
                    continue;
                }

                AiAbilityProcess(aiDataId, boardSlot, AbilityWhereFrom.TurnEndBoardSlot);

                yield return new WaitForSeconds(boardCardActionDelay);
            }
        }

        /// <summary>
        /// 핸드에 있을때 효과가 발동되는 카드 액션
        /// </summary>
        void HandOnCardAbilityAction(List<BaseCard> handCards)
        {
            // 핸드 온 카드 액션의 주체는 마스터 슬롯
            BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);
            List<(BaseCard card, AiData aiData, List<AbilityData> abilityDatas)> handOnCards = AiLogic.Instance.GetHandOnCards(handCards);

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
        /// 핸드 카드 사용
        /// </summary>
        public void HandCardUse(string cardUid, BoardSlot targetSlot)
        {
            BaseCard card = deckSystem.FindHandCard(cardUid);

            // 타겟 설정 카드인가 확인
            var (isSelectAttackType, aiData) = AiLogic.Instance.GetAiAttackInfo(card);

            Debug.Log($"핸드 카드({card.Id}) 사용. isSelectAttackType: {isSelectAttackType}, aiDataId: {aiData?.ai_Id ?? 0}, targetSlot: {targetSlot?.Slot ?? -1}");

            if (isSelectAttackType)
            {
                if (targetSlot == null)
                {
                    Debug.LogWarning($"핸드 카드({card.Id}) 타겟 슬롯이 없어 카드 사용 불가");
                    return;
                }

                List<BoardSlot> selectTypeTargetSlots = TargetLogic.Instance.GetSelectAttackTypeTargetSlot(aiData.attackType);

                // 타겟 슬롯에 대상 슬롯에 포함되어 있으면 카드 사용
                if (selectTypeTargetSlots.Contains(targetSlot))
                {
                    Debug.Log($"타겟 슬롯 {targetSlot.Slot} 에 핸드 카드({card.Id}) 사용");
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
                    Debug.LogWarning($"핸드 카드({card.Id}) 타겟 아님");
                    return;
                }
            }

            Debug.Log($"핸드 카드({card.Id}) 사용");

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
            List<Ability> abilities = AbilityLogic.Instance.GetHandOnAbilities();

            if (abilities.Count == 0)
                yield break;

            Debug.Log($"핸드 온 어빌리티 해제 시작. 어빌리티 개수: {abilities.Count}");

            for (int i = 0; i < abilities.Count; i++)
            {
                Ability ability = abilities[i];

                CardType cardType = BoardSystem.Instance.GetBoardSlot(ability.targetBoardSlot)?.CardType ?? CardType.None;

                if (ability.whereFrom != AbilityWhereFrom.TurnStarHandOn)
                {
                    Debug.Log($"{Utils.AbilityLog(ability)}는 핸드 온 어빌리티가 아니라서 패스. {ability.whereFrom} 어빌리티");
                    continue;
                }

                // Debug.Log($"{Utils.BoardSlotLog(ability.targetBoardSlot, cardType, ability.targetCardId)} {Utils.AbilityLog(ability)} Duration: {ability.duration} 으로 해제");

                BoardSlot selfBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.selfBoardSlot);
                BoardSlot targetBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.targetBoardSlot);

                StartCoroutine(AbilityLogic.Instance.AbilityRelease(ability, selfBoardSlot, targetBoardSlot));

                yield return new WaitForSeconds(abilityReleaseDelay);
            }

            Debug.Log($"핸드 온 어빌리티 해제 완료");
        }

        /// <summary>
        /// 보드 카드 지속시간(duration) 이 0되는 카드 어빌리티 해제
        /// </summary>
        IEnumerator BoardSlotCardAbilityRelease()
        {
            List<Ability> abilities = AbilityLogic.Instance.GetBoardSlotCardAbilities();

            if (abilities.Count == 0)
                yield break;

            Debug.Log($"보드 슬롯 어빌리티 해제 시작");

            for (int i = 0; i < abilities.Count; ++i)
            {
                Ability ability = abilities[i];

                ability.duration = ability.duration - 1;

                CardType cardType = BoardSystem.Instance.GetBoardSlot(ability.targetBoardSlot)?.CardType ?? CardType.None;

                if (ability.whereFrom == AbilityWhereFrom.TurnStarHandOn)
                {
                    Debug.Log($"{Utils.AbilityLog(ability)}는 보드 슬롯 카드 어빌리티가 아니라서 패스. {ability.whereFrom} 어빌리티");
                    continue;
                }

                if (ability.duration > 0)
                {
                    Debug.Log($"{Utils.BoardSlotLog(ability.targetBoardSlot, cardType, ability.targetCardId)} {Utils.AbilityLog(ability)}는 Duration: {ability.duration} 남아서 패스");
                    continue;
                }

                // Debug.Log($"{Utils.BoardSlotLog(ability.targetBoardSlot, cardType, ability.targetCardId)} {Utils.AbilityLog(ability)} Duration: {ability.duration} 으로 해제");

                BoardSlot selfBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.selfBoardSlot);
                BoardSlot targetBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.targetBoardSlot);

                StartCoroutine(AbilityLogic.Instance.AbilityRelease(ability, selfBoardSlot, targetBoardSlot));

                yield return new WaitForSeconds(abilityReleaseDelay);
            }

            Debug.Log($"보드 슬롯 어빌리티 해제 완료");
        }

        /// <summary>
        /// 핸드 카드 사용 가능 확인
        /// </summary>
        public bool CanHandCardUse(string cardUid)
        {
            BaseCard card = deckSystem.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 카드({card.Id}) 없음");
                return false;
            }

            if (card.inUse == false)
            {
                ToastNotification.Show($"card({card.Id}) is not in use");
                Debug.LogWarning($"사용할 수 없는 카드({card.Id}) InUse: false 설정");
                return false;
            }

            // 필요 마나 확인
            if (card is MagicCard magicCard && master.Mana < magicCard.Mana)
            {
                ToastNotification.Show($"mana({master.Mana}) is not enough");
                Debug.LogWarning($"핸드 카드({magicCard.Id}) 마나 부족으로 사용 불가능({master.Mana} < {magicCard.Mana})");
                return false;
            }

            // 필요 골드 확인
            if (card is BuildingCard buildingCard && master.Gold < buildingCard.Gold)
            {
                ToastNotification.Show($"gold({master.Gold}) is not enough");
                Debug.LogWarning($"핸드 카드({buildingCard.Id}) 골드 부족으로 사용 불가능({master.Gold} < {buildingCard.Gold})");
                return false;
            }

            Debug.Log($"핸드 카드({card.Id}) 사용 가능");

            return true;
        }

        /// <summary>
        /// 보드 슬롯에 카드 장착
        /// </summary>
        /// <param name="boardSlot"></param>
        /// <param name="cardUid"></param>
        public void BoardSlotEquipCard(BoardSlot boardSlot, string cardUid)
        {
            BaseCard card = deckSystem.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 {cardUid} 카드 없음");
                return;
            }

            // BoardSlot 에 카드 장착
            boardSlot.EquipCard(card);

            // Master handCards => boardCreatureCards or boardBuildingCards 로 이동
            deckSystem.HandCardToBoard(cardUid);

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, card);

            Debug.Log($"보드 슬롯 {boardSlot.Slot} 에 카드({card.Id}) 장착");
        }

        /// <summary>
        /// 보드 슬롯에 장착된 카드 제거
        /// </summary>
        /// <param name="boardSlot"></param>
        public IEnumerator RemoveBoardCard(BoardSlot boardSlot)
        {
            if (boardSlot == null)
            {
                Debug.LogError($"{boardSlot.Slot}번 보드 슬롯 없음");
                yield break;
            }

            Debug.Log($"{Utils.BoardSlotLog(boardSlot)} <color=#f52d2d>카드 제거</color>");

            // 카드 어빌리티 제거
            AbilityLogic.Instance.RemoveAbility(boardSlot.Card.Uid);

            // 마스터에서 카드 제거
            if (boardSlot.Card.Type != CardType.Monster)
                deckSystem.RemoveBoardCard(boardSlot.Card.Uid);

            // 보드에서 카드 제거 - 제일 마지막에 실행
            BoardSystem.Instance.RemoveCard(boardSlot.Card.Uid);

            int monsterCount = BoardSystem.Instance.GetMonsterBoardSlots().Count(slot => slot.Card != null);

            // 배틀 종료
            if (monsterCount == 0 || boardSlot.Slot == 0)
                yield return StartCoroutine(BattleEnd(monsterCount, boardSlot));
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
        /// 테스트
        /// </summary>
        public void Test()
        {
            // BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(6);
            // List<BoardSlot> targetSlots = BoardSystem.Instance.GetBoardSlots(new List<int> { 3, 2 });

            // AiData aiData = AiData.GetAiData(1001);
            // AbilityData abilityData = AbilityData.GetAbilityData(70001); // 기본 근거리 공격

            // StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots));

            BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);

            List<BoardSlot> firstSlots = TargetLogic.Instance.TargetFirstEnemy(selfSlot);
            Debug.Log($"TargetFirstEnemy - first Slot: {firstSlots[0].Slot}, index: {firstSlots[0].Index}");

            List<BoardSlot> secondsSlots = TargetLogic.Instance.TargetSecondEnemy(selfSlot);
            Debug.Log($"TargetSecondEnemy - first Slot: {secondsSlots[0].Slot}, index: {secondsSlots[0].Index}");
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
                // AbilityLogic.Instance.AddAbility(aiData, abilityData, boardSlot, targetSlots, whereFrom);

                // 어빌리티 적용
                StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, boardSlot, targetSlots, whereFrom));
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