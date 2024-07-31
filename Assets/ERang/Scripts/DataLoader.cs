using UnityEngine;
using ERang.Data;

namespace ERang
{
    /**
    * @brief 데이터를 로드하는 클래스
    */
    public class DataLoader : MonoBehaviour
    {
        public static DataLoader Instance { get; private set; }

        public int masterId = 1001;
        private Master master;

        void Awake()
        {
            Instance = this;

            CardData.Load("TableExports/CardDataTable");
            MasterData.Load("TableExports/MasterDataTable");
            AiData.Load("TableExports/AiDataTable");
            AiGroupData.Load("TableExports/AiGroupDataTable");
            ConditionData.Load("TableExports/ConditionDataTable");
            AbilityData.Load("TableExports/AbilityDataTable");
        }

        // Start is called before the first frame update
        void Start()
        {
            // 마스터 초기화
            MasterData masterData = MasterData.GetMasterData(masterId);
            master = new Master();

            foreach (int cardId in masterData.startCardIds)
            {
                CardData cardData = CardData.GetCardData(cardId);
                Card card = new Card(cardData);
                master.allCards.Add(card);
                master.deckCards.Add(card);
            }
        }

        public Master GetMaster()
        {
            return master;
        }
    }
}