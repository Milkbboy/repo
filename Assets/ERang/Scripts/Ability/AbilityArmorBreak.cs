using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityArmorBreak : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.ArmorBreak;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        /// <summary>
        /// def 0 설정
        /// </summary>
        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(targetSlot, false));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(targetSlot, true));
        }

        /// <summary>
        /// def 원복
        /// </summary>
        private IEnumerator Apply(BSlot targetSlot, bool isRelease)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int before = card.Def;
            int value = 0;

            if (isRelease)
            {
                // 감소할 def
                int sumBrokeDef = card.AbilitySystem.BrokenDefAbilities.Sum(ability => ability.abilityValue);
                // 더할 def
                int sumDefUp = card.AbilitySystem.DefUpAbilities.Sum(ability => ability.abilityValue);

                CardData cardData = Utils.CheckData(CardData.GetCardData, "CardData", card.Id);

                if (cardData == null)
                    yield break;

                value = cardData.def + sumDefUp - sumBrokeDef;
            }

            Debug.Log($"<color=red>--------------------------- {(isRelease ? "Release" : "Apply")} ------------------------------</color>");
            Debug.Log($"ArmorBreak {(isRelease ? "Release" : "Apply")}. card.Def: {card.Def} => {value}");

            targetSlot.SetDefense(value);

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, value));

            yield return new WaitForSeconds(0.1f);
        }
    }
}