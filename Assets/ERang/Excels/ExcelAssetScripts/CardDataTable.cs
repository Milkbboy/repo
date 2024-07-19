using System.Collections.Generic;
using UnityEngine;

namespace ERang.Table
{
    [ExcelAsset(AssetPath = "ERang/Resources/TableExports", LogOnImport = true)]
    public class CardDataTable : ScriptableObject
    {
        public List<CardDataEntity> items; // Replace 'EntityType' to an actual type that is serializable.
    }
}
