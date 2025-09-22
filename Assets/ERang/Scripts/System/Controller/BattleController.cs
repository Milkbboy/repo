using System.Collections;
using UnityEngine;
using TMPro;
using ERang.Data;

namespace ERang
{
    public class BattleController : MonoBehaviour
    {
        public static BattleController Instance { get; private set; }

        [Header("UI 참조")]
        public TextMeshProUGUI floorText;
        public TextMeshProUGUI resultText;
        public SatietyUI satietyUI;

        [Header("게임 오브젝트")]
        public DeckManager deckManager;

        // 인터페이스 구현
        public Player Player => player;
        public int TurnCount => turnManager?.TurnCount ?? 0;

        // 의존성 주입을 위한 인터페이스 참조
        private TurnManager turnManager;
        private ActionProcessor actionProcessor;

        // 게임 상태
        private Player player;
        private MasterCard masterCard;
        private bool keepSatiety;

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            keepSatiety = PlayerDataManager.GetValue(PlayerDataKeys.KeepSatiety);
            Debug.Log($"포만감 저장 여부: {keepSatiety}");
        }

        void Start()
        {
            // ✅ 필수 시스템 초기화 대기
            StartCoroutine(InitializeAndStartBattle());
        }

        /// <summary>
        /// 시스템 초기화 대기 후 배틀 시작
        /// </summary>
        private IEnumerator InitializeAndStartBattle()
        {
            // 필수 시스템들이 초기화될 때까지 대기
            while (AiLogic.Instance == null)
            {
                Debug.Log("🔄 AiLogic.Instance 초기화 대기 중...");
                yield return null;
            }

            Debug.Log("✅ 모든 필수 시스템 초기화 완료. 배틀 시작");

            InitializeBattle();
            yield return StartCoroutine(StartBattle());
        }

        /// <summary>
        /// 배틀 초기화
        /// </summary>
        public void InitializeBattle()
        {
            player = Player.Instance;

            // 의존성 주입 방식으로 변경 (추후 단계에서 개선)
            turnManager = TurnManager.Instance;
            actionProcessor = ActionProcessor.Instance;

            BoardSystem.Instance.CreateBoardSlots(player.CreatureSlotCount);

            LevelData levelData = LevelGroupData.GetLevelData(Player.Instance.levelId);

            if (levelData == null)
            {
                Debug.LogError($"BattleController - InitializeBattle. LevelGroupData({Player.Instance.levelId}) 데이터 없음");
                return;
            }

            Debug.Log($"----------------- BATTLE START {Player.Instance.floor} 층 ({Player.Instance.levelId}) -----------------");

            // 마스터 카드 생성
            StartCoroutine(BoardSystem.Instance.CreateMasterCard(player));
            masterCard = BoardSystem.Instance.MasterCard;

            // 마스터 크리쳐 카드 생성
            deckManager.CreateMasterCards(player);

            // 골드 설정
            BoardSystem.Instance.SetGold(player.Gold);

            // 루시 포만감 UI 설정
            if (player.MasterType == MasterType.Luci)
            {
                satietyUI.gameObject.SetActive(true);

                if (keepSatiety)
                    player.Satiety = PlayerDataManager.GetValue(PlayerDataKeys.Satiety);

                satietyUI.UpdateSatiety(player.Satiety, player.MaxSatiety);
            }
        }

        /// <summary>
        /// 배틀 시작
        /// </summary>
        public IEnumerator StartBattle()
        {
            LevelData levelData = LevelGroupData.GetLevelData(Player.Instance.levelId);

            // 의존성 있는 매니저들 초기화
            turnManager.Initialize(player, masterCard, deckManager);

            actionProcessor.Initialize(player, masterCard, deckManager);

            // 몬스터 카드 생성
            yield return StartCoroutine(BoardSystem.Instance.CreateMonsterCards(levelData.cardIds));

            // 첫 턴 시작
            yield return StartCoroutine(turnManager.StartTurn());
        }

        /// <summary>
        /// 배틀 종료
        /// </summary>
        public IEnumerator EndBattle(bool isWin)
        {
            int nextFloor = 0;
            int locationId = PlayerDataManager.GetValue(PlayerDataKeys.LastLocationId);

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

                PlayerDataManager.SetValues(
                    (PlayerDataKeys.MasterId, 0),
                    (PlayerDataKeys.LevelId, 0),
                    (PlayerDataKeys.LastLocationId, 0),
                    (PlayerDataKeys.MasterHp, 0),
                    (PlayerDataKeys.AreaId, 0),
                    (PlayerDataKeys.MasterCards, null)
                );
            }

            Debug.Log($"배틀 종료 {isWin}, loastLocationId: {locationId}, nextFloor: {nextFloor}");

            yield return new WaitForSeconds(2f);

            GameObject nextSceneObject = GameObject.Find("Scene Manager");

            if (nextSceneObject.TryGetComponent(out NextScene nextScene))
                nextScene.Play(isWin ? "Event" : "Lobby");

            PlayerDataManager.SetValue(PlayerDataKeys.LastScene, "Battle");
        }

        /// <summary>
        /// 포만감 UI 업데이트
        /// </summary>
        public void UpdateSatietyGauge(int amount)
        {
            if (amount > 0)
                player.IncreaseSatiety(amount);
            else
                player.DecreaseSatiety(-amount);

            satietyUI.UpdateSatiety(player.Satiety, player.MaxSatiety);
        }

        /// <summary>
        /// 최대 포만감 설정 및 UI 업데이트
        /// </summary>
        public void UpdateMaxSatietyGauge(int newMaxSatiety)
        {
            player.MaxSatiety = newMaxSatiety;

            // 현재 만복도가 새로운 최대 만복도보다 높다면 조정
            if (player.Satiety > newMaxSatiety)
            {
                player.Satiety = newMaxSatiety;
            }

            satietyUI.UpdateSatiety(player.Satiety, player.MaxSatiety);
        }

        // ========================================
        // TurnManager 위임 메서드들
        // ========================================

        /// <summary>
        /// 턴 종료 (UI에서 호출됨)
        /// Battle Scene => Board => Canvas => Turn => Button 에서 호출
        /// </summary>
        public void EndTurn()
        {
            StartCoroutine(turnManager.EndTurn());
        }

        // ========================================
        // ActionProcessor 위임 메서드들
        // ========================================

        public bool UseHandCard(HandCard handCard, BoardSlot targetSlot)
        {
            return actionProcessor.UseHandCard(handCard, targetSlot);
        }

        public void EquipCardToSlot(HandCard handCard, BoardSlot boardSlot)
        {
            actionProcessor.EquipCardToSlot(handCard, boardSlot);
        }

        public IEnumerator UseHandCard(string cardUid, BoardSlot targetSlot)
        {
            return actionProcessor.UseHandCard(cardUid, targetSlot);
        }

        public bool CanUseHandCard(string cardUid)
        {
            return actionProcessor.CanUseHandCard(cardUid);
        }

        public IEnumerator RemoveBoardCard(int slotNum)
        {
            yield return StartCoroutine(actionProcessor.RemoveBoardCard(slotNum));
        }
    }
}