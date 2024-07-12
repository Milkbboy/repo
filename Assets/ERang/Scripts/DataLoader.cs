using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ERang
{
    public class DataLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CardData.Load("ExcelExports/ExcelCard");
            ChampionData.Load("ExcelExports/ExcelChampion");
        }
    }
}