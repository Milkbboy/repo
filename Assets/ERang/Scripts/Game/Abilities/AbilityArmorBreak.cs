using System.Linq;
using System.Collections;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityArmorBreak : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.ArmorBreak;

        /// <summary>
        /// def 0 설정
        /// </summary>
        public override IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(targetSlot, false));
        }

        public override IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            yield return StartCoroutine(Apply(targetSlot, true));
        }

        /// <summary>
        /// def 원복
        /// </summary>
        private IEnumerator Apply(BSlot targetSlot, bool isRelease)
        {
            if (!ValidateTargetSlot(targetSlot, "ArmorBreak"))
                yield break;

            BaseCard targetCard = targetSlot.Card;
            int beforeDef = targetCard.Def;
            int newDef = 0;

            if (isRelease)
            {
                // 방어구 파괴 해제: 원래 방어력 복구
                newDef = CalculateRestoredDefense(targetCard);
                LogAbility($"방어구 파괴 해제: {beforeDef} -> {newDef}");
            }
            else
            {
                // 방어구 파괴 적용: 방어력 0으로 설정
                newDef = 0;
                LogAbility($"방어구 파괴 적용: {beforeDef} -> {newDef}");
            }

            targetSlot.SetDefense(newDef);

            yield return new WaitForSeconds(0.1f);
        }

        private int CalculateRestoredDefense(BaseCard card)
        {
            // 원래 방어력에서 다른 효과들을 계산
            var cardData = Utils.CheckData(CardData.GetCardData, "CardData", card.Id);
            if (cardData == null) return 0;

            int originalDef = cardData.def;
            int sumBrokenDef = card.AbilitySystem.BrokenDefAbilities.Sum(ability => ability.abilityValue);
            int sumDefUp = card.AbilitySystem.DefUpAbilities.Sum(ability => ability.abilityValue);

            int restoredDef = originalDef + sumDefUp - sumBrokenDef;
            return Mathf.Max(0, restoredDef); // 최소 0
        }
    }
}