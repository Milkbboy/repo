using System.Collections.Generic;
using UnityEngine;

namespace ERang.Table
{
    [ExcelAsset(AssetPath = "ERang/Resources/TableExports", LogOnImport = true)]
    public class AbilityDataTable : ScriptableObject
    {
        public List<AbilityDataEntity> items; // Replace 'EntityType' to an actual type that is serializable.
    }
}
