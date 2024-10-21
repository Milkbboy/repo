using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.Text; // Newtonsoft.Json 라이브러리 추가

namespace ERang
{
    public class PlayerPrefsViewer : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, object> playerPrefsValues = new();
        private Dictionary<int, List<LocationData>> locationsByDepth = new();
        Dictionary<int, int> selectedDepthIndies = new();

        [MenuItem("ERang/PlayerPrefs Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerPrefsViewer>("PlayerPrefs Viewer");
            window.LoadPlayerPrefs();
        }

        private void LoadPlayerPrefs()
        {
            playerPrefsValues.Clear(); // 기존 값을 지웁니다.
            locationsByDepth.Clear(); // 층별 데이터 초기화

            foreach (var key in PlayerPrefsUtility.PrefIntKeys)
            {
                if (PlayerPrefsUtility.HasKey(key))
                {
                    int intValue = PlayerPrefsUtility.GetInt(key);
                    playerPrefsValues[key] = intValue;
                }
            }

            foreach (var key in PlayerPrefsUtility.PrefStringKeys)
            {
                if (PlayerPrefsUtility.HasKey(key))
                {
                    string stringValue = PlayerPrefsUtility.GetString(key);
                    playerPrefsValues[key] = stringValue;

                    if (key == "Locations")
                    {
                        ParseLocations(stringValue);
                    }

                    if (key == "SelectedDepthIndies")
                    {
                        selectedDepthIndies = JsonConvert.DeserializeObject<Dictionary<int, int>>(stringValue);
                    }
                }
            }
        }

        private void OnEnable()
        {
            // Load PlayerPrefs values into the dictionary
            string keysString = PlayerPrefsUtility.GetString("PlayerPrefsKeys", "");
            Debug.Log($"keysString: {keysString}");

            foreach (var key in keysString.Split(';'))
            {
                if (string.IsNullOrEmpty(key)) continue;
                playerPrefsValues[key] = GetPlayerPrefValue(key);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("PlayerPrefs Viewer", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh"))
            {
                // LoadPlayerPrefs();
                PlayerPrefs.DeleteAll();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var kvp in playerPrefsValues)
            {
                if (kvp.Key == "DepthWidths")
                    continue;

                EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(200));

                if (kvp.Value is int intValue)
                {
                    int newValue = EditorGUILayout.IntField(intValue, GUILayout.Width(200));
                    if (newValue != intValue)
                    {
                        playerPrefsValues[kvp.Key] = newValue;
                        PlayerPrefsUtility.SetInt(kvp.Key, newValue);
                    }
                }
                else if (kvp.Value is string stringValue)
                {
                    if (kvp.Key == "Locations")
                    {
                        // Locations 데이터를 층별로 표시
                        foreach (var depth in locationsByDepth.Keys.OrderBy(d => d))
                        {
                            EditorGUILayout.BeginVertical("box");

                            GUILayout.Label($"{depth} 층", EditorStyles.boldLabel, GUILayout.Width(200));

                            foreach (var location in locationsByDepth[depth])
                            {
                                GUIStyle style = new GUIStyle(GUI.skin.textField);

                                if (selectedDepthIndies.TryGetValue(depth, out int selectedIndex) && location.index == selectedIndex)
                                    style.normal.textColor = new Color(0.5f, 1.0f, 0.5f); // 선택된 인덱스의 텍스트 색상을 연한 녹색으로 변경

                                string locationJson = JsonConvert.SerializeObject(location);
                                string linkText = string.Join(", ", location.adjacency.Select(ad => $"{location.ID} -> {ad}"));

                                EditorGUILayout.TextField(linkText, style, GUILayout.Width(400));
                            }

                            EditorGUILayout.EndVertical();
                        }
                    }
                    else if (kvp.Key == "SelectedDepthIndies")
                    {
                        List<(int, int)> selectedDepthIndiesList = selectedDepthIndies.Select(kvp => (kvp.Key, kvp.Value)).ToList();

                        string selectedFloorIndexText = string.Join(", ", selectedDepthIndiesList.Select(kvp => $"{kvp.Item1} 층 {kvp.Item2}"));
                        EditorGUILayout.TextField(selectedFloorIndexText, GUILayout.Width(400));
                    }
                    else
                    {
                        string newValue = EditorGUILayout.TextField(stringValue, GUILayout.Width(200));
                        if (newValue != stringValue)
                        {
                            playerPrefsValues[kvp.Key] = newValue;
                            PlayerPrefsUtility.SetString(kvp.Key, newValue);
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void ParseLocations(string json)
        {
            try
            {
                var locations = JsonConvert.DeserializeObject<Dictionary<string, LocationData>>(json);

                foreach (var location in locations.Values)
                {
                    if (!locationsByDepth.ContainsKey(location.depth))
                    {
                        locationsByDepth[location.depth] = new List<LocationData>();
                    }

                    locationsByDepth[location.depth].Add(location);
                }
            }
            catch
            {
                Debug.LogError("Failed to parse Locations JSON.");
            }
        }

        private string GetPlayerPrefValue(string key)
        {
            if (PlayerPrefsUtility.HasKey(key))
            {
                // 기본값을 빈 문자열로 설정
                string storedValue = PlayerPrefsUtility.GetString(key, "");

                // 값이 없을 때만 기본값을 설정
                if (string.IsNullOrEmpty(storedValue))
                    return "0";

                if (int.TryParse(storedValue, out int intValue))
                    return intValue.ToString();

                if (float.TryParse(storedValue, out float floatValue))
                    return floatValue.ToString();

                return storedValue;
            }

            return "0"; // 키가 존재하지 않을 때 기본값 반환
        }

        private void SetPlayerPrefValue(string key, string value)
        {
            Debug.Log($"SetPlayerPrefValue - key: {key}, value: {value}");

            if (int.TryParse(value, out int intValue))
            {
                PlayerPrefsUtility.SetInt(key, intValue);
            }
            else
            {
                PlayerPrefsUtility.SetString(key, value);
            }
        }

        private class LocationData
        {
            public int seed;
            public int depth;
            public int index;
            public List<Direction> directions;
            public List<int> adjacency;
            public int ID;
        }

        private class Direction
        {
            public int Item1;
            public string Item2;
        }
    }
}