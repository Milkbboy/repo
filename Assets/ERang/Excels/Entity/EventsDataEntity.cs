namespace ERang
{
    [System.Serializable]
    public class EventsDataEntity
    {
        public int EventsID;
        public string NameDesc;
        public string EventType;
        public string ExcludeFloor;
        public bool ExcludeEndFloor;
        public string IncludeFloor;
        public string Prefab;
        public int MinValue;
        public int MaxValue;
        public int EliteBattleLevelGroupID;
        public string RandomEventID;
    }
}