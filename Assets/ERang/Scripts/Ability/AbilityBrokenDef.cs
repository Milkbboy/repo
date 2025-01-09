using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityBrokenDef : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.BrokenDef;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isDefUp)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int before = card.Def;
            int value = cardAbility.abilityValue;

            if (isDefUp)
                card.IncreaseDefense(value);
            else
                card.DecreaseDefense(value);

            // 카드 어빌리티 중 ArmorBreak 가 있으면 방어력 감소 적용 안함
            if (card.ArmorBreakAbility != null)
            {
                Debug.Log($"{targetSlot.LogText} 카드 어빌리티 중 ArmorBreak 있어 def {(isDefUp ? "증가" : "감소")} 적용 안함.");
                yield break;
            }

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, isDefUp ? value : value * -1));

            yield return new WaitForSeconds(0.1f);
        }
    }
}