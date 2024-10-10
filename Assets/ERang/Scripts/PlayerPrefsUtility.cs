using UnityEngine;

public static class PlayerPrefsUtility
{
    public static void SetInt(string key, int value)
    {
        // Debug.Log($"PlayerPrefsUtility.SetInt - key: {key}, value: {value}");
        PlayerPrefs.SetInt(key, value);
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        int value = PlayerPrefs.GetInt(key, defaultValue);
        // Debug.Log($"PlayerPrefsUtility.GetInt - key: {key}, value: {value}");
        return value;
    }

    public static void SetFloat(string key, float value)
    {
        // Debug.Log($"PlayerPrefsUtility.SetFloat - key: {key}, value: {value}");
        PlayerPrefs.SetFloat(key, value);
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        float value = PlayerPrefs.GetFloat(key, defaultValue);
        // Debug.Log($"PlayerPrefsUtility.GetFloat - key: {key}, value: {value}");
        return value;
    }

    public static void SetString(string key, string value)
    {
        // Debug.Log($"PlayerPrefsUtility.SetString - key: {key}, value: {value}");
        PlayerPrefs.SetString(key, value);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string value = PlayerPrefs.GetString(key, defaultValue);
        // Debug.Log($"PlayerPrefsUtility.GetString - key: {key}, value: {value}");
        return value;
    }

    public static bool HasKey(string key)
    {
        bool hasKey = PlayerPrefs.HasKey(key);
        // Debug.Log($"PlayerPrefsUtility.HasKey - key: {key}, hasKey: {hasKey}");
        return hasKey;
    }

    public static void DeleteKey(string key)
    {
        // Debug.Log($"PlayerPrefsUtility.DeleteKey - key: {key}");
        PlayerPrefs.DeleteKey(key);
    }

    public static void Save()
    {
        // Debug.Log("PlayerPrefsUtility.Save");
        PlayerPrefs.Save();
    }
}