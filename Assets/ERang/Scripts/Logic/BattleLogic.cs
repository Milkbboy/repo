using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using RogueEngine;
using UnityEngine;

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
            // 보드 슬롯에서 공격 순서대로 크리쳐 카드 얻기
            List<Card> attackerCards = Board.Instance.GetOccupiedCreatureCards();
            List<Card> targetCards = Board.Instance.GetOccupiedMonsterCards();

            if (targetCards.Count == 0)
            {
                Debug.Log("MasterCreatureAction: monsterCards is empty");
                yield break;
            }

            for (int i = 0; i < attackerCards.Count; ++i)
            {
                Card attcker = attackerCards[i];

                // 반사 대미지는 어떻게 하지? 공격자가 영향을 받는 상황인데
                // 타겟이 공격자한테 영향을 주는지 확인 필요

                // 공격자에 영향 받은 카드들 (멀티 타겟 가능)
                List<Card> affectedMonsters = TargetLogic.Instance.CalulateTarget(attcker, targetCards);

                // 영향 받은 카드 설정
                foreach (Card affectedCard in affectedMonsters)
                {
                    BoardSlot boardSlot = Board.Instance.GetMonsterBoardSlot(affectedCard.uid);

                    if (boardSlot == null)
                    {
                        Debug.LogError($"MasterCreatureAction: monsterBoardSlot is null({affectedCard.uid})");
                        continue;
                    }

                    if (affectedCard.hp <= 0)
                    {
                        enemy.RemoveMonsterCard(affectedCard.uid);
                        Board.Instance.ResetBoardSlot(boardSlot.Slot);
                    }
                    else
                    {
                        boardSlot.SetCardHp(affectedCard.hp);
                    }

                    Debug.Log($"MasterCreatureAction: {attcker.id} -> {affectedCard.id}");

                    yield return new WaitForSeconds(1f);
                }
            }
        }

        /// <summary>
        /// 몬스터 카드의 AiData AttckType 은 Automatic 이어야 한다.
        /// </summary>
        /// <returns></returns>
        IEnumerator EnemyMonsterAction()
        {
            // 보드 슬롯 인덱스 6, 7, 8 순으로 공격
            List<BoardSlot> monsterSlots = Board.Instance.GetMonsterSlots();
            List<Card> creatureCards = master.GetCreatureCards();

            for (int i = 0; i < monsterSlots.Count; ++i)
            {
                BoardSlot slot = monsterSlots[i];

                // 카드가 장착되어 있지 않으면 다음 카드로
                if (!slot.IsOccupied)
                {
                    continue;
                }

                // 공격 카드 얻기
                // Card attackerCard = enemy.GetMonsterCard(slot.CardUid);
                List<Card> targetCards = master.GetCreatureCards();

                // 몬스터 카드에 영향을 받는 크리쳐 카드 리스트
            }

            yield return new WaitForSeconds(1f);
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