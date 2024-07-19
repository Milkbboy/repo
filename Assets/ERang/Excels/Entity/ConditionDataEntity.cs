namespace ERang
{
    [System.Serializable]
    public class ConditionDataEntity
    {
        public int ConditionData_Id; // Condition Id를 지정한다.
        public string SetTarget; // 체크할 대상을 선정함
        public string ConditionType; // 조건 타입 입력
        public string CheckPoint; // 조건이 발동되는 시점
        public string Method; // 해당 조건의 지속 방식
        public int Value;
        public string Compare;
    }
}
