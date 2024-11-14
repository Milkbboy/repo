using System.Collections.Generic;
using UnityEngine;

namespace ERang.Table
{
    [ExcelAsset(AssetPath = "ERang/Resources/TableExports", LogOnImport = true)]
    public class TextDataTable : ScriptableObject
    {
        public List<TextDataEntity> items; // Replace 'EntityType' to an actual type that is serializable.
    }
}