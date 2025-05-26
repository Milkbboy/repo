using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityBurn : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.Burn;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            GameCard card = targetSlot.Card;

            if (card == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {targetSlot.LogText} 카드 없음");
                yield break;
            }

            GameLogger.LogAbilityDetail($"화상 부여 - 대상: {card.Name}, 지속시간: {cardAbility.duration}턴, 틱당 데미지: {cardAbility.abilityValue}");

            // 기존 화상 효과 확인
            var existingBurn = card.AbilitySystem.CardAbilities.Find(a => a.abilityType == AbilityType.Burn);
            if (existingBurn != null)
            {
                GameLogger.LogAbilityDetail($"기존 화상 효과 제거 - 남은 지속시간: {existingBurn.duration}턴");
            }

            int value = cardAbility.abilityValue;

            int beforeHp = card.State.Hp;
            int beforeDef = card.State.Def;

            yield return StartCoroutine(targetSlot.TakeDamage(value));

            // 화상 상태이상은 즉시 효과가 아니라 매턴 발동되므로 여기서는 상태만 기록
            GameLogger.LogAbilityDetail($"화상 상태 부여 완료 - {card.Name}에 {cardAbility.duration}턴간 매턴 {cardAbility.abilityValue} 데미지");

            // 카드가 hp 0 으로 제거되는 경우도 있음
            Changes.Add((StatType.Hp, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeHp, card.State.Hp, value));
            Changes.Add((StatType.Def, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeDef, card.State.Def, value));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            GameCard card = targetSlot.Card;

            if (card == null)
            {
                GameLogger.LogAbilityDetail("화상 해제 대상 카드 없음 (이미 제거됨)");
                yield break;
            }

            GameLogger.LogAbilityDetail($"화상 상태 해제 - {card.Name}의 화상 효과 종료");

            yield return new WaitForSeconds(0.1f);
        }
    }
}