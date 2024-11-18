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
        public SatietyUI satietyUI;

        private Master master;
        private DeckSystem deckSystem;
        private bool isTruenEndProcessing = false;
        private MapLocation selectLocation;
        private bool keepSatiety;

        // for test
        private Queue<NamedAction> actionQueue = new Queue<NamedAction>();
        private List<BSlot> flashingSlots = new List<BSlot>();

        void Awake()
        {
            Instance = this;

            masterId = PlayerPrefsUtility.GetInt("MasterId", 1001);
            floor = PlayerPrefsUtility.GetInt("Floor", 1);
            levelId = PlayerPrefsUtility.GetInt("LevelId", 100100101);
            keepSatiety = PlayerPrefsUtility.GetValue<bool>("KeepSatiety", false);

            Debug.Log($"포만감 저장 여부: {keepSatiety}");

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
            floorText.text = $"{floor} 층\n({levelId}) \n{selectLocation?.eventType ?? EventType.None}";

            BoardSystem.Instance.CreateBoardSlots(master.CreatureSlotCount);

            LevelData levelData = LevelGroupData.GetLevelData(levelId);

            if (levelData == null)
            {
                Debug.LogError($"레벨({levelId}) LevelGroupData {Utils.RedText("테이블 데이터 없음")}");
                return;
            }

            Debug.Log($"----------------- BATTLE START {floor} 층 ({levelId}) -----------------");

            // 마스터 카드 생성
            StartCoroutine(BoardSystem.Instance.CreateMasterCard(master));

            // 마스터 크리쳐 카드 생성
            deckSystem.CreateMasterCards(master.StartCardIds);

            // 골드 설정
            BoardSystem.Instance.SetGold(master.Gold);

            // 루시 포만감 UI 설정
            if (master.MasterType == MasterType.Luci)
            {
                satietyUI.gameObject.SetActive(true);

                if (keepSatiety)
                    master.Satiety = PlayerPrefsUtility.GetInt("Satiety", master.Satiety);

                satietyUI.UpdateSatiety(master.Satiety, master.MaxSatiety);
            }

            // 몬스터 카드 생성
            StartCoroutine(BoardSystem.Instance.CreateMonsterCards(levelData.cardIds));

            StartCoroutine(TurnStart());
        }

        void Update()
        {
            ActionQueueProcess();
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

            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);

            List<BSlot> firstSlots = TargetLogic.Instance.TargetFirstEnemy(selfSlot);
            Debug.Log($"TargetFirstEnemy - first Slot: {firstSlots[0].SlotNum}, index: {firstSlots[0].Index}");

            List<BSlot> secondsSlots = TargetLogic.Instance.TargetSecondEnemy(selfSlot);
            Debug.Log($"TargetSecondEnemy - first Slot: {secondsSlots[0].SlotNum}, index: {secondsSlots[0].Index}");

            UpdateSatietyGauge(10);
        }

        public IEnumerator TurnStart()
        {
            yield return new WaitForSeconds(.3f);

            // ToastNotification.Show($"!! TURN START !! ({turnCount})");
            Debug.Log($"----------------- {turnCount} TURN START -----------------");

            // 턴 카운트 설정
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 마스터 마나 리셋
            BoardSystem.Instance.ResetMana(master);

            // 마나 충전
            BoardSystem.Instance.AddMana(master.RechargeMana);

            // 핸드 카드 만들기
            yield return StartCoroutine(deckSystem.MakeHandCards());

            // 핸드 카드 HandOn 어빌리티 액션
            HandOnCardAbilityAction(deckSystem.HandCards);

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
            // 지속 시간 종료 어빌리티 해제
            yield return StartCoroutine(ReleaseCardAbility(BoardSystem.Instance.GetLeftBoardSlots()));
            yield return StartCoroutine(ReleaseCardAbility(BoardSystem.Instance.GetRightBoardSlots()));

            // 카드 액션
            yield return StartCoroutine(TurnEndCardAction());

            // 핸드덱에 카드 제거
            deckSystem.RemoveTurnEndHandCard();

            // 마스터 마나 리셋
            BoardSystem.Instance.ResetMana(master);

            // 턴 다시 시작
            StartCoroutine(TurnStart());

            isTruenEndProcessing = false;

            // 보드 - 턴 카운트 증가
            turnCount += 1;

            BoardSystem.Instance.SetTurnCount(turnCount);
        }

        IEnumerator BattleEnd(bool isWin)
        {
            int nextFloor = 0;
            int locationId = PlayerPrefsUtility.GetInt("LastLocationId", 0);

            if (isWin)
            {
                resultText.text = "YOU WIN";
                // 이기면 층 증가
                nextFloor = floor + 1;

                // 마지막에 선택한 층 인덱스 저장
                PlayerPrefsUtility.SetInt("LastLocationId", locationId);

                if (keepSatiety)
                    PlayerPrefsUtility.SetInt("Satiety", master.Satiety);
            }
            else
            {
                resultText.text = "YOU LOSE";

                PlayerPrefsUtility.SetInt("MasterId", 0);
                PlayerPrefsUtility.SetInt("AreaId", 0);
                PlayerPrefsUtility.SetInt("LevelId", 0);
                PlayerPrefsUtility.SetInt("LastLocationId", 0);
            }

            PlayerPrefsUtility.SetInt("Floor", nextFloor);

            Debug.Log($"배틀 종료 {isWin}, loastLocationId: {locationId}, nextFloor: {nextFloor}");

            yield return new WaitForSeconds(2f);

            GameObject nextSceneObject = GameObject.Find("Scene Manager");

            if (nextSceneObject.TryGetComponent<NextScene>(out NextScene nextScene))
                nextScene.Play(isWin ? "Map" : "Lobby");
        }

        /// <summary>
        /// 턴 시작 액션
        /// </summary>
        IEnumerator TurnStartAction()
        {
            List<BSlot> reactionSlots = BoardSystem.Instance.GetRightBoardSlots();
            List<BSlot> opponentSlots = BoardSystem.Instance.GetLeftBoardSlots();

            foreach (BSlot boardSlot in reactionSlots)
            {
                BaseCard card = boardSlot.Card;

                if (card == null)
                {
                    // Debug.LogWarning($"{boardSlot.Slot}번 슬롯 장착된 카드가 없어 액션 패스");
                    continue;
                }

                // AiGroupData 액션 AiDtaId 얻기
                (AiData aiData, List<BSlot> targetSlots) = AiLogic.Instance.GetTurnStartActionAiDataId(boardSlot, opponentSlots);

                if (aiData == null)
                {
                    Debug.LogWarning($"{boardSlot.LogText} AiGroupData({card.AiGroupId})에 대한 이번 턴({turnCount}) 시작 리액션 안함");
                    continue;
                }

                List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, boardSlot, targetSlots, AbilityWhereFrom.TurnStartAction));

                yield return new WaitForSeconds(turnStartActionDelay);
            }
        }

        IEnumerator TurnEndCardAction()
        {
            List<BSlot> creatureSlots = BoardSystem.Instance.GetLeftBoardSlots();
            yield return StartCoroutine(BoardCardAction(creatureSlots));

            List<BSlot> monsterSlots = BoardSystem.Instance.GetRightBoardSlots();
            yield return StartCoroutine(BoardCardAction(monsterSlots));

            List<BSlot> buildingSlots = BoardSystem.Instance.GetBuildingBoardSlots();
            yield return StartCoroutine(BoardCardAction(buildingSlots));
        }

        /// <summary>
        /// 턴 종료 보드 슬롯 카드 액션
        /// </summary>
        IEnumerator BoardCardAction(List<BSlot> actorSlots)
        {
            foreach (BSlot boardSlot in actorSlots)
            {
                BaseCard card = boardSlot.Card;

                if (card == null)
                {
                    // Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없어 카드 액션 패스");
                    continue;
                }

                if (card.AiGroupId == 0)
                {
                    Debug.LogWarning($"{boardSlot.LogText} AiGroupId 가 {Utils.RedText(card.AiGroupId)}이라서 카드 액션 패스");
                    continue;
                }

                // 카드의 행동 aiData 설정
                int aiDataId = AiLogic.Instance.GetCardAiDataId(card);

                if (aiDataId == 0)
                {
                    Debug.LogWarning($"{boardSlot.LogText} AiGroupData({card.AiGroupId})에 해당하는 aiDataId 얻기 실패");
                    continue;
                }

                // 1. AiData 얻고
                AiData aiData = AiData.GetAiData(aiDataId);

                if (aiData == null)
                {
                    Debug.LogError($"{boardSlot.LogText} AiData({aiDataId}) <color=red>테이블 데이터 없음</color> ");
                    continue;
                }

                // 2. AiData 에 설정된 타겟 얻기
                List<BSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, boardSlot, "AiAbilityProcess");

                if (targetSlots.Count == 0)
                {
                    Debug.LogWarning($"{boardSlot.LogText} 설정 타겟({aiData.target}) 없음 ");
                    continue;
                }

                List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, boardSlot, targetSlots, AbilityWhereFrom.TurnEndBoardSlot));

                yield return new WaitForSeconds(boardCardActionDelay);
            }
        }

        /// <summary>
        /// 핸드에 있을때 효과가 발동되는 카드 액션
        /// </summary>
        void HandOnCardAbilityAction(List<BaseCard> handCards)
        {
            // 핸드 온 카드 액션의 주체는 마스터 슬롯
            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);
            List<(BaseCard card, AiData aiData, List<AbilityData> abilityDatas)> handOnCards = AiLogic.Instance.GetHandOnCards(handCards);

            foreach (var (card, aiData, abilityDatas) in handOnCards)
            {
                List<BSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot, "HandOnCardAbilityAction");

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.TurnStarHandOn));
            }
        }

        public bool HandCardUse(HCard hCard, BSlot targetSlot)
        {
            if (hCard.Card.CardType == CardType.Creature || hCard.Card.CardType == CardType.Building)
            {
                BoardSlotEquipCard(hCard, targetSlot);

                return true;
            }

            if (hCard.Card.CardType == CardType.Magic)
            {
                HandCardUse(hCard.Card.Uid, targetSlot);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 보드 슬롯에 카드 장착
        /// - BSlot OnMouseUp 에서 호출
        /// </summary>
        /// <param name="boardSlot"></param>
        /// <param name="cardUid"></param>
        public void BoardSlotEquipCard(HCard hCard, BSlot boardSlot)
        {
            if (boardSlot.SlotCardType != hCard.Card.CardType)
            {
                hCard.GoBackPosition();
                Debug.LogError($"카드 타입이 일치하지 않습니다. 슬롯 타입: {boardSlot.SlotCardType}, 카드 타입: {hCard.Card.CardType}");
                return;
            }

            // 보드 슬롯에 카드 장착
            boardSlot.EquipCard(hCard.Card);

            // 핸드 카드를 그레이브 덱으로 이동
            deckSystem.HandCardToBoard(hCard.Card);

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, hCard.Card);

            Debug.Log($"보드 슬롯 {boardSlot.SlotNum} 에 카드({hCard.Card.Id}) 장착");
        }

        /// <summary>
        /// 핸드 카드 사용
        // - 카드 사용 주체는 마스터 슬롯
        /// </summary>
        public void HandCardUse(string cardUid, BSlot neareastSlot)
        {
            BaseCard card = deckSystem.FindHandCard(cardUid);

            int aiDataId = AiLogic.Instance.GetCardAiDataId(card);

            AiData aiData = AiData.GetAiData(aiDataId);

            if (aiData == null)
            {
                Debug.LogError($"{card.LogText} 카드 AiData 없음");
                return;
            }

            // 타겟 설정 카드 확인
            bool isSelectAttackType = Constants.SelectAttackTypes.Contains(aiData.attackType);

            // 마법 사용 주체는 마스터 슬롯
            BSlot selfSlot = BoardSystem.Instance.GetMasterSlot();

            // 타겟팅이면 nearastSlot 을 대상으로 설정
            List<BSlot> targetSlots = (aiData.target == AiDataTarget.SelectEnemy) ?
                new List<BSlot> { neareastSlot } :
                TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot, "HandCardUse");

            Debug.Log($"{card.LogText} 사용. isSelectAttackType: {isSelectAttackType}, aiDataId: {aiData.ai_Id}, aiData.target: {aiData.target}, neareastSlot: {neareastSlot?.SlotNum ?? -1}, tagetSlots: {string.Join(", ", targetSlots.Select(slot => slot.SlotNum))}");

            // 대상 선택 사용 카드
            if (isSelectAttackType)
            {
                if (neareastSlot == null)
                {
                    Debug.LogError($"{card.LogText} 마법 대상이 없어서 카드 사용 실패");
                    return;
                }

                if (targetSlots.Contains(neareastSlot) == false)
                {
                    Debug.LogError($"{card.LogText} 대상 슬롯이 아닌 슬롯에 카드 사용 실패");
                    return;
                }
            }

            List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

            foreach (AbilityData abilityData in abilityDatas)
            {
                // 핸드 카드로 공격하는 경우 마스터 공격력 설정. 핸드 카드 동작은 마스터 카드로 하는데....이건 아니다.
                (selfSlot.Card as CreatureCard).SetAttack(aiData.value);

                // 어빌리티 적용
                StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.HandUse));
            }

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, card);

            // 마스터 핸드 카드 제거
            deckSystem.RemoveUsedHandCard(cardUid);
        }

        /// <summary>
        /// 보드 카드 지속시간(duration) 이 0되는 카드 어빌리티 해제
        /// </summary>
        IEnumerator ReleaseCardAbility(List<BSlot> boardSlots)
        {
            foreach (BSlot boardSlot in boardSlots)
            {
                if (boardSlot.Card == null)
                    continue;

                BaseCard card = boardSlot.Card;

                if (card.Abilities.Count == 0)
                {
                    Debug.Log($"{boardSlot.LogText} 해제할 어빌리티 없음");
                    continue;
                }

                Debug.Log($"{boardSlot.LogText} 어빌리티 {card.Abilities.Count} 개");

                for (int i = 0; i < card.Abilities.Count; ++i)
                {
                    CardAbility ability = card.Abilities[i];

                    ability.duration -= 1;

                    AbilityData abilityData = AbilityData.GetAbilityData(ability.abilityId);

                    string releaseLog = $"{boardSlot.LogText} {Utils.AbilityLog(abilityData.abilityType, ability.abilityId)} Duration: {ability.duration}";

                    if (ability.duration > 0)
                    {
                        Debug.Log($"{releaseLog} 남아서 패스");
                        continue;
                    }

                    Debug.Log($"{releaseLog} 으로 해제");

                    BSlot selfBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.selfSlotNum);
                    BSlot targetBoardSlot = BoardSystem.Instance.GetBoardSlot(ability.targetSlotNum);

                    StartCoroutine(AbilityLogic.Instance.AbilityRelease(ability, selfBoardSlot, targetBoardSlot));

                    yield return new WaitForSeconds(abilityReleaseDelay);
                }
            }
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

            if (card.InUse == false)
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
        /// 보드 슬롯에 장착된 카드 제거
        /// </summary>
        /// <param name="boardSlot"></param>
        public IEnumerator RemoveBoardCard(int slotNum)
        {
            BSlot boardSlot = BoardSystem.Instance.GetBoardSlot(slotNum);

            if (boardSlot == null)
            {
                Debug.LogError($"{boardSlot.LogText}. 보드 슬롯 없음");
                yield break;
            }

            Debug.Log($"{boardSlot.LogText} {Utils.RedText("카드 제거")}");

            int monsterCount = BoardSystem.Instance.GetRightBoardSlots().Count(slot => slot.Card != null);

            // 배틀 종료
            if (monsterCount == 0 || boardSlot.SlotNum == 0)
                yield return StartCoroutine(BattleEnd(monsterCount == 0));
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
        /// 테스트용 원거리
        /// </summary>
        public void TestRanged()
        {
            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(7);
            List<BSlot> targetSlots = BoardSystem.Instance.GetBoardSlots(new List<int> { 3, 2, 1 });

            AiData aiData = AiData.GetAiData(1004);
            AbilityData abilityData = AbilityData.GetAbilityData(70004); // 기본 원거리 공격

            StartCoroutine(AbilityLogic.Instance.AbilityAction(aiData, abilityData, selfSlot, targetSlots));
        }

        public void UpdateSatietyGauge(int amount)
        {
            if (amount > 0)
                master.IncreaseSatiety(amount);
            else
                master.DecreaseSatiety(-amount);

            satietyUI.UpdateSatiety(master.Satiety, master.MaxSatiety);
        }

        /// <summary>
        /// 테스트 카드 깜빡임
        /// </summary>
        /// <param name="boardSlot"></param>
        private void FlashingCard(BSlot boardSlot)
        {
            if (boardSlot == null)
                return;

            // boardSlot.StartFlashing();
            // flashingSlots.Add(boardSlot);
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
                // foreach (BSlot slot in flashingSlots)
                //     slot.StopFlashing();

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