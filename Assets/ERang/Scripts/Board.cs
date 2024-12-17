using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class Board : MonoBehaviour
    {
        public static Board Instance { get; private set; }

        public readonly CardType[] leftSlotCardTypes = { CardType.Master, CardType.Creature, CardType.Creature, CardType.Creature, CardType.None };
        public readonly CardType[] rightSlotCardTypes = { CardType.None, CardType.Monster, CardType.Monster, CardType.Monster, CardType.Master };
        public readonly CardType[] buildingSlotCardTypes = { CardType.Building, CardType.Building, CardType.None, CardType.None };

        BoardSystem boardSystem;

        void Awake()
        {
            Instance = this;

            boardSystem = BoardSystem.Instance;
        }


    }
}