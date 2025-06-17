using System.Collections.Generic;

public static class PlayerDataKeys
{
    // 🔑 강타입 키 정의
    public static readonly PlayerPrefKey<int> MasterId = new("MasterId", 1001);
    public static readonly PlayerPrefKey<int> MasterHp = new("MasterHp", -1);
    public static readonly PlayerPrefKey<int> MasterGold = new("MasterGold", 0);
    public static readonly PlayerPrefKey<int> ActId = new("ActId", 0);
    public static readonly PlayerPrefKey<int> AreaId = new("AreaId", 0);
    public static readonly PlayerPrefKey<int> Floor = new("Floor", 1);
    public static readonly PlayerPrefKey<int> MaxFloor = new("MaxFloor", 1);
    public static readonly PlayerPrefKey<int> LevelId = new("LevelId", 100100101);
    public static readonly PlayerPrefKey<int> DepthIndies = new("DepthIndies", 0);
    public static readonly PlayerPrefKey<int> Satiety = new("Satiety", 100);
    public static readonly PlayerPrefKey<int> LastLocationId = new("LastLocationId", 0);
    public static readonly PlayerPrefKey<int> Gold = new("Gold", -1);

    public static readonly PlayerPrefKey<string> MasterCards = new("MasterCards", null);
    public static readonly PlayerPrefKey<string> SelectedDepthIndies = new("SelectedDepthIndies", "{\"1\":0}");
    public static readonly PlayerPrefKey<string> DepthWidths = new("DepthWidths", "");
    public static readonly PlayerPrefKey<string> Locations = new("Locations", "{}");
    public static readonly PlayerPrefKey<string> LastScene = new("LastScene", "");
    public static readonly PlayerPrefKey<string> SelectLocation = new("SelectLocation", "");
    public static readonly PlayerPrefKey<string> RandomEvents = new("RandomEvents", "");

    public static readonly PlayerPrefKey<bool> KeepSatiety = new("KeepSatiety", false);

    // 🚫 삭제 제외 키 목록
    public static readonly HashSet<string> ExceptKeys = new() { "KeepSatiety", "Satiety" };
}