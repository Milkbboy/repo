using System.Collections;

namespace ERang
{
    public interface IActionProcessor
    {
        /// <summary>
        /// 핸드 카드 사용 처리
        /// </summary>
        bool UseHandCard(HCard hCard, BSlot targetSlot);

        /// <summary>
        /// 마법 카드 사용 처리
        /// </summary>
        IEnumerator UseHandCard(string cardUid, BSlot targetSlot);

        /// <summary>
        /// 보드 슬롯에 카드 장착
        /// </summary>
        void EquipCardToSlot(HCard hCard, BSlot boardSlot);

        /// <summary>
        /// 카드 사용 가능 여부 확인
        /// </summary>
        bool CanUseHandCard(string cardUid);

        /// <summary>
        /// 보드 카드 제거
        /// </summary>
        IEnumerator RemoveBoardCard(int slotNum);
    }
}