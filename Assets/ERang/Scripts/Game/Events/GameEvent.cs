using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class GameEvent : MonoBehaviour
    {
        public GameObject mapEventObject;
        public GameObject rewardObject;

        public DeckSystem deckSystem;

        void Awake()
        {
            TextData.Load("TableExports/TextDataTable");
            CardData.Load("TableExports/CardDataTable");
            LevelGroupData.Load("TableExports/LevelGroupDataTable");
            RewardSetData.Load("TableExports/RewardSetDataTable");
            RewardData.Load("TableExports/RewardDataTable");

            mapEventObject.GetComponent<MapEvent>().OnClickNextScene += NextScene;
            rewardObject.GetComponent<RewardEvent>().OnClickNextScene += NextScene;
        }

        void Start()
        {
            // 맵 이벤트 or 보상 이벤트 구분
            string lastScene = PlayerPrefsUtility.GetString("LastScene", "Battle");

            if (lastScene == "Event")
                lastScene = "Battle";

            mapEventObject.SetActive(lastScene == "Map");
            rewardObject.SetActive(lastScene == "Battle");

            Debug.Log($"DeckSystem allCards: {string.Join(", ", Player.Instance.AllCards)}");
        }

        public void NextScene()
        {
            PlayerPrefsUtility.SetString("LastScene", "Event");

            NextScene nextScene = GetComponent<NextScene>();
            nextScene.Play("Map");
        }
    }
}