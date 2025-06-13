using System;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 어빌리티 아이템
    /// </summary>
    public class AbilityItem
    {
        // 어빌리티 적용 위치
        public AbilityWhereFrom whereFrom;
        // 어빌리티 적용 턴
        public int applyTurn;
        // 어빌리티 값
        public int value;
        // 어빌리티 지속 턴
        public int duration;
        // 어빌리티 생성일
        public long createdDt;
    }

    [Serializable]
    public class CardAbility
    {
        public CardAbility()
        {
        }

        public CardAbility(int abilityId)
        {
            this.abilityId = abilityId;
        }

        public string abilityUid; // 어빌리티 고유 번호. abilityId + startTurn + abilityCount
        public int abilityId; // 어빌리티 Id
        public int aiDataId; // AiData Id
        public int selfSlotNum;
        public int targetSlotNum;
        public string abilityName; // 어빌리티 이름
        public int cardId; // 카드 Id

        public List<BSlot> targetSlots = new();

        // 어빌리티 아이템
        // 실제 어빌리티 총 value, 총 duration 을 계산할때 사용
        // 2025-01-09 value 는 중첩되지 않지만 duration 은 충첩되고 있음. 추후 AbilityType 에 따라 value 도 중첩될 수 있음.
        private Queue<AbilityItem> abilityItems = new();

        // -----------------------------------------------------------------------------------
        public AbilityType abilityType; // 어빌리티 타입
        public AbilityWorkType workType; // 어빌리티 작업 타입(HandOn 찾기 위함)
        public AiDataType aiType; // Ai 타입. Buff, Debuff 구분
        public int duration;
        public int abilityValue;
        // -----------------------------------------------------------------------------------

        public void AddAbilityItem(AbilityItem abilityItem)
        {
            abilityItems.Enqueue(abilityItem);

            CalcDuration();
        }

        /// <summary>
        /// 동일한 어빌리티의 duration 처리
        ///  - 추가된 순서대로 AbilityItem 의 duration 부터 감소
        /// </summary>
        public void DecreaseDuration()
        {
            if (abilityItems.Count == 0)
            {
                return;
            }

            AbilityItem abilityItem = abilityItems.Peek();

            abilityItem.duration--;

            if (abilityItem.duration <= 0)
                abilityItems.Dequeue();

            CalcDuration();
        }

        /// <summary>
        /// 어빌리티 duration 계산
        /// </summary>
        public void CalcDuration()
        {
            duration = abilityItems.Sum(abilityItem => abilityItem.duration);
        }
    }
}