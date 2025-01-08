using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class AbilityArmorBreak : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.ArmorBreak;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            yield return StartCoroutine(ApplySingle(aiData, abilityData, selfSlot, null));
        }

        /// <summary>
        /// Def 0 설정
        /// </summary>
        public IEnumerator ApplySingle(AiData aiData, AbilityData abilityData, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText} 카드 없음.");
                yield break;
            }

            int before = card.Def;
            int change = 0;

            Debug.Log("<color=red>--------------------------- Apply ------------------------------</color>");
            Debug.Log($"ArmorBreak Release. card.Def: {card.Def}");

            targetSlot.SetDefense(change);

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, change));

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Def 원복
        /// </summary>
        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{targetSlot.LogText}");
                yield break;
            }

            CardData cardData = CardData.GetCardData(card.Id);

            // 감소할 def
            int sumBrokeDef = card.BrokenDefAbilities.Sum(ability => ability.abilityValue);
            // 더할 def
            int sumDefUp = card.DefUpAbilities.Sum(ability => ability.abilityValue);

            Debug.Log("<color=red>--------------------------- Release ------------------------------</color>");
            Debug.Log($"ArmorBreak Release. cardData.def: {cardData.def}, sumBrokeDef: {sumBrokeDef}, sumDefUp: {sumDefUp}");

            int before = card.Def;
            int change = cardData.def + sumDefUp - sumBrokeDef;

            targetSlot.SetDefense(change);

            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, before, card.Def, change));

            yield return new WaitForSeconds(0.1f);
        }
    }
}