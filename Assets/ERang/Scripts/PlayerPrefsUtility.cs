using UnityEngine;
using System.Collections.Generic;

public static class PlayerPrefsUtility
{
    public static readonly List<string> PrefIntKeys = new() { "MasterId", "ActId", "AreaId", "Floor", "MaxFloor", "LevelId", "DepthIndies" };
    public static readonly List<string> PrefStringKeys = new() { "SelectedDepthIndies", "DepthWidths", "Locations" };

    private const string PlayerPrefsKeysKey = "PlayerPrefsKeys";

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        AddKey(key);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        AddKey(key);
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