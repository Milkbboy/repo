using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        public void SetAbility(AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            if (selfSlot.Card == null)
            {
                Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} 장착된 카드 없음 - AbilityLogic.AbilityAction");
                return;
            }

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 어빌리티({Utils.NumbersText(aiData.ability_Ids)}) 적용 - AbilityLogic.AbilityAction");

            foreach (int abilityId in aiData.ability_Ids)
            {
                // 이미 어빌리티가 적용 중이면 패스
                Card.DurationAbility durationAbility = selfSlot.Card.HasAbilityDuration(abilityId);

                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 이미 어빌리티({abilityId})가 적용 중으로 해당 어빌리티 패스 - AbilityLogic.AbilityAction");
                    continue;
                }

                AbilityData ability = AbilityData.GetAbilityData(abilityId);

                if (ability == null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} <color=red>어빌리티 데이터 없음</color> - AbilityLogic.AbilityAction");
                    continue;
                }

                switch (ability.abilityType)
                {
                    case AbilityType.Damage:
                        BoardLogic.Instance.AbilityDamage(aiData, selfSlot, targetSlots);
                        break;
                    case AbilityType.Heal:
                    case AbilityType.AtkUp:
                    case AbilityType.DefUp:
                    case AbilityType.BrokenDef:
                    case AbilityType.ChargeDamage:
                        BoardLogic.Instance.AbilityAffect(aiData, ability, selfSlot, targetSlots);
                        break;
                    case AbilityType.AddGoldPer:
                        BoardLogic.Instance.AbilityAddGoldPer(aiData, ability, selfSlot);
                        break;
                }
            }
        }
    }
}