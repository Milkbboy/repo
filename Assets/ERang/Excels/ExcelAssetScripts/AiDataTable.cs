using System.Collections.Generic;
using UnityEngine;

namespace ERang.Table
{
    [ExcelAsset(AssetPath = "ERang/Resources/TableExports", LogOnImport = true)]
    public class AiDataTable : ScriptableObject
    {
        public List<AiDataEntity> items; // Replace 'EntityType' to an actual type that is serializable.
    }
}
