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

        [Header("UI 참조")]
        public TextMeshProUGUI floorText;
        public TextMeshProUGUI resultText;
        public SatietyUI satietyUI;

        [Header("게임 오브젝트")]
        public Deck deck;
        public CardSelect cardSelect;

        [Header("딜레이 설정")]
        public float abilityReleaseDelay = 0.5f;

        public int TurnCount => turnManager.TurnCount;

        // 게임 상태
        public Master Master => master;
        private Master master;
        private MasterCard masterCard;
        private bool keepSatiety;

        // 턴 매니저 참조
        private TurnManager turnManager;

        // 액션 프로세서 참조
        private ActionProcessor actionProcessor;

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            keepSatiety = PlayerPrefsUtility.GetValue<bool>("KeepSatiety", false);
            Debug.Log($"포만감 저장 여부: {keepSatiety}");
        }

        void Start()
        {
            InitializeGame();
            StartCoroutine(StartBattle());
        }

        private void InitializeGame()
        {
            master = Player.Instance.master;

            // 턴 매니저 초기화
            turnManager = TurnManager.Instance;
            actionProcessor = ActionProcessor.Instance;

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
        }

        public IEnumerator StartBattle()
        {
            LevelData levelData = LevelGroupData.GetLevelData(Player.Instance.levelId);

            // 턴 매니저 초기화
            turnManager.Initialize(master, masterCard, deck);

            // 액션 프로세서 초기화
            actionProcessor.Initialize(master, masterCard, deck);

            // 몬스터 카드 생성
            yield return StartCoroutine(BoardSystem.Instance.CreateMonsterCards(levelData.cardIds));

            // 첫 턴 시작
            yield return StartCoroutine(turnManager.StartTurn());
        }

        /// <summary>
        /// 턴 관리 (TurnManager 로 위임)
        /// Battle Scene => Board => Canvas => Turn => Button 에서 호출
        /// </summary>
        public void TurnEnd()
        {
            StartCoroutine(turnManager.EndTurn());
        }

        // ========================================
        // 카드 관리 로직
        // ========================================
        public bool HandCardUse(HCard hCard, BSlot targetSlot)
        {
            return actionProcessor.UseHandCard(hCard, targetSlot);
        }

        public void BoardSlotEquipCard(HCard hCard, BSlot boardSlot)
        {
            actionProcessor.EquipCardToSlot(hCard, boardSlot);
        }

        public IEnumerator HandCardUse(string cardUid, BSlot targetSlot)
        {
            return actionProcessor.UseHandCard(cardUid, targetSlot);
        }

        public bool CanHandCardUse(string cardUid)
        {
            return actionProcessor.CanUseHandCard(cardUid);
        }

        public IEnumerator RemoveBoardCard(int slotNum)
        {
            yield return StartCoroutine(actionProcessor.RemoveBoardCard(slotNum));
        }

        public IEnumerator BattleEnd(bool isWin)
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

            if (nextSceneObject.TryGetComponent(out NextScene nextScene))
                nextScene.Play(isWin ? "Event" : "Lobby");

            PlayerPrefsUtility.SetString("LastScene", "Battle");
        }

        // ========================================
        // 기타 유틸리티 및 테스트 메서드들
        // ========================================

        public void UpdateSatietyGauge(int amount)
        {
            if (amount > 0)
                master.IncreaseSatiety(amount);
            else
                master.DecreaseSatiety(-amount);

            satietyUI.UpdateSatiety(master.Satiety, master.MaxSatiety);
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
    }
}