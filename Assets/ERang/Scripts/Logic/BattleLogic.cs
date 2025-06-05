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

        // ========================================
        // 카드 관리 로직
        // ========================================
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

        public IEnumerator HandCardUse(string cardUid, BSlot targetSlot)
        {
            BaseCard card = deck.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 카드({cardUid}) 없음");
                yield break;
            }

            foreach (int aiGroupId in card.AiGroupIds)
            {
                int aiDataId = AiLogic.Instance.GetCardAiDataId(card, aiGroupId);

                AiData aiData = AiData.GetAiData(aiDataId);

                if (aiData == null)
                {
                    Debug.LogError($"{card.LogText} 카드 AiData({aiDataId}) 없음");
                    continue;
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
                        continue;
                    }

                    if (targetSlots.Contains(targetSlot) == false)
                    {
                        Debug.LogError($"{card.LogText} 대상 슬롯이 아닌 슬롯에 카드 사용 실패");
                        continue;
                    }
                }

                List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                // 어빌리티 적용
                foreach (AbilityData abilityData in abilityDatas)
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.HandUse));
            }

            // 마스터 핸드 카드 제거 먼저 하고 어빌리티 발동 (먼저 삭제하지 않으면 핸드카드 선택 어빌리티에서 보일 수 있음)
            deck.RemoveHandCard(cardUid);

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(master, card);
        }

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