using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AbilityAddMana : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AddMana;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots)
        {
            BaseCard card = selfSlot.Card;

            if (card == null || card is not MasterCard masterCard || abilityData == null)
                yield break;

            // 이 시점에서 masterCard는 MasterCard 타입으로 변환된 상태입니다.
            int beforeMana = masterCard.Mana;

            selfSlot.AdjustMana(abilityData.value);

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} <color=#257dca>마나 {abilityData.value} 추가 획득</color>({beforeMana} => {masterCard.Mana})");

            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator Release(CardAbility ability, BSlot selfSlot, BSlot targetSlot)
        {
            BaseCard card = selfSlot.Card;
            AbilityData abilityData = AbilityData.GetAbilityData(ability.abilityId);

            if (card == null || card is not MasterCard masterCard || abilityData == null)
                yield break;

            int beforeMana = masterCard.Mana;
            int amount = abilityData.value;

            selfSlot.AdjustMana(amount * -1);

            Debug.Log($"<color=#257dca>마나 {amount} 감소</color>({beforeMana} => {masterCard.Mana})");

            yield return new WaitForSeconds(0.1f);
        }
    }
}