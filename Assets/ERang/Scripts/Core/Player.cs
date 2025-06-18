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
        public string NameDesc => nameDesc;
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
        private string nameDesc;
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
            gold += amount;
        }

        public void IncreaseSatiety(int amount)
        {
            satiety = Mathf.Clamp(satiety + amount, 0, maxSatiety);
        }

        public void DecreaseSatiety(int amount)
        {
            satiety = Mathf.Clamp(satiety - amount, 0, maxSatiety);
        }

        public void SetGold(int amount) => Gold = amount;
        public void RecoverHp(int amount) => Stat.RestoreHealth(amount);
        #endregion

        #region 카드 관리
        public void AddCard(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);
            if (cardData == null)
            {
                Debug.LogError($"Player - AddCard. CardData({cardId}) 데이터 없음");
                return;
            }

            CardFactory cardFactory = new(AiLogic.Instance);
            BaseCard card = cardFactory.CreateCard(cardData);
            allCards.Add(card);
            // 카드 타입별로 생성
            CardIds.Add(cardId);

            string cardIdsJson = JsonConvert.SerializeObject(CardIds);
            PlayerDataManager.SetValue(PlayerDataKeys.MasterCards, cardIdsJson);
        }
        #endregion

        #region 저장 및 불러오기
        public void SaveMaster(int nextFloor, int locationId, bool keepSatiety)
        {
            PlayerDataManager.SetValues(
                (PlayerDataKeys.MasterId, masterId),
                (PlayerDataKeys.Floor, floor),
                (PlayerDataKeys.LevelId, levelId),
                (PlayerDataKeys.LastLocationId, locationId),
                (PlayerDataKeys.MasterHp, Hp),
                (PlayerDataKeys.Gold, Gold)
            );

            if (keepSatiety)
                PlayerDataManager.SetValue(PlayerDataKeys.Satiety, Satiety);

            // Master 카드 ids 저장
            string cardIdsJson = JsonConvert.SerializeObject(CardIds);
            PlayerDataManager.SetValue(PlayerDataKeys.MasterCards, cardIdsJson);

            floor = nextFloor;
        }

        private void LoadPlayer()
        {
            masterId = PlayerDataManager.GetValue(PlayerDataKeys.MasterId);
            floor = PlayerDataManager.GetValue(PlayerDataKeys.Floor);
            levelId = PlayerDataManager.GetValue(PlayerDataKeys.LevelId);

            // 마스터
            MasterData masterData = MasterData.GetMasterData(masterId);
            if (masterData == null)
            {
                Debug.LogError($"Player - LoadPlayer. MasterData({masterId}) 데이터 없음");
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
            nameDesc = masterData.nameDesc;
            masterTexture = masterData.masterTexture;

            Stat = new CardStat(masterData.hp, masterData.def, masterData.startMana, masterData.atk, masterData.hp, masterData.maxMana);

            Debug.Log($"MasterData 데이터 먼저 설정: masterId: {masterId}, masterType: {masterType}, 카드: {string.Join(", ", cardIds)}");
        }

        private void LoadSavedPlayerData()
        {
            string selectLocationJson = PlayerDataManager.GetValue(PlayerDataKeys.SelectLocation);
            if (selectLocationJson != null)
                selectLocation = JsonConvert.DeserializeObject<MapLocation>(selectLocationJson);

            // 저장된 마스터 HP가 있으면 설정
            int savedHp = PlayerDataManager.GetValue(PlayerDataKeys.MasterHp);

            if (savedHp != -1)
            {
                Debug.Log($"저장된 마스터 HP 로드: {savedHp}");
                Stat.SetHp(savedHp);
            }

            // 저장된 마스터 골드가 있으면 설정
            int savedGold = PlayerDataManager.GetValue(PlayerDataKeys.Gold);

            if (savedGold != -1)
            {
                Debug.Log($"저장된 마스터 골드 로드: {savedGold}");
                SetGold(savedGold);
            }

            // 저장된 마스터 카드가 있으면 설정
            string savedCardsJson = PlayerDataManager.GetValue(PlayerDataKeys.MasterCards);

            if (!string.IsNullOrEmpty(savedCardsJson))
            {
                Debug.Log($"저장된 마스터 카드 로드: {savedCardsJson}");
                CardIds = JsonConvert.DeserializeObject<List<int>>(savedCardsJson);
            }
        }
        #endregion
    }
}