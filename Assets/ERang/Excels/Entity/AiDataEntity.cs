namespace ERang
{
    [System.Serializable]
    public class AiDataEntity
    {
        public int Ai_Id; // Ai의 Id 값
        public string NameDesc; // Ai의 이름 및 설명
        public string Type; // 행동의 타입을 정의
        public string Target; // 대상 혹은 복수 대상을 설정한다.
        public string Atk_Type; // 행동이 이루어지는 절차를 설정한다.
        public string Atk_Range; // 공격 범위를 설정한다.
        public int Atk_Cnt; // 공격 횟수
        public float Atk_Interval; // 공격 횟수가 1이 아닐 경우 공격이 진행되는 텀을 지정
        public int Value; // 해당 행동의 무게 값으로 Ai Group에서 참조된다.
        public int Explosion_Shock; // Type이 Explosion일 경우에만 입력
        public string Ability_id; // 실질적인 효과를 주는 Ability의 Id를 입력
    }
}
