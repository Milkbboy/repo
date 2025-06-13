using System.Collections;
using UnityEngine;

namespace ERang
{
    public class AbilityAddMana : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.AddMana;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, selfSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isAdd)
        {
            if (!ValidateTargetSlot(targetSlot, "AddMana"))
                yield break;

            // 마나는 마스터 카드만 가능
            if (!ValidateCardType<MasterCard>(targetSlot.Card, "AddMana"))
                yield break;

            MasterCard masterCard = targetSlot.Card as MasterCard;
            int value = cardAbility.abilityValue;

            if (value <= 0)
            {
                LogAbility($"잘못된 마나 값: {value}", LogType.Warning);
                yield break;
            }

            int beforeMana = masterCard.Mana;

            if (isAdd)
            {
                masterCard.IncreaseMana(value);
                LogAbility($"마나 증가: {beforeMana} -> {masterCard.Mana} (+{value})");
            }
            else
            {
                masterCard.DecreaseMana(value);
                LogAbility($"마나 감소: {beforeMana} -> {masterCard.Mana} (-{value})");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}