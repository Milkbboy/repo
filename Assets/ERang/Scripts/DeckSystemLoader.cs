using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class DeckSystemLoader : MonoBehaviour
    {
        public GameObject deckSystemPrefab;

        void Awake()
        {
            if (DeckSystem.Instance == null)
            {
                GameObject deckSystemObject = Instantiate(deckSystemPrefab);
                DontDestroyOnLoad(deckSystemObject);

                Debug.Log("DeckSystem을 씬 전환 시에도 유지되도록 설정");
            }
        }
    }
}