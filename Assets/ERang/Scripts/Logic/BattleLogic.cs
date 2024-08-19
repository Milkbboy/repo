using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ERang.Data;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }

        public HandDeck handDeck;
        public int turn = 0;
        public int masterId = 1001;
        public int handCardCount = 5;

        public Master master { get; private set; }
        public Enemy enemy { get; private set; }

        private System.Random random = new System.Random();

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

            TurnStart();
        }

        // Update is called once per frame
        void Update()
        {

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

            // 턴 시작시 실행되는 카드의 Reaction 을 확인
            TurnStartCardReaction();
        }

        public void TurnEnd()
        {
            turn += 1;

            // 핸드덱에 카드 제거
            for (int i = 0; i < master.handCards.Count; ++i)
            {
                HandDeck.Instance.RemoveCard(master.handCards[i].uid);
                Master.Instance.HandCardToGrave(master.handCards[i].uid);
            }

            // 마스터 데이타 설정 - 마나 증가
            master.IncreaseMana(2);

            // 보드 - 마스터
            Board.Instance.SetMasterStat(master);
            // 보드 - 턴 카운트
            Board.Instance.SetTurnCount(turn);

            // 보드 슬롯 카드 동작
            StartCoroutine(BoardSlotCardAction());

            // 턴 다시 시작
            TurnStart();
        }

        void TurnStartCardReaction()
        {
            // 리액션을 확인하는 카드들
            List<Card> reactionCards = Board.Instance.GetOccupiedMonsterCards();
            // 적 카드들
            List<Card> enemyCards = Board.Instance.GetOccupiedCreatureCards();

            foreach (Card reactionCard in reactionCards)
            {
                Debug.Log($"TurnStartCardReaction: {reactionCard.id}, aiGroupId: {reactionCard.aiGroupId}");

                // 몬스터 카드의 턴 시작 리액션 정보들. AiGroupData 의 reactionCondition 들의 조건을 확인
                List<(AiGroupData.Reaction, ConditionData)> reactionConditionPairs = reactionCard.GetTurnStartReaction();

                // 리액션
                List<string> logs = reactionConditionPairs.Select((x, index) =>
                {
                    return $"[{index}] reaction: {JsonConvert.SerializeObject(x.Item1)}, condition: {JsonConvert.SerializeObject(x.Item2)}";
                }).ToList();

                if (reactionConditionPairs.Count == 0)
                {
                    Debug.Log($"TurnStartCardReaction: {reactionCard.id}, aiGroupId: {reactionCard.aiGroupId}, reactionConditionPairs is empty");
                    continue;
                }

                Debug.Log($"TurnStartCardReaction: {reactionCard.id}, aiGroupId: {reactionCard.aiGroupId}, reactionConditionPairs {string.Join("\n", logs)}");

                // 즉발 행동 대상, 조건 검사. 실행할 AiData 얻기. 즉발 행동 안할 수 있음.
                foreach (var (reaction, condition) in reactionConditionPairs)
                {
                    // 즉발 행동 발동에 사용되는 대상. 자신도 대상 가능.
                    List<Card> targets = TargetLogic.Instance.GetConditionTargets(condition, reactionCard, enemyCards);
                    int aiDataId = ConditionLogic.Instance.GetReactionConditionAiDataId(reaction, condition, targets);

                    if (aiDataId == 0)
                    {
                        Debug.Log($"TurnStartCardReaction: {reactionCard.id}, aiGroupId: {reactionCard.aiGroupId}, reaction: {reaction.conditionId}, aiDataId: {aiDataId}");
                        continue;
                    }

                    Debug.Log($"TurnStartCardReaction: {reactionCard.id}, aiGroupId: {reactionCard.aiGroupId}, reaction: {reaction.conditionId}, aiDataId: {aiDataId}");

                    AiData aiData = AiData.GetAiData(aiDataId);
                }
            }
        }

        // 보드 슬롯 카드 동작
        IEnumerator BoardSlotCardAction()
        {
            StartCoroutine(MasterCreatureAction());

            yield return new WaitForSeconds(1f);

            StartCoroutine(EnemyMonsterAction());
        }

        /// <summary>
        /// 크리쳐의 AiData AttckType 은 Automatic 이어야 한다.
        /// - 유저 인풋이 있는 카드는 AttackType 에 Select 가 있다.
        /// </summary>
        /// <returns></returns>
        IEnumerator MasterCreatureAction()
        {
            List<int> attackerIds = Board.Instance.GetOccupiedCreatureCards().Select(x => x.id).ToList();
            var logAttacker = new { attackerIds };
            List<int> targetIds = Board.Instance.GetOccupiedMonsterCards().Select(x => x.id).ToList();
            var logTarget = new { targetIds };
            Debug.Log($"MasterCreatureAction attackerIds: {JsonConvert.SerializeObject(logAttacker)}, targetIds: {JsonConvert.SerializeObject(logTarget)}");

            return CreateBoardCardAction(
                () => Board.Instance.GetOccupiedCreatureCards(),
                () => Board.Instance.GetOccupiedMonsterCards(),
                (cardUid) => Board.Instance.GetMonsterBoardSlot(cardUid),
                (cardUid) => enemy.RemoveMonsterCard(cardUid),
                (slot) => Board.Instance.ResetMonsterSlot(slot)
            );
        }

        /// <summary>
        /// 몬스터 카드의 AiData AttckType 은 Automatic 이어야 한다.
        /// </summary>
        /// <returns></returns>
        IEnumerator EnemyMonsterAction()
        {
            List<int> attackerIds = Board.Instance.GetOccupiedMonsterCards().Select(x => x.id).ToList();
            var logAttacker = new { attackerIds };
            List<int> targetIds = Board.Instance.GetOccupiedCreatureCards().Select(x => x.id).ToList();
            var logTarget = new { targetIds };
            Debug.Log($"EnemyMonsterAction attackerIds: {JsonConvert.SerializeObject(logAttacker)}, targetIds: {JsonConvert.SerializeObject(logTarget)}");

            return CreateBoardCardAction(
                () => Board.Instance.GetOccupiedMonsterCards(),
                () => Board.Instance.GetOccupiedCreatureCards(),
                (cardUid) => Board.Instance.GetCreatureBoardSlot(cardUid),
                (cardUid) => master.BoardCreatureCardToExtinction(cardUid),
                (slot) => Board.Instance.ResetCreatureSlot(slot)
            );
        }

        IEnumerator CreateBoardCardAction(
            Func<List<Card>> getAttackerCards,
            Func<List<Card>> getTargetCards,
            Func<string, BoardSlot> getBoardSlot,
            Action<string> removeCard,
            Action<int> resetSlot)
        {
            // 보드 슬롯에서 공격 순서대로 카드 얻기
            List<Card> attackerCards = getAttackerCards();

            for (int i = 0; i < attackerCards.Count; ++i)
            {
                Card attacker = attackerCards[i];
                List<Card> targetCards = getTargetCards();

                if (targetCards.Count == 0)
                {
                    Debug.Log("BattleLogic.CreateBoardCardAction: targetCards is empty");
                    yield break;
                }

                // 반사 대미지는 어떻게 하지? 공격자가 영향을 받는 상황인데
                // 타겟이 공격자한테 영향을 주는지 확인 필요
                // 공격자에 영향 받은 카드들 (멀티 타겟 가능)
                List<Card> affectCards = TargetLogic.Instance.CalulateTarget(attacker, targetCards);

                if (affectCards == null)
                {
                    Debug.Log($"BattleLogic.CreateBoardCardAction: {attacker.id} affectCards is null");
                    yield return null;
                }

                // 영향 받은 카드 설정
                foreach (Card affectedCard in affectCards)
                {
                    BoardSlot boardSlot = getBoardSlot(affectedCard.uid);

                    if (boardSlot == null)
                    {
                        Debug.LogError($"BattleLogic.CreateBoardCardAction: boardSlot is null({affectedCard.uid})");
                        continue;
                    }

                    if (affectedCard.hp <= 0)
                    {
                        removeCard(affectedCard.uid);
                        resetSlot(boardSlot.Slot);
                    }
                    else
                    {
                        boardSlot.SetCardHp(affectedCard.hp);
                    }

                    yield return new WaitForSeconds(1f);
                }
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

            Board.Instance.SetMasterStat(master);

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

            Board.Instance.SetMasterStat(master);

            Debug.Log($"HandCardUsed: {cardUid}");
        }
    }
}