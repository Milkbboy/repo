using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    [System.Serializable]
    public class Reward 
    {
        public int gold;
        public int xp;
        public List<string> cards = new List<string>();
        public List<string> items = new List<string>();
    }
}
