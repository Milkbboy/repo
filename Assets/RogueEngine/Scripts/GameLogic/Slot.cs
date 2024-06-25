using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace RogueEngine
{
    /// <summary>
    /// Represent a slot in gameplay (data only)
    /// </summary>

    [System.Serializable]
    public struct Slot : INetworkSerializable
    {
        public int x; //From 1 to 4
        public bool enemy; //false for players, true for enemies

        public static int x_min = 1;
        public static int x_max = 4;

        private static List<Slot> all_slots = new List<Slot>();

        public Slot(int x,bool e)
        {
            this.x = x;
            this.enemy = e;
        }

        public bool IsValid()
        {
            return x >= x_min && x <= x_max;
        }

        //Get a random slot on player side
        public static Slot GetRandom(bool e, System.Random rand)
        {
            return new Slot(rand.Next(x_min, x_max + 1), e);
        }

        //Get a random slot amongts all valid ones
        public static Slot GetRandom(System.Random rand)
        {
            return new Slot(rand.Next(x_min, x_max + 1), rand.Next(0, 2) == 0);
        }
		
		public static Slot Get(int x, bool e)
        {
            List<Slot> slots = GetAll();
            foreach (Slot slot in slots)
            {
                if (slot.x == x && slot.enemy == e)
                    return slot;
            }
            return new Slot(x, e);
        }

        //Get all valid slots
        public static List<Slot> GetAll()
        {
            if (all_slots.Count > 0)
                return all_slots; //Faster access

            for (int e = 0; e <= 1; e++)
            {
                for (int x = x_min; x <= x_max; x++)
                {
                    all_slots.Add(new Slot(x, e == 1));
                }
            }
            return all_slots;
        }

        public static bool operator ==(Slot slot1, Slot slot2)
        {
            return slot1.x == slot2.x && slot1.enemy == slot2.enemy;
        }

        public static bool operator !=(Slot slot1, Slot slot2)
        {
            return slot1.x != slot2.x || slot1.enemy != slot2.enemy;
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref enemy);
        }

        public static Slot None
        {
            get { return new Slot(0, false); }
        }
    }
}
