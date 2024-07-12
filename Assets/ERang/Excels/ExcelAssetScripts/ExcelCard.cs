using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    [ExcelAsset(AssetPath = "ERang/Resources/ExcelExports")]
    public class ExcelCard : ScriptableObject
    {
        public List<CardEntity> items; // Replace 'EntityType' to an actual type that is serializable.
    }
}
