using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    [ExcelAsset(AssetPath = "ERang/Resources/ExcelExports")]
    public class ExcelChampion : ScriptableObject
    {
        public List<ChampionEntity> items; // Replace 'EntityType' to an actual type that is serializable.
    }
}
