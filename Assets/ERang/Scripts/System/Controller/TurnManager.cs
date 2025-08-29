using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        [Header("턴 설정")]
        public float turnStartActionDelay = 0.5f;
        public float boardTurnEndDelay = 0.5f;

        public int TurnCount => turnCount;
        public bool IsTurnEndProcessing => isTurnEndProcessing;

        public float TurnStartActionDelay
        {
            get => turnStartActionDelay;
            set => turnStartActionDelay = value;
        }

        public float BoardTurnEndDelay
        {
            get => boardTurnEndDelay;
            set => boardTurnEndDelay = value;
        }

        private int turnCount = 1;
        private bool isTurnEndProcessing = false;

        // 외부 시스템 참조 (DI 패턴으로 개선 가능)
        private Player player;
        private MasterCard masterCard;
        private DeckManager deckManager;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void Initialize(Player player, MasterCard masterCard, DeckManager deckManager)
        {
            this.player = player;
            this.masterCard = masterCard;
            this.deckManager = deckManager;
        }

        public IEnumerator StartTurn()
        {
            yield return new WaitForSeconds(0.3f);

            Debug.Log($"----------------- {turnCount} TURN START -----------------");

            // 턴 카운트 설정
            BoardSystem.Instance.SetTurnCount(turnCount);

            // 마스터 행동 시작
            BoardSlot masterSlot = BoardSystem.Instance.GetMasterSlot();
            yield return StartCoroutine(CardPriorAbility(masterSlot));

            BoardSystem.Instance.SetHp(player.Hp);
            BoardSystem.Instance.SetMana(player.RechargeMana);

            // 핸드 카드 만들기
            yield return StartCoroutine(deckManager.MakeHandCards());

            // 핸드 카드 HandOn 어빌리티 액션
            yield return StartCoroutine(HandOnCardAbilityAction(deckManager.Data.HandCards));

            // 턴 시작시 실행되는 몬스터 카드 Reaction
            yield return StartCoroutine(TurnStartMonsterReaction());
        }

        public IEnumerator EndTurn()
        {
            if (isTurnEndProcessing)
            {
                Debug.LogWarning("이미 턴 종료 처리 중");
                yield break;
            }

            isTurnEndProcessing = true;
            Debug.Log($"----------------- {turnCount} TURN END -----------------");

            // 크리쳐 카드 행동
            List<BoardSlot> creatureSlots = BoardSystem.Instance.GetLeftBoardSlots();
            yield return StartCoroutine(BoardTurnEnd(creatureSlots));

            // 마스터 행동 후 어빌리티
            BoardSlot masterSlot = BoardSystem.Instance.GetMasterSlot();
            yield return StartCoroutine(CardPostAbility(masterSlot));

            // 몬스터 카드 행동
            List<BoardSlot> monsterSlots = BoardSystem.Instance.GetRightBoardSlots();
            yield return StartCoroutine(BoardTurnEnd(monsterSlots));

            // 건물 카드 행동
            List<BoardSlot> buildingSlots = BoardSystem.Instance.GetBuildingBoardSlots();
            yield return StartCoroutine(BoardTurnEnd(buildingSlots));

            // 핸드 온 카드 어빌리티 해제
            List<BaseCard> allCards = BoardSystem.Instance.GetAllSlots()
                .Where(slot => slot.Card != null)
                .Select(slot => slot.Card)
                .ToList();
            yield return StartCoroutine(ReleaseHandOnCardAbility(allCards));

            // 지속 시간 종료 어빌리티 해제
            yield return StartCoroutine(ReleaseBoardCardAbility(BoardSystem.Instance.GetLeftBoardSlots()));
            yield return StartCoroutine(ReleaseBoardCardAbility(BoardSystem.Instance.GetRightBoardSlots()));

            // 핸드덱에 카드 제거
            deckManager.TurnEndProcess();

            // 마스터 마나 리셋
            BoardSystem.Instance.ResetMana();

            // 턴 증가
            turnCount++;
            BoardSystem.Instance.SetTurnCount(turnCount);

            isTurnEndProcessing = false;

            // 다음 턴 시작
            StartCoroutine(StartTurn());
        }

        // ========================================
        // 턴 관련 헬퍼 메서드들
        // ========================================

        private IEnumerator TurnStartMonsterReaction()
        {
            List<BoardSlot> reactionSlots = BoardSystem.Instance.GetRightBoardSlots();
            List<BoardSlot> opponentSlots = BoardSystem.Instance.GetLeftBoardSlots();

            foreach (BoardSlot boardSlot in reactionSlots)
            {
                BaseCard card = boardSlot.Card;

                if (card == null)
                    continue;

                foreach (int aiGroupId in card.AiGroupIds)
                {
                    // AiGroupData 액션 AiDtaId 얻기
                    (AiData aiData, List<BoardSlot> targetSlots) = AiLogic.Instance.GetTurnStartActionAiDataId(boardSlot, opponentSlots, aiGroupId);

                    if (aiData == null)
                    {
                        Debug.LogWarning($"{boardSlot.ToSlotLogInfo()} AiGroupData({aiGroupId})에 대한 이번 턴({turnCount}) 시작 리액션 안함");
                        continue;
                    }

                    List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                    // 어빌리티 적용
                    foreach (AbilityData abilityData in abilityDatas)
                        yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, boardSlot, targetSlots, AbilityWhereFrom.TurnStartAction));
                }
            }
        }

        private IEnumerator BoardTurnEnd(List<BoardSlot> actorSlots)
        {
            foreach (BoardSlot boardSlot in actorSlots)
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

        private IEnumerator CardPriorAbility(BoardSlot boardSlot)
        {
            BaseCard card = boardSlot.Card;

            if (card == null)
                yield break;

            foreach (CardAbility cardAbility in card.AbilitySystem.PriorCardAbilities)
            {
                yield return StartCoroutine(AbilityLogic.Instance.AbilityAction(cardAbility, boardSlot, boardSlot));
            }
        }

        private IEnumerator CardPostAbility(BoardSlot boardSlot)
        {
            BaseCard card = boardSlot.Card;

            if (card == null)
                yield break;

            foreach (CardAbility cardAbility in card.AbilitySystem.PostCardAbilities)
            {
                yield return StartCoroutine(AbilityLogic.Instance.AbilityAction(cardAbility, boardSlot, boardSlot));
            }
        }

        private IEnumerator CardAiAction(BoardSlot boardSlot)
        {
            BaseCard card = boardSlot.Card;

            if (card == null)
                yield break;

            if (card.AiGroupIds == null || card.AiGroupIds.Count == 0)
            {
                Debug.LogWarning($"{boardSlot.ToSlotLogInfo()} AiGroupIds 가 {string.Join(", ", card.AiGroupIds)}이라서 카드 액션 패스");
                yield break;
            }

            foreach (int aiGroupId in card.AiGroupIds)
            {
                // 카드의 행동 aiData 설정
                int aiDataId = AiLogic.Instance.GetCardAiDataId(card, aiGroupId);

                if (aiDataId == 0)
                {
                    Debug.LogWarning($"{boardSlot.ToSlotLogInfo()} AiGroupData({aiGroupId})에 해당하는 aiDataId 얻기 실패");
                    continue;
                }

                // 1. AiData 얻고
                AiData aiData = AiData.GetAiData(aiDataId);

                if (aiData == null)
                {
                    Debug.LogError($"{boardSlot.ToSlotLogInfo()} AiData({aiDataId}) <color=red>테이블 데이터 없음</color> ");
                    continue;
                }

                // 2. AiData 에 설정된 타겟 얻기
                List<BoardSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, boardSlot, "CardAiAction");

                if (targetSlots.Count == 0)
                {
                    Debug.LogWarning($"{boardSlot.ToSlotLogInfo()} 설정 타겟({aiData.target}) 없음 ");
                    continue;
                }

                List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, boardSlot, targetSlots, AbilityWhereFrom.TurnEndBoardSlot));

                yield return new WaitForSeconds(boardTurnEndDelay);
            }
        }

        private IEnumerator HandOnCardAbilityAction(IReadOnlyList<BaseCard> handCards)
        {
            // 핸드 온 카드 액션의 주체는 마스터 슬롯
            BoardSlot selfSlot = BoardSystem.Instance.GetBoardSlot(0);
            List<(BaseCard card, AiData aiData, List<AbilityData> abilityDatas)> handOnCards = AiLogic.Instance.GetHandOnCards(handCards);

            foreach (var (card, aiData, abilityDatas) in handOnCards)
            {
                List<BoardSlot> targetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot, "HandOnCardAbilityAction");

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.TurnStarHandOn));
            }
        }

        private IEnumerator ReleaseHandOnCardAbility(IReadOnlyList<BaseCard> cards)
        {
            foreach (var card in cards)
            {
                List<CardAbility> onHandCardAbilities = card.AbilitySystem.CardAbilities
                    .Where(ability => ability.workType == AbilityWorkType.OnHand)
                    .ToList();

                foreach (var cardAbility in onHandCardAbilities)
                {
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityRelease(cardAbility, AbilityWhereFrom.TurnEndHandOn));
                    card.AbilitySystem.RemoveCardAbility(cardAbility);
                }
            }
        }

        private IEnumerator ReleaseBoardCardAbility(List<BoardSlot> boardSlots)
        {
            foreach (BoardSlot boardSlot in boardSlots)
            {
                if (boardSlot.Card == null)
                    continue;

                BaseCard card = boardSlot.Card;
                List<CardAbility> removedCardAbilities = new();

                foreach (CardAbility cardAbility in card.AbilitySystem.CardAbilities)
                {
                    // 턴 종료시 모든 어빌리티 실행
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityAction(cardAbility, boardSlot, boardSlot));

                    // duration이 0이 된 어빌리티는 해제
                    if (cardAbility.duration == 0)
                    {
                        yield return StartCoroutine(AbilityLogic.Instance.AbilityRelease(cardAbility, AbilityWhereFrom.TurnEndBoardSlot));
                        removedCardAbilities.Add(cardAbility);
                    }
                }

                foreach (CardAbility cardAbility in removedCardAbilities)
                {
                    card.AbilitySystem.RemoveCardAbility(cardAbility);
                }

                boardSlot.DrawAbilityIcons();

                Debug.Log($"{boardSlot.ToSlotLogInfo()} 해제된 어빌리티 {string.Join(", ", removedCardAbilities.Select(ability => ability.abilityId))} - ReleaseBoardCardAbility");
            }
        }
    }
}