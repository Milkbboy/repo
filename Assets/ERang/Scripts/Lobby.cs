using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ERang
{
    public class Lobby : MonoBehaviour
    {
        public Transform parentMasterPrefab;
        public GameObject masterPrefab;
        public float spacing = 2f;
        public ToggleGroup toggleGroup; // ToggleGroup 추가

        public int selectedMasterId = 0;

        private NextScene nextScene;

        void Awake()
        {
            nextScene = GetComponent<NextScene>();
        }

        public void NextScene()
        {
            if (selectedMasterId == 0)
                return;

            // 선택된 MasterId를 PlayerPrefs에 저장
            PlayerPrefs.SetInt("SelectedMasterId", selectedMasterId);
            PlayerPrefs.Save();

            nextScene.Play();
        }

        // Start is called before the first frame update
        void Start()
        {
            List<MasterData> masterDatas = MasterData.GetDatas();

            // 프리팹의 RectTransform에서 width 값을 가져옴
            RectTransform prefabRectTransform = masterPrefab.GetComponent<RectTransform>();
            float prefabWidth = prefabRectTransform.rect.width;

            // 시작 위치 계산 (중앙 정렬)
            float totalWidth = (masterDatas.Count - 1) * (prefabWidth + spacing);
            float startX = -totalWidth / 2;

            for (int i = 0; i < masterDatas.Count; i++)
            {
                Vector3 position = new(startX + i * (prefabWidth + spacing), 0, 0);
                // GameObject instance = Instantiate(masterPrefab, position, Quaternion.identity, parentMasterPrefab);
                GameObject instance = Instantiate(masterPrefab);
                instance.transform.SetParent(parentMasterPrefab, false); // 부모 설정
                instance.transform.localPosition = position; // 부모의 로컬 좌표계에서 위치 설정

                // 자식 오브젝트인 Portrait를 찾아 Image 컴포넌트 설정
                Transform portraitTransform = instance.transform.Find("Portrait");
                if (portraitTransform != null)
                {
                    Image portraitImage = portraitTransform.GetComponent<Image>();
                    if (portraitImage != null)
                    {
                        // Texture2D를 Sprite로 변환하여 설정
                        Texture2D masterTexture = masterDatas[i].masterTexture;
                        if (masterTexture != null)
                        {
                            Sprite masterSprite = Sprite.Create(
                                masterTexture,
                                new Rect(0, 0, masterTexture.width, masterTexture.height),
                                new Vector2(0.5f, 0.5f)
                            );
                            portraitImage.sprite = masterSprite; // masterTexture로 변경
                        }

                        // Toggle 컴포넌트를 Portrait에 추가
                        Toggle toggleButton = portraitTransform.gameObject.AddComponent<Toggle>();
                        toggleButton.group = toggleGroup;
                        toggleButton.targetGraphic = portraitImage;

                        // 선택된 상태를 표시하기 위해 색상 변경
                        ColorBlock colors = toggleButton.colors;
                        colors.normalColor = Color.white;
                        colors.selectedColor = Color.green; // 선택된 상태의 색상
                        toggleButton.colors = colors;

                        // 버튼 클릭 이벤트 설정
                        int masterId = masterDatas[i].master_Id; // 클로저 문제를 피하기 위해 로컬 변수 사용
                        toggleButton.onValueChanged.AddListener((isSelected) => ToggleMaster(masterId, isSelected));

                        // Toggle 버튼의 라벨에 캐릭터 이름 설정
                        Transform labelTransform = portraitTransform.Find("Label");
                        if (labelTransform != null)
                        {
                            Text labelText = labelTransform.GetComponent<Text>();
                            if (labelText != null)
                            {
                                labelText.text = masterId.ToString(); // 캐릭터 이름 설정
                            }
                        }
                    }
                }

                // // 자식 오브젝트인 ToggleButton을 찾아 Toggle 컴포넌트 설정
                // Transform toggleButtonTransform = instance.transform.Find("Toggle");

                // if (toggleButtonTransform != null)
                // {
                //     Toggle toggleButton = toggleButtonTransform.GetComponent<Toggle>();
                //     if (toggleButton != null)
                //     {
                //         // ToggleGroup에 추가
                //         toggleButton.group = toggleGroup;

                //         // 버튼 클릭 이벤트 설정
                //         int index = i; // 클로저 문제를 피하기 위해 로컬 변수 사용
                //         toggleButton.onValueChanged.AddListener((isSelected) => ToggleMaster(index, isSelected));

                //         // Toggle 버튼의 라벨에 캐릭터 이름 설정
                //         Transform labelTransform = toggleButtonTransform.Find("Label");
                //         if (labelTransform != null)
                //         {
                //             Text labelText = labelTransform.GetComponent<Text>();
                //             if (labelText != null)
                //             {
                //                 labelText.text = masterDatas[i].master_Id.ToString(); // 캐릭터 이름 설정
                //             }
                //         }
                //     }
                // }
                // else
                // {
                //     Debug.LogError("Portrait or ToggleButton not found in masterPrefab");
                // }
            }

            // Update is called once per frame
            void Update()
            {

            }

            // 마스터의 상태를 토글하는 메서드
            void ToggleMaster(int masterId, bool isSelected)
            {
                if (isSelected)
                {
                    selectedMasterId = masterId;
                    Debug.Log("Selected Master: " + selectedMasterId);
                }
            }
        }
    }
}