using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ERang
{
    public class DataLoader : MonoBehaviour
    {
        void Awake()
        {
            CardData.Load("ExcelExports/ExcelCard");
            ChampionData.Load("ExcelExports/ExcelChampion");
        }
        // Start is called before the first frame update
        void Start()
        {
        }
    }
}