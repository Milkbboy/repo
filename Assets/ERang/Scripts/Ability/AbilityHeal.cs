using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityHeal : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Heal;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            GameCard card = targetSlot.Card;

            if (card == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {targetSlot.LogText} 카드 없음");
                yield break;
            }

            int value = cardAbility.abilityValue;
            int beforeHp = card.State.Hp;
            int maxHp = card.State.MaxHp;

            GameLogger.LogAbilityDetail($"힐 어빌리티 - 대상: {card.Name}, 힐량: {value}, 현재HP: {beforeHp}/{maxHp}");

            // 최대 체력 확인
            if (beforeHp >= maxHp)
            {
                GameLogger.LogAbilityDetail($"{card.Name} 이미 최대 체력 - 힐 효과 없음");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            targetSlot.RestoreHealth(value);

            int afterHp = card.State.Hp;
            int actualHeal = afterHp - beforeHp;

            GameLogger.LogAbilityDetail($"힐 완료 - {card.Name} HP: {beforeHp} → {afterHp} (실제 힐량: {actualHeal})");


            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, card.State.Hp, actualHeal));

            yield return new WaitForSeconds(0.1f);
        }

        // 즉시 효과는 해제 불필요
        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            GameLogger.LogAbilityDetail("힐 어빌리티는 즉시 효과 - 해제 불필요");
            yield break;
        }
    }
}