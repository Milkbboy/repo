using System.Collections;
using System.Linq;
using System.Collections.Generic;
using ERang.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ERang
{
    public class MapUI : MonoBehaviour
    {
        public Button mapButton;
        public GameObject actButtonParent;
        public GameObject areaButtonParent;
        public GameObject scrollViewContent;
        public GameObject contentPrefab;

        // Start is called before the first frame update
        void Start()
        {
            // 액트 버튼 부모 설정
            VerticalLayoutGroup layoutGroup = actButtonParent.GetComponent<VerticalLayoutGroup>();

            if (layoutGroup == null)
                layoutGroup = actButtonParent.AddComponent<VerticalLayoutGroup>();

            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10;

            // Area 버튼 부모 설정
            VerticalLayoutGroup areaLayoutGroup = areaButtonParent.GetComponent<VerticalLayoutGroup>();

            if (areaLayoutGroup == null)
                areaLayoutGroup = areaButtonParent.AddComponent<VerticalLayoutGroup>();

            areaLayoutGroup.childForceExpandHeight = false;
            areaLayoutGroup.childForceExpandWidth = false;
            areaLayoutGroup.childControlHeight = true;
            areaLayoutGroup.childControlWidth = true;
            areaLayoutGroup.childAlignment = TextAnchor.UpperCenter;
            areaLayoutGroup.spacing = 10;

            // ScrollViewContent 설정
            VerticalLayoutGroup scrollViewLayoutGroup = scrollViewContent.GetComponent<VerticalLayoutGroup>();

            if (scrollViewLayoutGroup == null)
                scrollViewLayoutGroup = scrollViewContent.AddComponent<VerticalLayoutGroup>();

            scrollViewLayoutGroup.childForceExpandHeight = false;
            scrollViewLayoutGroup.childForceExpandWidth = false;
            scrollViewLayoutGroup.childControlHeight = true;
            scrollViewLayoutGroup.childControlWidth = true;
            scrollViewLayoutGroup.childAlignment = TextAnchor.UpperCenter;
            scrollViewLayoutGroup.spacing = 10;

            ContentSizeFitter contentSizeFitter = scrollViewContent.GetComponent<ContentSizeFitter>();

            if (contentSizeFitter == null)
                contentSizeFitter = scrollViewContent.AddComponent<ContentSizeFitter>();

            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 액트 버튼 생성
            List<ActData> actDatas = ActData.GetActDatas();

            foreach (ActData actData in actDatas)
            {
                Button button = Instantiate(mapButton, actButtonParent.transform);

                if (button == null)
                {
                    Debug.LogError("Button is null");
                    return;
                }

                // RectTransform rectTransform = button.GetComponent<RectTransform>();
                // rectTransform.sizeDelta = new Vector2(160, 30);

                LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();

                layoutElement.preferredWidth = 160;
                layoutElement.preferredHeight = 30;

                button.GetComponentInChildren<TextMeshProUGUI>().text = actData.nameDesc;

                button.onClick.AddListener(() =>
                {
                    Debug.Log($"{actData.nameDesc}: {actData.actID}, {string.Join(", ", actData.areaIds)}");

                    MakeFloor(actData);
                });
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void MakeFloor(ActData actData)
        {
            int floor = 0;

            List<(int, int, int, int)> areaFloors = new();

            foreach (int areaId in actData.areaIds)
            {
                AreaData areaData = AreaData.GetAreaData(areaId);

                if (areaData == null) continue;

                for (int i = floor; i < areaData.floorMax; ++i)
                {
                    floor += 1;
                    areaFloors.Add((actData.actID, areaId, floor, areaData.levelGroupId));
                }

                // 마지막 층
                if (areaData.isEnd == true)
                {
                    floor += 1;
                    areaFloors.Add((actData.actID, areaId, floor, areaData.levelGroupId));
                }
            }

            // Debug.Log($"AreaFloors: {string.Join(", ", areaFloors)}");
            CreateFloorButtons(areaFloors);
        }

        /// <summary>
        /// 층 버튼 생성
        /// </summary>
        /// <param name="floors">actId, areaId, floor, levelGroupId</param>
        void CreateFloorButtons(List<(int, int, int, int)> floors)
        {
            // 기존에 생성된 버튼들을 제거
            foreach (Transform child in scrollViewContent.transform)
            {
                Destroy(child.gameObject);
            }

            // areaIds에 해당하는 버튼들을 생성
            foreach ((int actId, int areaId, int floor, int levelGroupId) in floors)
            {
                Button button = Instantiate(mapButton, scrollViewContent.transform);

                if (button == null)
                {
                    Debug.LogError("Button is null");
                    return;
                }

                LayoutElement layoutElement = button.GetComponent<LayoutElement>();

                if (layoutElement == null)
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();

                layoutElement.preferredWidth = 160;
                layoutElement.preferredHeight = 30;

                button.GetComponentInChildren<TextMeshProUGUI>().text = $"{floor}: {levelGroupId}";

                button.onClick.AddListener(() =>
                {
                    Debug.Log($"Area {floor}: {levelGroupId} clicked");
                    LevelGroupData levelGroupData = LevelGroupData.GetLevelGroupData(levelGroupId);

                    LevelData selectedLevelData = SelectRandomLevelData(levelGroupData.levelDatas);

                    // 카드 배치 구하기
                });
            }
        }

        LevelData SelectRandomLevelData(List<LevelData> levelDatas)
        {
            float totalRatio = levelDatas.Sum(x => x.spawnRatio);
            float randomValue = Random.Range(0, totalRatio);
            float cumulativeRatio = 0;

            foreach (var levelData in levelDatas)
            {
                cumulativeRatio += levelData.spawnRatio;

                if (randomValue < cumulativeRatio)
                    return levelData;
            }

            // 기본적으로 첫 번째 levelData 반환 (안전장치)
            return levelDatas[0];
        }
    }
}