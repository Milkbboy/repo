using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ERang.Data;

namespace ERang
{
    public class MapUI : MonoBehaviour
    {
        public Button mapButton;
        public GameObject actButtonParent;
        public GameObject areaButtonParent;
        public GameObject scrollViewContent;
        public TextMeshProUGUI levelIDText;
        public TextMeshProUGUI monsterIdsText;
        public TextMeshProUGUI floorText;

        private int actId;
        private int areaId;
        private int floor;
        private int levelId;

        // Start is called before the first frame update
        void Start()
        {
            // 선택된 MasterId를 PlayerPrefs에 저장
            actId = PlayerDataManager.GetValue(PlayerDataKeys.ActId);
            areaId = PlayerDataManager.GetValue(PlayerDataKeys.AreaId);
            floor = PlayerDataManager.GetValue(PlayerDataKeys.Floor);
            levelId = PlayerDataManager.GetValue(PlayerDataKeys.LevelId);

            floorText.text = $"{floor} 층 할 차례";

            // 액트 버튼 부모 설정
            SetupLayoutGroup(actButtonParent);

            // Area 버튼 부모 설정
            SetupLayoutGroup(areaButtonParent);

            // ScrollViewContent 설정
            SetupLayoutGroup(scrollViewContent);

            if (!scrollViewContent.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter))
                contentSizeFitter = scrollViewContent.AddComponent<ContentSizeFitter>();

            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ActData actData = ActData.GetActData(actId);

            if (actId == 0)
                actData = ActData.GetActDatas().First();

            MakeActButton(actData);
        }

        void MakeActButton(ActData actData)
        {
            Button button = Instantiate(mapButton, actButtonParent.transform);

            if (button == null)
            {
                Debug.LogError("Button is null");
                return;
            }

            if (!button.TryGetComponent<LayoutElement>(out var layoutElement))
                layoutElement = button.gameObject.AddComponent<LayoutElement>();

            layoutElement.preferredWidth = 160;
            layoutElement.preferredHeight = 30;

            button.GetComponentInChildren<TextMeshProUGUI>().text = actData.nameDesc;

            button.onClick.AddListener(() =>
            {
                // Debug.Log($"{actData.nameDesc}: {actData.actID}, {string.Join(", ", actData.areaIds)}");
                MakeFloor(actData);
            });

            if (actData.actID == actId)
                button.onClick.Invoke();
        }

        /// <summary>
        /// 층 만들기
        /// </summary>
        /// <param name="actData"></param>
        void MakeFloor(ActData actData)
        {
            AreaData areaData = AreaData.GetAreaDataFromFloor(floor);

            if (areaId == 0)
                areaData = AreaData.GetAreaDatas().First();

            List<(int, int, int, int)> areaFloors = new();

            for (int i = areaData.floorStart; i <= areaData.floorMax; ++i)
            {
                areaFloors.Add((actData.actID, areaData.areaID, i, areaData.levelGroupId));
            }

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
                Destroy(child.gameObject);

            // areaIds에 해당하는 버튼들을 생성
            foreach ((int actId, int areaId, int floor, int levelGroupId) in floors)
            {
                Button button = Instantiate(mapButton, scrollViewContent.transform);

                if (button == null)
                {
                    Debug.LogError("Button is null");
                    return;
                }

                if (!button.TryGetComponent<LayoutElement>(out var layoutElement))
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();

                layoutElement.preferredWidth = 160;
                layoutElement.preferredHeight = 30;

                button.GetComponentInChildren<TextMeshProUGUI>().text = $"{floor}: {levelGroupId}";

                button.onClick.AddListener(() =>
                {
                    // Debug.Log($"Area {floor}: {levelGroupId} clicked");
                    LevelGroupData levelGroupData = LevelGroupData.GetLevelGroupData(levelGroupId);

                    LevelData selectedLevelData = SelectRandomLevelData(levelGroupData.levelDatas);

                    var levelIds = levelGroupData.levelDatas.Select(levelData => levelData.levelId == selectedLevelData.levelId ? $"<color=#dd3333>{levelData.levelId}</color> <= 선택" : levelData.levelId.ToString()).ToList();

                    // 카드 배치 구하기
                    // Debug.Log($"{string.Join(", ", levelIds)}. 카드 배치: {string.Join(", ", selectedLevelData.cardIds)}");

                    levelIDText.text = $"Level ID\n{string.Join("\n", levelIds)}";

                    List<(int, string, int)> cardDataList = new();

                    for (int i = 0; i < selectedLevelData.cardIds.Count(); ++i)
                    {
                        int pos = i + 1;
                        int cardId = selectedLevelData.cardIds[i];

                        if (cardId == 0)
                        {
                            cardDataList.Add((pos, "빈자리", 0));
                            continue;
                        }

                        CardData monsterCardData = CardData.GetCardData(cardId);

                        if (monsterCardData == null)
                        {
                            Debug.LogError($"카드 데이터 없음: {cardId}");
                            continue;
                        }

                        cardDataList.Add((pos, monsterCardData.nameDesc, monsterCardData.card_id));
                    }

                    monsterIdsText.text = $"등장 카드들 \n{string.Join("\n", cardDataList.Select(x => $"{x.Item1}: {x.Item2}({x.Item3})").ToList())}";

                    PlayerDataManager.SetValues(
                        (PlayerDataKeys.ActId, actId),
                        (PlayerDataKeys.AreaId, areaId),
                        (PlayerDataKeys.Floor, floor),
                        (PlayerDataKeys.LevelId, selectedLevelData.levelId)
                    );
                });

                // 현재 층과 같은 층의 버튼을 깜빡이게 설정
                if (floor == this.floor)
                {
                    StartCoroutine(BlinkButton(button));
                }
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

        private void SetupLayoutGroup(GameObject parent)
        {
            if (!parent.TryGetComponent<VerticalLayoutGroup>(out var layoutGroup))
                layoutGroup = parent.AddComponent<VerticalLayoutGroup>();

            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10;
        }

        IEnumerator BlinkButton(Button button)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage == null)
                yield break;

            Color originalColor = buttonImage.color;
            Color blinkColor = Color.green; // 깜빡일 때 사용할 색상

            while (true)
            {
                buttonImage.color = blinkColor;
                yield return new WaitForSeconds(0.5f);
                buttonImage.color = originalColor;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}