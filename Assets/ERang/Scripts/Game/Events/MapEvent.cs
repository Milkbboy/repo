using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using TMPro;
using ERang.Data;

namespace ERang
{
    public class MapEvent : MonoBehaviour
    {
        public TextMeshProUGUI eventTypeText;
        public UnityAction OnClickNextScene;

        void Start()
        {
            // 맵 랜덤 이벤트
            string randomEventsJson = PlayerDataManager.GetValue(PlayerDataKeys.RandomEvents);

            if (string.IsNullOrEmpty(randomEventsJson))
            {
                Debug.LogError("랜덤 이벤트가 없습니다.");
                return;
            }

            List<int> randomEvents = JsonConvert.DeserializeObject<List<int>>(randomEventsJson);

            if (randomEvents.Count > 0)
            {
                RandomEventsData randomEventsData = RandomEventsData.GetEventsData(randomEvents[^1]);

                if (randomEventsData != null)
                    eventTypeText.text = randomEventsData.nameDesc;
            }

            // 층 증가
            int floor = PlayerDataManager.GetValue(PlayerDataKeys.Floor);
            Player.Instance.floor = floor + 1;

            PlayerDataManager.SetValue(PlayerDataKeys.Floor, Player.Instance.floor);
        }

        public void ChangeScene()
        {
            OnClickNextScene?.Invoke();
        }
    }
}