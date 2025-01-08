using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityWeaken : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Weaken;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            yield return StartCoroutine(ApplySingle(aiData, abilityData, selfSlot, null));
        }

        /// <summary>
        /// 적용 - Atk 감소
        /// </summary>
        public IEnumerator ApplySingle(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText}");
                yield break;
            }

            int before = card.Atk;
            int change = abilityData.value;

            targetSlot.DecreaseAttack(change);

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Atk, change));

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 해제 - Atk 증가
        /// </summary>
        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText}");
                yield break;
            }
            int before = card.Atk;
            int change = ability.abilityValue;

            targetSlot.IncreaseAttack(change);

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Atk, change));

            yield return new WaitForSeconds(0.1f);
        }
    }
}