using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class DataLoader : MonoBehaviour
    {
        void Awake()
        {
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
        }
    }
}