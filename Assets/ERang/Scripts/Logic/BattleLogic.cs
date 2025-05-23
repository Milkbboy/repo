using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Newtonsoft.Json;
using ERang.Data;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }

        public int turnCount = 1;
        public float turnStartActionDelay = .5f;
        public float boardTurnEndDelay = .5f;
        public float abilityReleaseDelay = .5f;
        public TextMeshProUGUI floorText;
        public TextMeshProUGUI resultText;

        public Master Master => master;

        public SatietyUI satietyUI;

        public Deck deck;
        public CardSelect cardSelect;

        private Master master;
        private MasterCard masterCard;
        private bool isTruenEndProcessing = false;

        private bool keepSatiety;

        // for test
        BaseCard testCard;
        private Queue<NamedAction> actionQueue = new Queue<NamedAction>();
        private List<BSlot> flashingSlots = new List<BSlot>();

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            keepSatiety = PlayerPrefsUtility.GetValue<bool>("KeepSatiety", false);
            Debug.Log($"포만감 저장 여부: {keepSatiety}");
        }

        void Start()
        {
            master = Player.Instance.master;
            // floorText.text = $"{floor} 층\n({levelId}) \n{selectLocation?.eventType ?? EventType.None}";

            BoardSystem.Instance.CreateBoardSlots(master.CreatureSlotCount);

            LevelData levelData = LevelGroupData.GetLevelData(Player.Instance.levelId);

            if (levelData == null)
            {
                Debug.LogError($"레벨({Player.Instance.levelId}) LevelGroupData {Utils.RedText("테이블 데이터 없음")}");
                return;
            }

            Debug.Log($"----------------- BATTLE START {Player.Instance.floor} 층 ({Player.Instance.levelId}) -----------------");

            // 마스터 카드 생성
            StartCoroutine(BoardSystem.Instance.CreateMasterCard(master));
            masterCard = BoardSystem.Instance.MasterCard;

            // 마스터 크리쳐 카드 생성
            deck.CreateMasterCards(master);

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

            // BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);

            // List<BSlot> firstSlots = TargetLogic.Instance.TargetFirstEnemy(selfSlot);
            // Debug.Log($"TargetFirstEnemy - first Slot: {firstSlots[0].SlotNum}, index: {firstSlots[0].Index}");

            // List<BSlot> secondsSlots = TargetLogic.Instance.TargetSecondEnemy(selfSlot);
            // Debug.Log($"TargetSecondEnemy - first Slot: {secondsSlots[0].SlotNum}, index: {secondsSlots[0].Index}");

            // UpdateSatietyGauge(10);

            // StartCoroutine(AbilityTest());
            cardSelect.gameObject.SetActive(true);
            cardSelect.DrawCards(deck.HandCards);
        }

        private IEnumerator AbilityTest()
        {
            int testSlotNum = 0;
            int[] abilityIds = { 70017, 70018, 100024, 100025 };

            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(testSlotNum);
            BSlot targetSlot = BoardSystem.Instance.GetBoardSlot(testSlotNum);

            // 테스트 아쳐 카드 생성 후 테스트 어빌리티 추가
            if (testCard == null)
            {
                // int testCardId = 100201;
                // CardData cardData = CardData.GetCardData(testCardId);
                // testCard = Utils.MakeCard(cardData);
                // testCard.CardType = CardType.Monster;

                // // 테스트 아쳐 카드 장착
                // selfSlot.EquipCard(testCard);

                AiData aiData = AiData.GetAiData(3003);

                List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(new List<int>(abilityIds));

                foreach (AbilityData abilityData in abilityDatas)
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, new List<BSlot> { targetSlot }, AbilityWhereFrom.Test));
            }
            else
            {
                // List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(new List<int>(abilityIds));

                // foreach (AbilityData abilityData in abilityDatas)
                //     StartCoroutine(AbilityLogic.Instance.AbilityAction(abilityData, null, selfSlot, targetSlot));

                yield return StartCoroutine(CardPriorAbility(targetSlot));
                yield return StartCoroutine(CardAiAction(targetSlot));
                yield return StartCoroutine(CardPostAbility(targetSlot));
            }
        }

        public IEnumerator TurnStart()
        {
            yield return new WaitForSeconds(.3f);

            // ToastNotification.Show($"!! TURN START !! ({turnCount})");
            Debug.Log($"----------------- {turnCount} TURN START -----------------");

            // 턴 카운트 설정
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 마스터 행동 시작
            BSlot masterSlot = BoardSystem.Instance.GetMasterSlot();
            yield return StartCoroutine(CardPriorAbility(masterSlot));

            BoardSystem.Instance.SetHp(master.Hp);
            BoardSystem.Instance.SetMana(master.RechargeMana);

            // 핸드 카드 만들기
            yield return StartCoroutine(deck.MakeHandCards());

            // 핸드 카드 HandOn 어빌리티 액션
            yield return HandOnCardAbilityAction(deck.HandCards);

            // 턴 시작시 실행되는 몬스터 카드 Reaction
            StartCoroutine(TurnStartMonsterReaction());
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
            // 크리쳐 카드 행동
            List<BSlot> creatureSlots = BoardSystem.Instance.GetLeftBoardSlots();
            yield return StartCoroutine(BoardTurnEnd(creatureSlots));

            // 마스터 행동 후 어빌리티
            BSlot masterSlot = BoardSystem.Instance.GetMasterSlot();
            yield return StartCoroutine(CardPostAbility(masterSlot));

            // 몬스터 카드 행동
            List<BSlot> monsterSlots = BoardSystem.Instance.GetRightBoardSlots();
            yield return StartCoroutine(BoardTurnEnd(monsterSlots));

            // 건물 카드 행동
            List<BSlot> buildingSlots = BoardSystem.Instance.GetBuildingBoardSlots();
            yield return StartCoroutine(BoardTurnEnd(buildingSlots));

            // 핸드 온 카드 어빌리티 해제
            List<BaseCard> allCards = BoardSystem.Instance.GetAllSlots().Where(slot => slot.Card != null).Select(slot => slot.Card).ToList();
            yield return StartCoroutine(ReleaseHandOnCardAbility(allCards));

            // 지속 시간 종료 어빌리티 해제
            yield return StartCoroutine(ReleaseBoardCardAbility(BoardSystem.Instance.GetLeftBoardSlots()));
            yield return StartCoroutine(ReleaseBoardCardAbility(BoardSystem.Instance.GetRightBoardSlots()));

            // 핸드덱에 카드 제거
            deck.TrunEndProcess();

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
                nextFloor = Player.Instance.floor + 1;
                Player.Instance.SaveMaster(nextFloor, locationId, keepSatiety);
            }
            else
            {
                resultText.text = "YOU LOSE";

                PlayerPrefsUtility.SetInt("MasterId", 0);
                PlayerPrefsUtility.SetInt("LevelId", 0);
                PlayerPrefsUtility.SetInt("LastLocationId", 0);
                PlayerPrefsUtility.SetInt("MasterHp", 0);

                PlayerPrefsUtility.SetInt("AreaId", 0);
                PlayerPrefsUtility.SetString("MasterCards", null);
            }

            Debug.Log($"배틀 종료 {isWin}, loastLocationId: {locationId}, nextFloor: {nextFloor}");

            yield return new WaitForSeconds(2f);

            GameObject nextSceneObject = GameObject.Find("Scene Manager");

            if (nextSceneObject.TryGetComponent<NextScene>(out NextScene nextScene))
                nextScene.Play(isWin ? "Event" : "Lobby");

            PlayerPrefsUtility.SetString("LastScene", "Battle");
        }

        /// <summary>
        /// 턴 시작 액션
        /// </summary>
        IEnumerator TurnStartMonsterReaction()
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
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, boardSlot, targetSlots, AbilityWhereFrom.TurnStartAction));
            }
        }

        /// <summary>
        /// 턴 종료 보드 슬롯 카드 행동
        /// </summary>
        IEnumerator BoardTurnEnd(List<BSlot> actorSlots)
        {
            foreach (BSlot boardSlot in actorSlots)
            {
                // 유저 마스터 카드는 여기서 동작 안하고 따로 함
                if (boardSlot.SlotNum == 0 && boardSlot.SlotCardType == CardType.Master)
                    continue;

                // 카드 액션 전 적용 어빌리티 (기존 어빌리티)
                yield return StartCoroutine(CardPriorAbility(boardSlot));

                // 카드 AI 액션 (신규 어빌리티)
                yield return StartCoroutine(CardAiAction(boardSlot));

                // 카드 액션 후 적용 어빌리티 (기존 어빌리티)
                yield return StartCoroutine(CardPostAbility(boardSlot));
            }
        }

        IEnumerator CardPriorAbility(BSlot boardSlot)
        {
            BaseCard card = boardSlot.Card;

            if (card == null)
            {
                // Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없어 카드 액션 패스");
                yield break;
            }

            foreach (CardAbility cardAbility in card.AbilitySystem.PriorCardAbilities)
            {
                yield return StartCoroutine(AbilityLogic.Instance.AbilityAction(cardAbility, boardSlot, boardSlot));
            }
        }

        IEnumerator CardPostAbility(BSlot boardSlot)
        {
            BaseCard card = boardSlot.Card;

            if (card == null)
            {
                // Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없어 카드 액션 패스");
                yield break;
            }

            foreach (CardAbility cardAbility in card.AbilitySystem.PostCardAbilities)
            {
                yield return StartCoroutine(AbilityLogic.Instance.AbilityAction(cardAbility, boardSlot, boardSlot));
            }
        }

        IEnumerator CardAiAction(BSlot boardSlot)
        {
            BaseCard card = boardSlot.Card;

            if (card == null)
            {
                // Debug.LogWarning($"{boardSlot.Slot}번 슬롯에 카드가 없어 카드 액션 패스");
                yield break;
            }

            if (card.AiGroupId == 0)
            {
                Debug.LogWarning($"{boardSlot.LogText} AiGroupId 가 {Utils.RedText(card.AiGroupId)}이라서 카드 액션 패스");
                yield break;
            }

            // 카드의 행동 aiData 설정
            int aiDataId = AiLogic.Instance.GetCardAiDataId(card);

            if (aiDataId == 0)
            {
                Debug.LogWarning($"{boardSlot.LogText} AiGroupData({card.AiGroupId})에 해당하는 aiDataId 얻기 실패");
                yield break;
            }

            // 1. AiData 얻고
            AiData aiData = AiData.GetAiData(aiDataId);

            if (aiData == null)
            {
                Debug.LogError($"{boardSlot.LogText} AiData({aiDataId}) <color=red>테이블 데이터 없음</color> ");
                yield break;
            }

            // 2. AiData 에 설정된 타겟 얻기
            List<BSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, boardSlot, "CardAiAction");

            if (targetSlots.Count == 0)
            {
                Debug.LogWarning($"{boardSlot.LogText} 설정 타겟({aiData.target}) 없음 ");
                yield break;
            }

            List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

            // 어빌리티 적용
            foreach (AbilityData abilityData in abilityDatas)
                yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, boardSlot, targetSlots, AbilityWhereFrom.TurnEndBoardSlot));

            yield return new WaitForSeconds(boardTurnEndDelay);
        }

        /// <summary>
        /// 핸드에 있을때 효과가 발동되는 카드 액션
        /// </summary>
        IEnumerator HandOnCardAbilityAction(List<BaseCard> handCards)
        {
            // 핸드 온 카드 액션의 주체는 마스터 슬롯
            BSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);
            List<(BaseCard card, AiData aiData, List<AbilityData> abilityDatas)> handOnCards = AiLogic.Instance.GetHandOnCards(handCards);

            foreach (var (card, aiData, abilityDatas) in handOnCards)
            {
                List<BSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot, "HandOnCardAbilityAction");

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.TurnStarHandOn));
            }
        }

        public bool HandCardUse(HCard hCard, BSlot targetSlot)
        {
            if (CanHandCardUse(hCard.Card.Uid) == false)
                return false;

            if (hCard.Card.CardType == CardType.Creature || hCard.Card.CardType == CardType.Building)
            {
                BoardSlotEquipCard(hCard, targetSlot);

                return true;
            }

            if (hCard.Card.CardType == CardType.Magic)
            {
                StartCoroutine(HandCardUse(hCard.Card.Uid, targetSlot));

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
                Debug.LogError($"카드 타입이 일치하지 않아 장착 실패. {boardSlot.SlotCardType}에 {hCard.Card.CardType} 장착 시도");
                return;
            }

            // 핸드 카드 => 보드 카드 이동
            deck.HandCardToBaord(hCard);

            // 보드 슬롯에 카드 장착
            boardSlot.EquipCard(hCard.Card);

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, hCard.Card);

            Debug.Log($"{boardSlot.LogText} 에 {hCard.LogText} 장착");
        }

        /// <summary>
        /// 핸드 카드 사용
        // - 카드 사용 주체는 마스터 슬롯
        /// </summary>
        public IEnumerator HandCardUse(string cardUid, BSlot targetSlot)
        {
            BaseCard card = deck.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 카드({cardUid}) 없음");
                yield break;
            }

            int aiDataId = AiLogic.Instance.GetCardAiDataId(card);

            AiData aiData = AiData.GetAiData(aiDataId);

            if (aiData == null)
            {
                Debug.LogError($"{card.LogText} 카드 AiData 없음");
                yield break;
            }

            // 타겟 설정 카드 확인
            bool isSelectAttackType = Constants.SelectAttackTypes.Contains(aiData.attackType);

            // 마법 사용 주체는 마스터 슬롯
            BSlot selfSlot = BoardSystem.Instance.GetMasterSlot();

            // 타겟팅이면 nearastSlot 을 대상으로 설정
            List<BSlot> targetSlots = (aiData.target == AiDataTarget.SelectEnemy) ?
                new List<BSlot> { targetSlot } :
                TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot, "HandCardUse");

            Debug.Log($"{card.LogText} 사용. isSelectAttackType: {isSelectAttackType}, aiDataId: {aiData.ai_Id}, aiData.target: {aiData.target}, targetSlot: {targetSlot?.SlotNum ?? -1}, tagetSlots: {string.Join(", ", targetSlots.Select(slot => slot.SlotNum))}");

            // 대상 선택 사용 카드
            if (isSelectAttackType)
            {
                if (targetSlot == null)
                {
                    Debug.LogError($"{card.LogText} 마법 대상이 없어서 카드 사용 실패");
                    yield break;
                }

                if (targetSlots.Contains(targetSlot) == false)
                {
                    Debug.LogError($"{card.LogText} 대상 슬롯이 아닌 슬롯에 카드 사용 실패");
                    yield break;
                }
            }

            // 마스터 핸드 카드 제거 먼저 하고 어빌리티 발동 (먼저 삭제하지 않으면 핸드카드 선택 어빌리티에서 보일 수 있음)
            deck.RemoveHandCard(cardUid);

            List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

            // 어빌리티 적용
            foreach (AbilityData abilityData in abilityDatas)
                yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.HandUse));

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, card);
        }

        /// <summary>
        /// 핸드 카드에 있는 OnHand 어빌리티 해제
        /// - Ability 의 duration 상관 없이 바로 해제
        /// </summary>
        IEnumerator ReleaseHandOnCardAbility(List<BaseCard> cards)
        {
            foreach (var card in cards)
            {
                List<CardAbility> onHandCardAbilities = card.AbilitySystem.CardAbilities.Where(ability => ability.workType == AbilityWorkType.OnHand).ToList();

                foreach (var cardAbility in onHandCardAbilities)
                {
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityRelease(cardAbility, AbilityWhereFrom.TurnEndHandOn));

                    card.RemoveCardAbility(cardAbility);
                }
            }
        }

        /// <summary>
        /// 보드 카드 지속시간 감소하고 duration 이 0 이 되면 카드 어빌리티 해제
        /// </summary>
        IEnumerator ReleaseBoardCardAbility(List<BSlot> boardSlots)
        {
            foreach (BSlot boardSlot in boardSlots)
            {
                if (boardSlot.Card == null)
                    continue;

                BaseCard card = boardSlot.Card;

                List<CardAbility> removedCardAbilities = new();

                foreach (CardAbility cardAbility in card.AbilitySystem.CardAbilities)
                {
                    cardAbility.DecreaseDuration();

                    if (cardAbility.duration == 0)
                    {
                        yield return StartCoroutine(AbilityLogic.Instance.AbilityRelease(cardAbility, AbilityWhereFrom.TurnEndBoardSlot));

                        removedCardAbilities.Add(cardAbility);
                    }
                }

                foreach (CardAbility cardAbility in removedCardAbilities)
                {
                    card.RemoveCardAbility(cardAbility);
                }

                boardSlot.DrawAbilityIcons();

                Debug.Log($"{boardSlot.LogText} 해제된 어빌리티 {string.Join(", ", removedCardAbilities.Select(ability => ability.abilityId))} - ReleaseBoardCardAbility");
            }
        }

        /// <summary>
        /// 핸드 카드 사용 가능 확인
        /// </summary>
        public bool CanHandCardUse(string cardUid)
        {
            BaseCard card = deck.FindHandCard(cardUid);

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

            int requiredMana = 0;

            if (card is CreatureCard creatureCard)
                requiredMana = creatureCard.Mana;

            if (card is MagicCard magicCard)
                requiredMana = magicCard.Mana;

            // 필요 마나 확인
            if (masterCard.Mana < requiredMana)
            {
                ToastNotification.Show($"mana({masterCard.Mana}) is not enough");
                Debug.LogWarning($"핸드 카드({card.Id}) 마나 부족으로 사용 불가능({masterCard.Mana} < {requiredMana})");
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
            string extinctionCardIds = string.Join(", ", deck.ExtinctionCards.Select(card => card.Id));
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

            StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.Test));
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