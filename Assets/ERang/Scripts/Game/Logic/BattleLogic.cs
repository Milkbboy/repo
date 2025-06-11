using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ERang.Data;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public static BattleLogic Instance { get; private set; }

        [Header("딜레이 설정")]
        public float abilityReleaseDelay = 0.5f;

        private BattleController battleController;

        public int TurnCount => battleController.TurnCount;

        // 게임 상태
        public Player Player => battleController.Player;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        void Start()
        {
            battleController = BattleController.Instance;

            if (battleController == null)
            {
                Debug.LogError("BattleController 인스턴스가 없습니다.");
                return;
            }
        }

        /// <summary>
        /// 턴 관리 (TurnManager 로 위임)
        /// Battle Scene => Board => Canvas => Turn => Button 에서 호출
        /// </summary>
        public void TurnEnd()
        {
            if (battleController is BattleController bc)
                bc.EndTurn();
        }

        /// <summary>
        /// 배틀 종료
        /// </summary>
        public IEnumerator BattleEnd(bool isWin)
        {
            if (battleController is BattleController bc)
                yield return bc.EndBattle(isWin);
        }

        /// <summary>
        /// 포만감 게이지 업데이트
        /// </summary>
        public void UpdateSatietyGauge(int amount)
        {
            if (battleController is BattleController bc)
                bc.UpdateSatietyGauge(amount);
        }

        // ========================================
        // ActionProcessor 위임 메서드들 (기존 호환성)
        // ========================================

        public bool HandCardUse(HCard hCard, BSlot targetSlot)
        {
            if (battleController is BattleController bc)
                return bc.UseHandCard(hCard, targetSlot);

            return false;
        }

        public void BoardSlotEquipCard(HCard hCard, BSlot boardSlot)
        {
            if (battleController is BattleController bc)
                bc.EquipCardToSlot(hCard, boardSlot);
        }

        public IEnumerator HandCardUse(string cardUid, BSlot targetSlot)
        {
            if (battleController is BattleController bc)
                return bc.UseHandCard(cardUid, targetSlot);

            return null;
        }

        public bool CanHandCardUse(string cardUid)
        {
            if (battleController is BattleController bc)
                return bc.CanUseHandCard(cardUid);

            return false;
        }

        public IEnumerator RemoveBoardCard(int slotNum)
        {
            if (battleController is BattleController bc)
                yield return bc.RemoveBoardCard(slotNum);
        }

        /// <summary>
        /// 소멸 카드 확인
        /// </summary>
        public void ExtinctionCards()
        {
            var deckManager = FindObjectOfType<DeckManager>();

            if (deckManager == null)
            {
                Debug.LogError("Deck 인스턴스가 없습니다.");
                return;
            }

            string extinctionCardIds = string.Join(", ", deckManager.Data.ExtinctionCards.Select(card => card.Id));
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
    }
}