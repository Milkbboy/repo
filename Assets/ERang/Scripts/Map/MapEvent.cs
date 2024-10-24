using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using ERang.Data;

namespace ERang
{
    public class MapEvent : MonoBehaviour
    {
        public TextMeshProUGUI eventTypeText;

        // Start is called before the first frame update
        void Start()
        {
            // 맵 랜덤 이벤트
            string randomEventsJson = PlayerPrefsUtility.GetString("RandomEvents", null);

            if (string.IsNullOrEmpty(randomEventsJson))
            {
                Debug.LogError("랜덤 이벤트가 없습니다.");
                return;
            }

            List<int> randomEvents = JsonConvert.DeserializeObject<List<int>>(randomEventsJson);

            if (randomEvents.Count > 0)
            {
                RandomEventsData randomEventsData = RandomEventsData.GetEventsData(randomEvents[^1]);

                if (randomEventsData)
                    eventTypeText.text = randomEventsData.nameDesc;
            }

            // 층 증가
            int floor = PlayerPrefsUtility.GetInt("Floor", 1);
            PlayerPrefsUtility.SetInt("Floor", floor + 1);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeScene()
        {
            NextScene nextScene = GetComponent<NextScene>();
            nextScene.Play("Map");
        }
    }
}