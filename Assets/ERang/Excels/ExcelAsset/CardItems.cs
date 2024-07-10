using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
	[ExcelAsset(AssetPath = "ERang/Resources/ExcelExports")]
	public class CardItems : ScriptableObject
	{
		public List<CardEntity> cards; // Replace 'EntityType' to an actual type that is serializable.
	}
}