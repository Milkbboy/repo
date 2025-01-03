using System;
using System.Collections.Generic;
using System.Linq;

namespace ERang
{
    public class Ability
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
        public AbilityWorkType workType; // 어빌리티 작업 타입(HandOn 찾기 위함)

        public int aiDataId;
        public AiDataType aiType; // Ai 타입. Buff, Debuff 구분

        public int duration;

        public int selfSlotNum;
        public int targetSlotNum;

        public List<BSlot> targetSlots = new();

        public string LogText => Utils.AbilityLog(abilityId, abilityUid);

        public int abilityValue;

        // 어빌리티 요소
        private Queue<Ability> abilities = new();

        public void AddAbility(Ability ability)
        {
            abilities.Enqueue(ability);

            CalcDuration();
        }

        public void DecreaseDuration()
        {
            if (abilities.Count == 0)
            {
                return;
            }

            Ability ability = abilities.Peek();

            ability.duration--;

            if (ability.duration <= 0)
                abilities.Dequeue();

            CalcDuration();
        }

        public void CalcDuration()
        {
            duration = abilities.Sum(ability => ability.duration);
        }
    }
}