using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.UI;

namespace RogueEngine.FX
{
    /// <summary>
    /// Text number FX that appear when a card receives damage
    /// </summary>

    public class DamageFX : MonoBehaviour
    {
        public Text text_value;

        void Start()
        {

        }

        void Update()
        {

        }

        public void SetValue(int value)
        {
            if (text_value != null)
                text_value.text = value.ToString();
        }

        public void SetValue(string value)
        {
            if (text_value != null)
                text_value.text = value;
        }
    }
}