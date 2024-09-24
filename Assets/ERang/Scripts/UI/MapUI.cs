using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ERang
{
    public class MapUI : MonoBehaviour
    {
        public Button actButton;
        public GameObject actButtonParent;

        // Start is called before the first frame update
        void Start()
        {
            // 액트 버튼 생성
            List<ActData> actDatas = ActData.GetActDatas();

            foreach (ActData actData in actDatas)
            {
                Button button = Instantiate(actButton, actButtonParent.transform);

                if (button == null)
                {
                    Debug.LogError("Button is null");
                    return;
                }

                button.GetComponentInChildren<TextMeshProUGUI>().text = actData.nameDesc;

                button.onClick.AddListener(() =>
                {
                    Debug.Log(actData.nameDesc);
                });
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}