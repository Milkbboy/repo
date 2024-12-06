using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class GameEvent : MonoBehaviour
    {
        public GameObject mapEventObject;
        public GameObject rewardObject;

        private DeckSystem deckSystem;

        void Awake()
        {
            TextData.Load("TableExports/TextDataTable");
            CardData.Load("TableExports/CardDataTable");
            LevelGroupData.Load("TableExports/LevelGroupDataTable");
            RewardSetData.Load("TableExports/RewardSetDataTable");
            RewardData.Load("TableExports/RewardDataTable");

            // 시스템 생성
            deckSystem = DeckSystem.Instance;

            if (deckSystem == null)
            {
                Debug.LogError("DeckSystem을 로드하는 데 실패했습니다.");
            }

            mapEventObject.GetComponent<MapEvent>().OnClickNextScene += NextScene;
            rewardObject.GetComponent<Reward>().OnClickNextScene += NextScene;
        }

        void Start()
        {
            // 맵 이벤트 or 보상 이벤트 구분
            string lastScene = PlayerPrefsUtility.GetString("LastScene", "Battle");

            if (lastScene == "Event")
                lastScene = "Battle";

            mapEventObject.SetActive(lastScene == "Map");
            rewardObject.SetActive(lastScene == "Battle");

            Debug.Log($"DeckSystem allCards: {string.Join(", ", deckSystem.AllCards)}");
        }

        public void NextScene()
        {
            PlayerPrefsUtility.SetString("LastScene", "Event");

            NextScene nextScene = GetComponent<NextScene>();
            nextScene.Play("Map");
        }
    }
}