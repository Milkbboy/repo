using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ERang
{
    public class AbilityIconDescUI : MonoBehaviour
    {
        public TextMeshPro desc;

        public void SetDesc(string desc)
        {
            this.desc.text = desc;
        }
    }
}