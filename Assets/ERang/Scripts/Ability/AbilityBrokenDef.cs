using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityBrokenDef : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.BrokenDef;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            foreach (BSlot targetSlot in targetSlots)
            {
                BaseCard card = targetSlot.Card;

                if (card == null)
                {
                    Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                    continue;
                }

                int before = card.Def;
                int change = abilityData.value;

                // 카드 어빌리티 중 ArmorBreak 가 있으면 방어력 감소 적용 안함
                if (card.ArmorBreakAbility != null)
                {
                    Debug.Log($"{targetSlot.LogText} 카드 어빌리티 중 ArmorBreak 있어 def 감소 적용 안함.");
                    yield break;
                }

                targetSlot.DecreaseDefense(change);

                Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, change));
            }

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator ApplySingle(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int before = card.Def;
            int change = abilityData.value;

            targetSlot.DecreaseDefense(change);

            // 카드 어빌리티 중 ArmorBreak 가 있으면 방어력 감소 적용 안함
            if (card.ArmorBreakAbility != null)
            {
                Debug.Log($"{targetSlot.LogText} 카드 어빌리티 중 ArmorBreak 있어 def 감소 적용 안함.");
                yield break;
            }

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, change));

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            AbilityData abilityData = AbilityData.GetAbilityData(ability.abilityId);

            if (abilityData == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} abilityId: {ability.abilityId} 어빌리티 데이터 없음.");
                yield break;
            }

            int before = card.Def;
            int amount = abilityData.value;

            // 카드 어빌리티 중 ArmorBreak 가 있으면 방어력 감소 적용 안함
            if (card.ArmorBreakAbility != null)
            {
                Debug.Log($"{targetSlot.LogText} 카드 어빌리티 중 ArmorBreak 있어 def 증가 적용 안함.");
                yield break;
            }

            card.IncreaseDefense(amount);

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, amount));

            yield return new WaitForSeconds(0.1f);
        }
    }
}