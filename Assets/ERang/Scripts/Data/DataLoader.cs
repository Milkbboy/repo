using UnityEngine;
using ERang.Data;

namespace ERang
{
    /**
    * @brief 데이터를 로드하는 클래스
    */
    public class DataLoader : MonoBehaviour
    {
        void Awake()
        {
            ActData.Load("TableExports/ActDataTable");
            AreaData.Load("TableExports/AreaDataTable");
            LevelGroupData.Load("TableExports/LevelGroupDataTable");
            CardData.Load("TableExports/CardDataTable");
            MonsterCardData.Load("TableExports/MonsterCardDataTable");
            MasterData.Load("TableExports/MasterDataTable");
            AiData.Load("TableExports/AiDataTable");
            AiGroupData.Load("TableExports/AiGroupDataTable");
            ConditionData.Load("TableExports/ConditionDataTable");
            AbilityData.Load("TableExports/AbilityDataTable");
            EventsData.Load("TableExports/EventsDataTable");
            RandomEventsData.Load("TableExports/RandomEventsDataTable");
            TextData.Load("TableExports/TextDataTable");
            RewardSetData.Load("TableExports/RewardSetDataTable");
            RewardData.Load("TableExports/RewardDataTable");
            SummonData.Load("TableExports/SummonDataTable");
        }
    }
}