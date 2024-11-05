using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class BuildingCard : BaseCard
    {
        public int Gold { get; set; }

        public BuildingCard(CardData cardData) : base(cardData)
        {
            Gold = cardData.costGold;
        }
    }
}