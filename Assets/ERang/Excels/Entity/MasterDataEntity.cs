namespace ERang
{
    [System.Serializable]
    public class MasterDataEntity
    {
        public int Master_Id; // 마왕의 Id 값
        public string MasterNameDesc_Id; // 마왕 캐릭터의 실제 이름 String이 들어간 데이터 Id
        public string MasterDesc_Id; // 마왕의 캐릭터 설명을 담은 String이 들어간 데이터 Id
        public int MasterAi_id; // 마왕의 공격 타입에 대한 Id (근거리, 원거리, 혹은 폭파 등)
        public int Hp; // 마왕 캐릭터의 초기 체력 값
        public int Atk; // 마왕 캐릭터의 초기 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int Def; // 마왕 캐릭터의 초기 방어력 값
        public int StartMana; // 마왕 캐릭터의 초기 마나 값
        public int MaxMana; // 마왕 캐릭터의 최대 마나 값
        public int RechargeMana; // 턴이 다시 시작 될 때 얻게 되는 마나 초기 값
        public string StartCardDeck_Id; // 마왕이 처음 스테이지에 진입 할 때 갖게되는 카드의 복수 값
        public int StartArtiFact_Id; // 마왕이 처음 시작 시 갖고 있는 아티팩트의 id 값
        public string StartAbility_Id; // 마왕이 선천적으로 가지고 있는 특성 id 값
    }
}
