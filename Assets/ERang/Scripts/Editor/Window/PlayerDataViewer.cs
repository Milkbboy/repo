#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace ERang
{
    public class PlayerDataViewer : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, object> cachedValues = new();
        private LocationDataParser locationParser = new();
        private string searchFilter = "";
        private PlayerDataGroup selectedGroup = PlayerDataGroup.All;

        private enum PlayerDataGroup
        {
            All,
            Master,
            Progress,
            Settings,
            Map
        }

        [MenuItem("ERang/0.Player Data Viewer")]
        public static void ShowWindow()
        {
            // 기본값 초기화
            if (!PlayerDataManager.HasKey(PlayerDataKeys.KeepSatiety))
                PlayerDataManager.SetValue(PlayerDataKeys.KeepSatiety, false);

            var window = GetWindow<PlayerDataViewer>("Player Data Viewer");
            window.titleContent = new GUIContent("🎮 Player Data", "플레이어 데이터 관리 도구");
            window.RefreshData();
        }

        private void OnEnable() => RefreshData();

        private void OnGUI()
        {
            DrawHeader();
            DrawFilters();
            EditorGUILayout.Space(10);
            DrawDataSection();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal("box");

            GUILayout.Label("🎮 Player Data Manager", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();

            // 새로고침 버튼
            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80), GUILayout.Height(25)))
                RefreshData();

            // 초기화 버튼 (확인 다이얼로그)
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("🗑️ Reset All", GUILayout.Width(80), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("데이터 초기화",
                    "모든 플레이어 데이터를 초기화하시겠습니까?\n(일부 설정 제외)",
                    "초기화", "취소"))
                {
                    PlayerDataManager.DeleteAllExcept();
                    RefreshData();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();

            // 검색 필터
            GUILayout.Label("🔍", GUILayout.Width(20));
            string newFilter = EditorGUILayout.TextField(searchFilter, GUILayout.Width(200));
            if (newFilter != searchFilter)
            {
                searchFilter = newFilter;
                RefreshData();
            }

            GUILayout.Space(20);

            // 그룹 필터
            GUILayout.Label("📂", GUILayout.Width(20));
            PlayerDataGroup newGroup = (PlayerDataGroup)EditorGUILayout.EnumPopup(selectedGroup, GUILayout.Width(100));
            if (newGroup != selectedGroup)
            {
                selectedGroup = newGroup;
                RefreshData();
            }

            GUILayout.FlexibleSpace();

            // 데이터 상태 표시
            if (PlayerDataManager.ValidateData())
            {
                GUILayout.Label("✅ 데이터 정상", EditorStyles.helpBox);
            }
            else
            {
                GUI.color = Color.yellow;
                GUILayout.Label("⚠️ 데이터 이상", EditorStyles.helpBox);
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawDataSection()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var filteredKeys = GetFilteredKeys().ToList();

            if (!filteredKeys.Any())
            {
                EditorGUILayout.HelpBox("조건에 맞는 데이터가 없습니다.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            // 그룹별로 데이터 표시
            DrawMasterData(filteredKeys);
            DrawProgressData(filteredKeys);
            DrawMapData(filteredKeys);
            DrawSettingsData(filteredKeys);
            DrawOtherData(filteredKeys);

            EditorGUILayout.EndScrollView();
        }

        private void DrawMasterData(List<string> keys)
        {
            var masterKeys = keys.Where(k => k.StartsWith("Master")).ToList();
            if (!masterKeys.Any()) return;

            DrawGroupHeader("👤 Master Data", masterKeys.Count);
            foreach (var key in masterKeys)
                DrawDataField(key);
        }

        private void DrawProgressData(List<string> keys)
        {
            var progressKeys = keys.Where(k => k.Contains("Floor") || k.Contains("Level") || k.Contains("Act") || k.Contains("Area")).ToList();
            if (!progressKeys.Any()) return;

            DrawGroupHeader("📈 Progress Data", progressKeys.Count);
            foreach (var key in progressKeys)
                DrawDataField(key);
        }

        private void DrawMapData(List<string> keys)
        {
            var mapKeys = keys.Where(k => k.Contains("Location") || k.Contains("Depth")).ToList();
            if (!mapKeys.Any()) return;

            DrawGroupHeader("🗺️ Map Data", mapKeys.Count);

            foreach (var key in mapKeys)
            {
                if (key == "Locations")
                    DrawLocationData();
                else if (key == "SelectedDepthIndies")
                    DrawSelectedDepthData();
                else
                    DrawDataField(key);
            }
        }

        private void DrawSettingsData(List<string> keys)
        {
            var settingKeys = keys.Where(k => k.Contains("Keep") || k.Contains("Last")).ToList();
            if (!settingKeys.Any()) return;

            DrawGroupHeader("⚙️ Settings", settingKeys.Count);
            foreach (var key in settingKeys)
                DrawDataField(key);
        }

        private void DrawOtherData(List<string> keys)
        {
            var otherKeys = keys.Where(k =>
                !k.StartsWith("Master") &&
                !k.Contains("Floor") && !k.Contains("Level") && !k.Contains("Act") && !k.Contains("Area") &&
                !k.Contains("Location") && !k.Contains("Depth") &&
                !k.Contains("Keep") && !k.Contains("Last")).ToList();

            if (!otherKeys.Any()) return;

            DrawGroupHeader("📦 Other Data", otherKeys.Count);
            foreach (var key in otherKeys)
                DrawDataField(key);
        }

        private void DrawGroupHeader(string title, int count)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"({count})", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDataField(string key)
        {
            if (!cachedValues.ContainsKey(key)) return;

            EditorGUILayout.BeginHorizontal();

            // 키 이름 표시
            EditorGUILayout.LabelField(key, GUILayout.Width(150));

            var value = cachedValues[key];

            try
            {
                // 타입별 입력 필드
                switch (value)
                {
                    case int intValue:
                        int newIntValue = EditorGUILayout.IntField(intValue, GUILayout.Width(200));
                        if (newIntValue != intValue)
                            UpdateValue(key, newIntValue);
                        break;

                    case string stringValue when key == "MasterCards":
                        DrawMasterCardsField(stringValue);
                        break;

                    case string stringValue:
                        string newStringValue = EditorGUILayout.TextField(stringValue, GUILayout.Width(200));
                        if (newStringValue != stringValue)
                            UpdateValue(key, newStringValue);
                        break;

                    case bool boolValue:
                        bool newBoolValue = EditorGUILayout.Toggle(boolValue, GUILayout.Width(200));
                        if (newBoolValue != boolValue)
                            UpdateValue(key, newBoolValue);
                        break;

                    case float floatValue:
                        float newFloatValue = EditorGUILayout.FloatField(floatValue, GUILayout.Width(200));
                        if (Math.Abs(newFloatValue - floatValue) > 0.001f)
                            UpdateValue(key, newFloatValue);
                        break;
                }

                // 복사 버튼
                if (GUILayout.Button("📋", GUILayout.Width(25)))
                    EditorGUIUtility.systemCopyBuffer = value.ToString();
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox($"Error: {ex.Message}", MessageType.Error);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMasterCardsField(string cardsJson)
        {
            try
            {
                if (string.IsNullOrEmpty(cardsJson))
                {
                    EditorGUILayout.TextField("Empty", GUILayout.Width(200));
                    return;
                }

                var cards = JsonConvert.DeserializeObject<int[]>(cardsJson);
                string cardsText = string.Join(", ", cards);
                EditorGUILayout.TextField(cardsText, GUILayout.Width(200));
            }
            catch
            {
                EditorGUILayout.TextField(cardsJson, GUILayout.Width(200));
            }
        }

        private void DrawLocationData()
        {
            if (!cachedValues.TryGetValue("Locations", out var locationsValue) ||
                !(locationsValue is string locationsJson)) return;

            var parsedData = locationParser.ParseLocations(locationsJson);

            EditorGUILayout.BeginVertical("box");
            foreach (var (depth, locations) in parsedData.OrderBy(kvp => kvp.Key))
            {
                EditorGUILayout.LabelField($"📍 {depth}층 ({locations.Count}개 위치)", EditorStyles.boldLabel);

                foreach (var location in locations.Take(3)) // 최대 3개만 표시
                {
                    string locationInfo = $"ID:{location.ID}, Index:{location.index}, Links:{location.adjacency?.Count ?? 0}";
                    EditorGUILayout.TextField(locationInfo, GUILayout.Width(400));
                }

                if (locations.Count > 3)
                    EditorGUILayout.LabelField($"... and {locations.Count - 3} more", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedDepthData()
        {
            if (!cachedValues.TryGetValue("SelectedDepthIndies", out var depthValue) ||
                !(depthValue is string depthJson)) return;

            try
            {
                var selectedDepths = JsonConvert.DeserializeObject<Dictionary<int, int>>(depthJson);
                string depthText = string.Join(", ", selectedDepths.Select(kvp => $"{kvp.Key}층: {kvp.Value}"));
                EditorGUILayout.TextField(depthText, GUILayout.Width(400));
            }
            catch
            {
                EditorGUILayout.TextField("Invalid JSON", GUILayout.Width(400));
            }
        }

        private void UpdateValue(string key, object newValue)
        {
            cachedValues[key] = newValue;

            // PlayerPrefs에 저장
            switch (newValue)
            {
                case int intValue:
                    PlayerPrefs.SetInt(key, intValue);
                    break;
                case string stringValue:
                    PlayerPrefs.SetString(key, stringValue);
                    break;
                case bool boolValue:
                    PlayerPrefs.SetInt(key, boolValue ? 1 : 0);
                    break;
                case float floatValue:
                    PlayerPrefs.SetFloat(key, floatValue);
                    break;
            }

            PlayerPrefs.Save();
        }

        private void RefreshData()
        {
            cachedValues.Clear();

            foreach (var key in PlayerDataManager.GetAllKeys())
            {
                if (PlayerPrefs.HasKey(key))
                {
                    // 타입 추정 및 값 로드
                    cachedValues[key] = GetTypedValue(key);
                }
            }
        }

        private object GetTypedValue(string key)
        {
            // Int 타입 키인지 확인
            if (IsIntKey(key))
                return PlayerPrefs.GetInt(key, 0);

            // Bool 타입 키인지 확인
            if (IsBoolKey(key))
                return PlayerPrefs.GetInt(key, 0) == 1;

            // 기본적으로 string으로 처리
            return PlayerPrefs.GetString(key, "");
        }

        private bool IsIntKey(string key) =>
            key.Contains("Id") || key.Contains("Hp") || key.Contains("Gold") ||
            key.Contains("Floor") || key.Contains("Level") || key.Contains("Satiety");

        private bool IsBoolKey(string key) =>
            key.StartsWith("Keep") || key.Contains("Enable") || key.Contains("Toggle");

        private IEnumerable<string> GetFilteredKeys()
        {
            var allKeys = cachedValues.Keys.AsEnumerable();

            // 검색 필터 적용
            if (!string.IsNullOrEmpty(searchFilter))
                allKeys = allKeys.Where(k => k.ToLower().Contains(searchFilter.ToLower()));

            // 그룹 필터 적용
            allKeys = selectedGroup switch
            {
                PlayerDataGroup.Master => allKeys.Where(k => k.StartsWith("Master")),
                PlayerDataGroup.Progress => allKeys.Where(k => k.Contains("Floor") || k.Contains("Level") || k.Contains("Act")),
                PlayerDataGroup.Settings => allKeys.Where(k => k.Contains("Keep") || k.Contains("Last")),
                PlayerDataGroup.Map => allKeys.Where(k => k.Contains("Location") || k.Contains("Depth")),
                _ => allKeys
            };

            return allKeys.OrderBy(k => k);
        }
    }

    // ========================================
    // 📁 LocationDataParser.cs - 위치 데이터 파서
    // ========================================
    public class LocationDataParser
    {
        public Dictionary<int, List<LocationData>> ParseLocations(string json)
        {
            var result = new Dictionary<int, List<LocationData>>();

            try
            {
                var locations = JsonConvert.DeserializeObject<Dictionary<string, LocationData>>(json);

                foreach (var location in locations.Values)
                {
                    if (!result.ContainsKey(location.depth))
                        result[location.depth] = new List<LocationData>();

                    result[location.depth].Add(location);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Location JSON 파싱 실패: {ex.Message}");
            }

            return result;
        }
    }

    // ========================================
    // 📁 LocationData.cs - 위치 데이터 구조체
    // ========================================
    [Serializable]
    public class LocationData
    {
        public int seed;
        public int depth;
        public int index;
        public List<Direction> directions;
        public List<int> adjacency;
        public int ID;
    }

    [Serializable]
    public class Direction
    {
        public int Item1;
        public string Item2;
    }
}
#endif