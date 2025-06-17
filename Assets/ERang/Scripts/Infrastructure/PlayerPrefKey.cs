[System.Serializable]
public readonly struct PlayerPrefKey<T>
{
    public readonly string Key;
    public readonly T DefaultValue;

    public PlayerPrefKey(string key, T defaultValue)
    {
        Key = key ?? throw new System.ArgumentNullException(nameof(key));
        DefaultValue = defaultValue;
    }

    public static implicit operator string(PlayerPrefKey<T> prefKey) => prefKey.Key;

    public override string ToString() => Key;
}