using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using Newtonsoft.Json;

namespace ERang
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        #region 마스터 정보
        // 기본 정보
        public int MasterId => masterId;
        public MasterType MasterType => masterType;
        public Texture2D CardImage => masterTexture;
        public List<int> CardIds { get => cardIds; set => cardIds = value; }

        // CardState 스탯 관리
        public CardStat Stat { get; protected set; }
        public int Hp => Stat.Hp;
        public int MaxHp => Stat.MaxHp;
        public int Mana => Stat.Mana;
        public int MaxMana => Stat.MaxMana;
        public int Atk => Stat.Atk;
        public int Def => Stat.Def;

        // 추가 속성들
        public int RechargeMana { get => rechargeMana; set => rechargeMana = value; }
        public int Gold { get => gold; set => gold = value; }
        public int CreatureSlotCount => creatureSlots;
        public int Satiety { get => satiety; set => satiety = value; }
        public int MaxSatiety => maxSatiety;

        private int masterId;
        private MasterType masterType;
        private List<int> cardIds = new();
        private int rechargeMana;
        private int gold;
        private int creatureSlots;
        private int satiety;
        private int maxSatiety;
        private Texture2D masterTexture;
        #endregion

        #region 게임 진행 상황
        public int floor;
        public int levelId;
        private MapLocation selectLocation;
        public int AllCardCount => allCards.Count;
        public List<BaseCard> AllCards => allCards;

        [SerializeField] private List<BaseCard> allCards = new();
        #endregion

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("Player 생성됨");

                LoadPlayer();
            }
        }

        #region 마스터 기능
        public void AddGold(int amount)
        {
            int oldGold = gold;
            gold += amount;
            Debug.Log($"<color=#257dca>Add Gold({gold}): {oldGold} -> {gold}</color>");
        }

        public void IncreaseSatiety(int amount)
        {
            int oldSatiety = satiety;
            satiety = Mathf.Clamp(satiety + amount, 0, maxSatiety);
            Debug.Log($"<color=#257dca>만복감 증가({amount}): {oldSatiety} -> {satiety}</color>");
        }

        public void DecreaseSatiety(int amount)
        {
            int oldSatiety = satiety;
            satiety = Mathf.Clamp(satiety - amount, 0, maxSatiety);
            Debug.Log($"<color=#257dca>만복감 감소({amount}): {oldSatiety} -> {satiety}</color>");
        }

        public void SetHp(int amount) => Stat.SetHp(amount);
        public void SetGold(int amount) => Gold = amount;
        public void SetMana(int amount) => Stat.SetMana(amount);
        public void RecoverHp(int amount) => Stat.RestoreHealth(amount);
        #endregion

        #region 카드 관리
        public void AddCard(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);

            if (cardData == null)
            {
                Debug.LogError($"CardData 테이블에 {Utils.RedText(cardId)} 카드 없음 - AddCard");
                return;
            }

            CardFactory cardFactory = new(AiLogic.Instance);
            BaseCard card = cardFactory.CreateCard(cardData);
            allCards.Add(card);
            // 카드 타입별로 생성
            CardIds.Add(cardId);

            string cardIdsJson = JsonConvert.SerializeObject(CardIds);
            PlayerPrefsUtility.SetString("MasterCards", cardIdsJson);
        }
        #endregion

        #region 저장 및 불러오기
        public void SaveMaster(int nextFloor, int locationId, bool keepSatiety)
        {
            PlayerPrefsUtility.SetInt("Floor", nextFloor);
            PlayerPrefsUtility.SetInt("MasterId", masterId);
            PlayerPrefsUtility.SetInt("LevelId", levelId);
            PlayerPrefsUtility.SetInt("LastLocationId", locationId);
            PlayerPrefsUtility.SetInt("MasterHp", Hp);
            PlayerPrefsUtility.SetInt("Gold", Gold);

            if (keepSatiety)
                PlayerPrefsUtility.SetInt("Satiety", Satiety);

            // Master 카드 ids 저장
            string cardIdsJson = JsonConvert.SerializeObject(CardIds);
            PlayerPrefsUtility.SetString("MasterCards", cardIdsJson);

            floor = nextFloor;
        }

        private void LoadPlayer()
        {
            masterId = PlayerPrefsUtility.GetInt("MasterId", 1001);
            floor = PlayerPrefsUtility.GetInt("Floor", 1);
            levelId = PlayerPrefsUtility.GetInt("LevelId", 100100101);

            // 마스터
            MasterData masterData = MasterData.GetMasterData(masterId);
            if (masterData == null)
            {
                Debug.LogError($"마스터({masterId}) MasterData {Utils.RedText("테이블 데이터 없음")}");
                return;
            }

            InitializeMasterData(masterData);
            LoadSavedPlayerData();
        }

        private void InitializeMasterData(MasterData masterData)
        {
            masterType = masterData.masterType;
            rechargeMana = masterData.rechargeMana;
            gold = 1000; // 임시
            creatureSlots = masterData.creatureSlots;
            satiety = masterData.satietyGauge;
            maxSatiety = masterData.maxSatietyGauge;
            cardIds = masterData.startCardIds;
            masterTexture = masterData.masterTexture;

            Stat = new CardStat(masterData.hp, masterData.def, masterData.startMana, masterData.atk, masterData.hp, masterData.maxMana);

            Debug.Log($"MasterData 데이터 먼저 설정: masterId: {masterId}, masterType: {masterType}, 카드: {string.Join(", ", cardIds)}");
        }

        private void LoadSavedPlayerData()
        {
            string selectLocationJson = PlayerPrefsUtility.GetString("SelectLocation", null);
            if (selectLocationJson != null)
                selectLocation = JsonConvert.DeserializeObject<MapLocation>(selectLocationJson);

            // 저장된 마스터 HP가 있으면 설정
            int savedHp = PlayerPrefsUtility.GetInt("MasterHp", -1);

            if (savedHp != -1)
            {
                Debug.Log($"저장된 마스터 HP 로드: {savedHp}");
                SetHp(savedHp);
            }

            // 저장된 마스터 골드가 있으면 설정
            int savedGold = PlayerPrefsUtility.GetInt("Gold", -1);

            if (savedGold != -1)
            {
                Debug.Log($"저장된 마스터 골드 로드: {savedGold}");
                SetGold(savedGold);
            }

            // 저장된 마스터 카드가 있으면 설정
            string savedCardsJson = PlayerPrefsUtility.GetString("MasterCards", null);

            if (!string.IsNullOrEmpty(savedCardsJson))
            {
                Debug.Log($"저장된 마스터 카드 로드: {savedCardsJson}");
                CardIds = JsonConvert.DeserializeObject<List<int>>(savedCardsJson);
            }
        }
        #endregion
    }
}