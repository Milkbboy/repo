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
            CardData.Load("TableExports/CardDataTable");
            MonsterCardData.Load("TableExports/MonsterCardDataTable");
            MasterData.Load("TableExports/MasterDataTable");
            AiData.Load("TableExports/AiDataTable");
            AiGroupData.Load("TableExports/AiGroupDataTable");
            ConditionData.Load("TableExports/ConditionDataTable");
            AbilityData.Load("TableExports/AbilityDataTable");
        }
    }
}