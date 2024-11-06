using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class MasterCard : CreatureCard, IManaManageable
    {
        public int MaxMana => maxMana;
        private int maxMana;

        public MasterCard(Master master) : base(master)
        {
            maxMana = master.MaxMana;
        }

        public void IncreaseMana(int amount)
        {
            maxMana += amount;
        }

        public void DecreaseMana(int amount)
        {
            maxMana -= amount;
        }
    }
}