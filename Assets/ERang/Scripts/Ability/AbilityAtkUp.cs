using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class AbilityAtkUp : MonoBehaviour, IAbility
    {
        public AbilityType AbilityType => AbilityType.AtkUp;
        public List<(StatType, bool, int, int, CardType, int, int, int)> Changes { get; set; } = new();

        public IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            GameLogger.LogAbilityDetail($"공격력 버프 적용 시작 - 대상: {targetSlot.Card?.Name ?? targetSlot.LogText}");
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot)
        {
            GameLogger.LogAbilityDetail($"공격력 버프 해제 시작 - 대상: {targetSlot.Card?.Name ?? targetSlot.LogText}");
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        private IEnumerator Apply(CardAbility cardAbility, BSlot targetSlot, bool isAtkUp)
        {
            GameCard card = targetSlot.Card;

            if (card == null)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {targetSlot.LogText} 카드 없음");
                yield break;
            }

            if (card.CardType != CardType.Creature)
            {
                GameLogger.Log(LogCategory.ERROR, $"❌ {card.LogText} 크리쳐 카드 아님. 현재 타입: {card.CardType}");
                yield break;
            }

            int beforeAtk = card.State.Atk;
            int value = cardAbility.abilityValue;

            GameLogger.LogAbilityDetail($"공격력 {(isAtkUp ? "증가" : "감소")} - {card.Name}: {beforeAtk} {(isAtkUp ? "+" : "-")}{value}");

            if (isAtkUp)
                card.IncreaseAttack(value);
            else
                card.DecreaseAttack(value);

            int afterAtk = card.State.Atk;

            GameLogger.LogAbilityDetail($"공격력 변경 완료 - {card.Name}: {beforeAtk} → {afterAtk}");

            Changes.Add((StatType.Atk, true, targetSlot.SlotNum, card.Id, targetSlot.SlotCardType, beforeAtk, card.State.Atk, isAtkUp ? value : value * -1));

            yield return new WaitForSeconds(0.1f);
        }
    }
}