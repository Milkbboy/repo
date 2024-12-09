using UnityEngine;

namespace ERang
{
    /// <summary>
    /// DeckSystem을 씬 전환 시에도 유지되도록 설정
    /// - Editor Hierachy에 GameObject 생성 후 해당 스크립트를 추가
    /// </summary>
    public class DeckSystemLoader : MonoBehaviour
    {
        public GameObject deckSystemPrefab;

        void Awake()
        {
            if (DeckSystem.Instance == null)
            {
                GameObject deckSystemObject = Instantiate(deckSystemPrefab);
                DontDestroyOnLoad(deckSystemObject);

                // Debug.Log("DeckSystem을 씬 전환 시에도 유지되도록 설정");
            }
        }
    }
}