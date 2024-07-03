using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang;

namespace ERang
{
    public class DataLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CardData.Load();
            Champion.Load();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}