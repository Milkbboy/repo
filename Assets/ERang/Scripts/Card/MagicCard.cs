using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    // 마법 카드
    public class MagicCard : BaseCard
    {
        public int Atk { get; set; }
        public int Mana { get; set; }

        public MagicCard(CardData cardData) : base(cardData)
        {
            Atk = cardData.atk;
            Mana = cardData.costMana;
        }
    }
}