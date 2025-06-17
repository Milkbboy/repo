using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerDataManager
{
    private const string PlayerPrefsKeysKey = "PlayerPrefsKeys";

    // ✅ 타입 안전한 Get/Set 메서드
    public static T GetValue<T>(PlayerPrefKey<T> key)
    {
        try
        {
            return typeof(T) switch
            {
                Type t when t == typeof(int) => (T)(object)PlayerPrefs.GetInt(key.Key, (int)(object)key.DefaultValue),
                Type t when t == typeof(string) => (T)(object)PlayerPrefs.GetString(key.Key, (string)(object)key.DefaultValue),
                Type t when t == typeof(bool) => (T)(object)(PlayerPrefs.GetInt(key.Key, (bool)(object)key.DefaultValue ? 1 : 0) == 1),
                Type t when t == typeof(float) => (T)(object)PlayerPrefs.GetFloat(key.Key, (float)(object)key.DefaultValue),
                _ => throw new NotSupportedException($"지원하지 않는 타입: {typeof(T)}")
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"PlayerData 읽기 실패 - Key: {key.Key}, Error: {ex.Message}");
            return key.DefaultValue;
        }
    }

    public static void SetValue<T>(PlayerPrefKey<T> key, T value)
    {
        try
        {
            switch (value)
            {
                case int intValue:
                    PlayerPrefs.SetInt(key.Key, intValue);
                    break;
                case string stringValue:
                    PlayerPrefs.SetString(key.Key, stringValue);
                    break;
                case bool boolValue:
                    PlayerPrefs.SetInt(key.Key, boolValue ? 1 : 0);
                    break;
                case float floatValue:
                    PlayerPrefs.SetFloat(key.Key, floatValue);
                    break;
                default:
                    throw new NotSupportedException($"지원하지 않는 타입: {typeof(T)}");
            }

            AddKeyToRegistry(key.Key);
            PlayerPrefs.Save();

            Debug.Log($"PlayerData 저장 완료 - {key.Key}: {value}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"PlayerData 저장 실패 - Key: {key.Key}, Value: {value}, Error: {ex.Message}");
        }
    }

    // ✅ 배치 처리 지원
    public static void SetValues(params (string key, object value)[] keyValuePairs)
    {
        try
        {
            foreach (var (key, value) in keyValuePairs)
            {
                switch (value)
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
                AddKeyToRegistry(key);
            }

            PlayerPrefs.Save();
            Debug.Log($"배치 저장 완료 - {keyValuePairs.Length}개 항목");
        }
        catch (Exception ex)
        {
            Debug.LogError($"배치 저장 실패: {ex.Message}");
        }
    }

    // ✅ 안전한 삭제 기능
    public static void DeleteAllExcept()
    {
        try
        {
            var keysToDelete = new List<string>();

            foreach (var key in GetAllKeys())
            {
                if (!PlayerDataKeys.ExceptKeys.Contains(key))
                    keysToDelete.Add(key);
            }

            foreach (var key in keysToDelete)
            {
                PlayerPrefs.DeleteKey(key);
            }

            PlayerPrefs.DeleteKey(PlayerPrefsKeysKey);
            PlayerPrefs.Save();

            Debug.Log($"플레이어 데이터 초기화 완료 - {keysToDelete.Count}개 키 삭제");
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 초기화 실패: {ex.Message}");
        }
    }

    // ✅ 키 존재 확인
    public static bool HasKey<T>(PlayerPrefKey<T> key) => PlayerPrefs.HasKey(key.Key);

    // ✅ 모든 키 조회
    public static IEnumerable<string> GetAllKeys()
    {
        string keysString = PlayerPrefs.GetString(PlayerPrefsKeysKey, "");
        return keysString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    }

    // ✅ 키 레지스트리 관리
    private static void AddKeyToRegistry(string key)
    {
        string keys = PlayerPrefs.GetString(PlayerPrefsKeysKey, "");

        if (!keys.Contains(key))
        {
            keys += key + ";";
            PlayerPrefs.SetString(PlayerPrefsKeysKey, keys);
        }
    }

    // ✅ 데이터 검증
    public static bool ValidateData()
    {
        try
        {
            // 필수 데이터 검증
            if (GetValue(PlayerDataKeys.MasterId) < 0) return false;
            if (GetValue(PlayerDataKeys.MasterHp) < 0) return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}