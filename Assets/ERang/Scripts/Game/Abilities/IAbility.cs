using System.Collections;

namespace ERang
{
    /// <summary>
    /// 기본 어빌리티 인터페이스 (보드 슬롯용)
    /// </summary>
    public interface IAbility
    {
        public AbilityType AbilityType => AbilityType.None;

        IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot);
        IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot);
    }

    /// <summary>
    /// 핸드 카드 전용 어빌리티 인터페이스
    /// </summary>
    public interface IHandAbility : IAbility
    {
        IEnumerator ApplySingle(BaseCard card);
        IEnumerator Release(BaseCard card);
    }
}