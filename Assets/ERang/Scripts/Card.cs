using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class Card
    {
        public CardData cardData;

        public Card(CardData cardData)
        {
            this.cardData = cardData;
        }
    }
}