using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsUtility
{
    public static readonly List<string> PrefIntKeys = new() { "MasterId", "ActId", "AreaId", "Floor", "MaxFloor", "LevelId", "DepthIndies", "Satiety" };
    public static readonly List<string> PrefStringKeys = new() { "SelectedDepthIndies", "DepthWidths", "Locations" };
    public static readonly List<string> PrefBoolKeys = new() { "KeepSatiety" };
    public static readonly List<string> PrefExceptKeys = new() { "KeepSatiety", "Satiety" };

    private const string PlayerPrefsKeysKey = "PlayerPrefsKeys";

    public static void DeleteAllExcept()
    {
        foreach (var key in GetAllKeys())
        {
            // 삭제 제외되는 키 값
            if (PrefExceptKeys.Contains(key))
                continue;

            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.DeleteKey(PlayerPrefsKeysKey);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        AddKey(key);

        PlayerPrefs.Save();
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        AddKey(key);

        PlayerPrefs.Save();
    }

    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        AddKey(key);

        PlayerPrefs.Save();
    }

    public static T GetValue<T>(string key, T defaultValue = default)
    {
        if (typeof(T) == typeof(int))
        {
            if (PrefIntKeys.Contains(key))
                return (T)(object)GetInt(key, (int)(object)defaultValue);
        }

        if (typeof(T) == typeof(string))
        {
            if (PrefStringKeys.Contains(key))
                return (T)(object)GetString(key, (string)(object)defaultValue);
        }

        if (typeof(T) == typeof(bool))
        {
            if (PrefBoolKeys.Contains(key))
                return (T)(object)GetBool(key, (bool)(object)defaultValue);
        }

        throw new System.InvalidCastException($"지원하지 않는 형식입니다. key: {key}, type: {typeof(T)}");
    }

    public static object GetValue(string key)
    {
        if (PrefIntKeys.Contains(key))
            return GetInt(key);

        if (PrefStringKeys.Contains(key))
            return GetString(key);

        return null;
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        int value = PlayerPrefs.GetInt(key, defaultValue);
        return value;
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string value = PlayerPrefs.GetString(key, defaultValue);
        return value;
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        int value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
        return value == 1;
    }

    public static bool HasKey(string key)
    {
        bool hasKey = PlayerPrefs.HasKey(key);
        return hasKey;
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static IEnumerable<string> GetAllKeys()
    {
        string keysString = PlayerPrefs.GetString(PlayerPrefsKeysKey, "");

        return keysString.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    public static void AddKey(string key)
    {
        string keys = PlayerPrefs.GetString(PlayerPrefsKeysKey, "");

        if (!keys.Contains(key))
        {
            keys += key + ";";
            PlayerPrefs.SetString(PlayerPrefsKeysKey, keys);
        }
    }

    private static void RemoveKey(string key)
    {
        string keys = PlayerPrefs.GetString(PlayerPrefsKeysKey, "");

        if (keys.Contains(key))
        {
            keys = keys.Replace(key + ";", "");
            PlayerPrefs.SetString(PlayerPrefsKeysKey, keys);
        }
    }
}