using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ERang
{
    public class PlayerPrefsViewer : EditorWindow
    {
        private Vector2 scrollPosition;
        private string newKey = "";
        private string newValue = "";
        private Dictionary<string, string> playerPrefsValues = new Dictionary<string, string>();

        [MenuItem("ERang/PlayerPrefs Viewer")]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefsViewer>("PlayerPrefs Viewer");

            // Set initial values for testing
            // PlayerPrefsUtility.SetInt("ActId", 0);
            // PlayerPrefsUtility.SetInt("AreaId", 0);
            // PlayerPrefsUtility.SetInt("Floor", 0);
            // PlayerPrefsUtility.SetInt("LevelId", 0);

            int actId = PlayerPrefsUtility.GetInt("ActId", 0);
            int areaId = PlayerPrefsUtility.GetInt("AreaId", 0);
            int floor = PlayerPrefsUtility.GetInt("Floor", 0);
            int levelId = PlayerPrefsUtility.GetInt("LevelId", 0);
            string masterId = PlayerPrefsUtility.GetString("MasterId", "");

            Debug.Log($"ActId: {actId}, AreaId: {areaId}, Floor: {floor}, LevelId: {levelId}, MasterId: {masterId}");
        }

        private void OnEnable()
        {
            // Load PlayerPrefs values into the dictionary
            string keysString = PlayerPrefsUtility.GetString("PlayerPrefsKeys", "");
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
                // PlayerPrefsUtility.SetInt("ActId", 0);
                // PlayerPrefsUtility.SetInt("AreaId", 0);
                // PlayerPrefsUtility.SetInt("Floor", 0);
                // PlayerPrefsUtility.SetInt("LevelId", 0);

                PlayerPrefs.DeleteAll();

                int actId = PlayerPrefsUtility.GetInt("ActId", 0);
                int areaId = PlayerPrefsUtility.GetInt("AreaId", 0);
                int floor = PlayerPrefsUtility.GetInt("Floor", 0);
                int levelId = PlayerPrefsUtility.GetInt("LevelId", 0);
                string masterId = PlayerPrefsUtility.GetString("MasterId", "");
                string depthWidthJson = PlayerPrefs.GetString("depthWidth", null);
                string locationsJson = PlayerPrefs.GetString("locations", null);
                string depthIndiesJson = PlayerPrefsUtility.GetString("depthIndies", null);
                int currentLocationId = PlayerPrefsUtility.GetInt("CurrentLocationId", 0);

                Debug.Log($"ActId: {actId}, AreaId: {areaId}, Floor: {floor}, LevelId: {levelId}, MasterId: {masterId}, depthWidthJson: {depthWidthJson}, locationsJson: {locationsJson}, depthIndiesJson: {depthIndiesJson}, currentLocationId: {currentLocationId}");

                // Repaint();
            }

            return;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var key in playerPrefsValues.Keys.ToList())
            {
                string value = playerPrefsValues[key];
                Debug.Log($"key: {key}, value: {value}");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(key, GUILayout.Width(200));
                string newValue = EditorGUILayout.TextField(value, GUILayout.Width(200));

                if (newValue != value)
                {
                    playerPrefsValues[key] = newValue;
                    SetPlayerPrefValue(key, newValue);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    PlayerPrefsUtility.DeleteKey(key);
                    playerPrefsValues.Remove(key);
                    Repaint();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(20);
            GUILayout.Label("Add New PlayerPref", EditorStyles.boldLabel);
            newKey = EditorGUILayout.TextField("Key", newKey);
            newValue = EditorGUILayout.TextField("Value", newValue);

            if (GUILayout.Button("Add"))
            {
                if (!string.IsNullOrEmpty(newKey))
                {
                    SetPlayerPrefValue(newKey, newValue);
                    playerPrefsValues[newKey] = newValue;
                    string keys = PlayerPrefsUtility.GetString("PlayerPrefsKeys", "");
                    if (!keys.Contains(newKey))
                    {
                        keys += newKey + ";";
                        PlayerPrefsUtility.SetString("PlayerPrefsKeys", keys);
                    }
                    newKey = "";
                    newValue = "";
                    Repaint();
                }
            }
        }

        private string GetPlayerPrefValue(string key)
        {
            if (PlayerPrefsUtility.HasKey(key))
            {
                // 기본값을 빈 문자열로 설정
                string storedValue = PlayerPrefsUtility.GetString(key, "");
                Debug.Log($"GetPlayerPrefValue - key: {key}, storedValue: {storedValue}");

                if (string.IsNullOrEmpty(storedValue))
                {
                    // 값이 없을 때만 기본값을 설정
                    return "0";
                }

                if (int.TryParse(storedValue, out int intValue))
                {
                    return intValue.ToString();
                }
                else if (float.TryParse(storedValue, out float floatValue))
                {
                    return floatValue.ToString();
                }
                else
                {
                    return storedValue;
                }
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
            else if (float.TryParse(value, out float floatValue))
            {
                PlayerPrefsUtility.SetFloat(key, floatValue);
            }
            else
            {
                PlayerPrefsUtility.SetString(key, value);
            }
        }
    }
}