using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using Newtonsoft.Json;

namespace ERang
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        public MasterCard masterCard;

        public int masterId;
        public int floor;
        public int levelId;
        private MapLocation selectLocation;
        public int AllCardCount => allCards.Count;
        public List<GameCard> AllCards => allCards;

        [SerializeField] private List<GameCard> allCards = new();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                GameLogger.Log(LogCategory.DEBUG, "Player 인스턴스 생성됨");

                LoadMaster();
            }
        }

        public void RecoverHp(int hp)
        {
            masterCard.RecoverHp(hp);
        }

        /// <summary>
        /// 마스터 카드에 카드 추가
        /// </summary>
        /// <param name="cardId"></param>
        public void AddCard(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);

            if (cardData == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ 카드({cardId}) CardData 테이블 데이터 없음");
                return;
            }

            GameCard card = Utils.MakeCard(cardData);
            allCards.Add(card);

            // 카드 타입별로 생성
            masterCard.CardIds.Add(cardId);

            string cardIdsJson = JsonConvert.SerializeObject(masterCard.CardIds);
            PlayerPrefsUtility.SetString("MasterCards", cardIdsJson);
        }

        public void SaveMaster(int nextFloor, int locationId, bool keepSatiety)
        {
            PlayerPrefsUtility.SetInt("Floor", nextFloor);

            PlayerPrefsUtility.SetInt("MasterId", masterId);
            PlayerPrefsUtility.SetInt("LevelId", levelId);
            PlayerPrefsUtility.SetInt("LastLocationId", locationId);
            PlayerPrefsUtility.SetInt("MasterHp", masterCard.State.Hp);
            PlayerPrefsUtility.SetInt("Gold", masterCard.Gold);
            GameLogger.Log(LogCategory.DEBUG, $"저장된 마스터 골드: {masterCard.Gold}");


            if (keepSatiety)
            {
                PlayerPrefsUtility.SetInt("Satiety", masterCard.Satiety);
                GameLogger.Log(LogCategory.DEBUG, $"저장된 마스터 포만도: {masterCard.Satiety}");
            }

            // Master 카드 ids 저장
            string cardIdsJson = JsonConvert.SerializeObject(masterCard.CardIds);
            PlayerPrefsUtility.SetString("MasterCards", cardIdsJson);
            GameLogger.Log(LogCategory.DEBUG, $"저장된 마스터 카드: {cardIdsJson}");

            floor = nextFloor;
        }

        private void LoadMaster()
        {
            masterId = PlayerPrefsUtility.GetInt("MasterId", 1001);
            floor = PlayerPrefsUtility.GetInt("Floor", 1);
            levelId = PlayerPrefsUtility.GetInt("LevelId", 100100101);

            // 마스터
            MasterData masterData = MasterData.GetMasterData(masterId);

            if (masterData == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ 마스터({masterId}) MasterData 테이블 데이터 없음");
                return;
            }

            masterCard = new MasterCard(masterData);

            string selectLocationJson = PlayerPrefsUtility.GetString("SelectLocation", null);

            if (selectLocationJson != null)
            {
                GameLogger.Log(LogCategory.DEBUG, $"저장된 맵 위치: {selectLocationJson}");
                selectLocation = JsonConvert.DeserializeObject<MapLocation>(selectLocationJson);
            }

            GameLogger.Log(LogCategory.DEBUG, $"마스터 인스턴스 생성될 때 카드: {string.Join(", ", masterCard.CardIds)}");

            // 저장된 마스터 HP가 있으면 설정
            int savedHp = PlayerPrefsUtility.GetInt("MasterHp", -1);

            if (savedHp != -1)
            {
                GameLogger.Log(LogCategory.DEBUG, $"저장된 마스터 HP: {savedHp}");
                masterCard.SetHp(savedHp);
            }

            // 저장된 마스터 골드가 있으면 설정
            int savedGold = PlayerPrefsUtility.GetInt("Gold", -1);

            if (savedGold != -1)
            {
                GameLogger.Log(LogCategory.DEBUG, $"저장된 마스터 골드: {savedGold}");
                masterCard.SetGold(savedGold);
            }

            // 저장된 마스터 카드가 있으면 설정
            string savedCardsJson = PlayerPrefsUtility.GetString("MasterCards", null);

            if (!string.IsNullOrEmpty(savedCardsJson))
            {
                GameLogger.Log(LogCategory.DEBUG, $"저장된 마스터 카드: {savedCardsJson}");
                masterCard.SetCardIds(JsonConvert.DeserializeObject<List<int>>(savedCardsJson));
            }
        }
    }
}