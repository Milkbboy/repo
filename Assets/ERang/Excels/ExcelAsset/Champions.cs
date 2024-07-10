using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
	[ExcelAsset(AssetPath = "ERang/Resources/ExcelExports")]
	public class Champions : ScriptableObject
	{
		public List<ChampionEntity> champions; // Replace 'EntityType' to an actual type that is serializable.
	}
}