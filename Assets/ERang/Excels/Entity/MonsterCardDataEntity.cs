namespace ERang
{
    [System.Serializable]
    public class MonsterCardDataEntity
    {
        public int Card_Id; // 카드의 Id 값
        public string NameDesc; // 카드의 이름
        public string CardNameDesc_Id; // 카드의 실제 이름 String이 들어간 데이터 Id
        public string CardDesc_Id; // 카드에 대한 설명을 담은 String이 들어간 데이터 Id
        public string CardShortDesc_Id; // 카드 간단 설명
        public string CardType; // 해당 카드의 타입을 입력 (타입은 초안에서 총 6개의 타입으로 구분)
        public string Grade; // 해당 카드의 등급을 입력 (등급은 초안에서 총 5개의 등급으로 구분)
        public int AiGroup_id; // 해당 카드가 가지고 있는 Ai 그룹의 Id 값
        public int CostMana; // 소환에 필요한 마나
        public int CostGold; // 소환에 필요한 골드
        public int Hp; // 체력 값
        public int Atk; // 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int Def; // 초기 방어력 값
        public string Ability_Id; // 해당 카드가 가진 어빌리티 값으로 복수 지정이 가능하다.
        public bool Extinction; // Bool 값으로 True 시 해당 카드는 사용 시 해당 전투에서 카드 덱에서 삭제된다.
        public bool CompleteExtinction; // Extinction 값이 TRUE일 경우에만 동작한다. 해당 값이 TRUE일 경우 해당 카드는 사용 시 다음 전투에서도 삭제된다. (1회성 카드)
        public int Level; // 기본 레벨 값
        public string Level_1_img; // 크리쳐의 경우에만 입력. 해당 크리쳐의 1~2레벨 시 이미지 링크
        public string Level_3_img; // 크리쳐의 경우에만 입력. 해당 크리쳐의 3~4레벨 시 이미지 링크
        public string Level_5_img; // 크리쳐의 경우에만 입력. 해당 크리쳐의 5레벨 시 이미지 링크
        public string HandStart_Ability; // 핸드에 해당 카드가 들어 온 순간 발동되는 어빌티리 (Ex. 전쟁의 가호 : 모든 크리쳐 공격력 1턴 동안 1 증가)
        public string HandEnd_Ability; // 해당 카드가 핸드에서 무덤으로 들어가는 순간 발동되는 어빌티리 (Ex. 부패 : 핸드에 남은 카드 개수 당 2의 데미지를 마왕이 받는다)
        public int Owner; // 개성 카드에만 입력. 해당 카드를 소유한 마스터의 Id를 입력
    }
}