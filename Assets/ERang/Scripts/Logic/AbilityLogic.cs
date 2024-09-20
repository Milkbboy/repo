using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Ability
    {
        public AbilityWhereFrom whereFrom; // 어빌리티 적용 위치
        public int atkCount; // 공격 횟수
        public AiDataType aiType; // Ai 타입
        public AiDataAttackType aiAttackType; // Ai 공격 타입
        public int abilityId; // 어빌리티 Id
        public AbilityType abilityType; // 어빌리티 타입
        public AbilityWorkType abilityWorkType; // 어빌리티 작업 타입(HandOn 찾기 위함)
        public int beforeValue; // 어빌리티 적용 전 값
        public int abilityValue; // 어빌리티 값
        public int duration; // 현재 지속 시간
        public int selfBoardSlot; // 어빌리티 발동 보드 슬롯
        public string selfCardUid; // 어빌리티 발동 카드 Uid
        public int targetBoardSlot; // 어빌리티 대상 보드 슬롯
        public int targetCardId; // 어빌리티 대상 카드 Id
        public string targetCardUid; // 어빌리티 대상 카드 Uid
    }

    public class AbilityLogic : MonoBehaviour
    {
        public static AbilityLogic Instance { get; private set; }

        public List<Ability> abilities = new();

        void Awake()
        {
            Instance = this;
        }

        public List<Ability> GetAbilities()
        {
            return abilities;
        }

        public List<Ability> GetHandOnAbilities()
        {
            return abilities.FindAll(ability => ability.whereFrom == AbilityWhereFrom.TurnStarHandOn);
        }

        public List<Ability> GetBoardSlotCardAbilities()
        {
            return abilities.FindAll(ability => ability.whereFrom != AbilityWhereFrom.TurnStarHandOn);
        }

        /// <summary>
        /// 카드 버프 개수 얻기
        /// </summary>
        public int GetBuffCount(string cardUid)
        {
            return abilities.Count(ability => ability.targetCardUid == cardUid && ability.aiType == AiDataType.Buff);
        }

        /// <summary>
        /// 카드 디버프 개수 얻기
        /// </summary>
        public int GetDebuffCount(string cardUid)
        {
            return abilities.Count(ability => ability.targetCardUid == cardUid && ability.aiType == AiDataType.Debuff);
        }

        /// <summary>
        /// 카드 어빌리티 제거 - card uid
        /// </summary>
        public void RemoveAbility(string cardUid)
        {
            abilities.RemoveAll(ability => ability.targetCardUid == cardUid);
        }

        /// <summary>
        /// 핸드 온 어빌리티 추가
        /// </summary>
        public void AddHandOnAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            // 핸드 온 어빌리티는 턴 종료시 해제되기 때문에 모두 저장
            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }
        }

        /// <summary>
        /// 어빌리티 추가
        /// </summary>
        public void AddAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            foreach (BoardSlot targetSlot in targetSlots)
            {
                Ability durationAbility = abilities.Find(a => a.abilityId == abilityData.abilityId && a.targetBoardSlot == targetSlot.Slot);

                // 이미 어빌리티가 적용 중이면 패스
                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 이미 {Utils.AbilityLog(abilityData)}가 적용 중으로 해당 어빌리티 패스");
                    continue;
                }

                // 효과 지속 시간이 있는 어빌리티만 추가
                if (abilityData.duration <= 0)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }
        }

        public IEnumerator AbilityAction(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots, AbilityWhereFrom whereFrom)
        {
            string abilityLog = $"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)} 타겟 (슬롯, 카드): {string.Join(", ", targetSlots.Select(slot => (slot.Slot, slot.Card?.Id)))}";

            List<BoardSlot> passedSlots = new();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                Ability durationAbility = abilities.Find(a => a.abilityId == abilityData.abilityId && a.targetBoardSlot == targetSlot.Slot);

                // 이미 어빌리티가 적용 중이면 패스
                if (durationAbility != null)
                {
                    Debug.LogWarning($"{Utils.BoardSlotLog(targetSlot)} 이미 {Utils.AbilityLog(abilityData)} 가 적용 중 패스");
                    passedSlots.Add(targetSlot);
                    continue;
                }

                // 효과 지속 시간이 있는 어빌리티만 추가
                if (abilityData.duration <= 0)
                    continue;

                AddAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);
            }

            // passedSlots에 추가된 슬롯은 어빌리티 적용 대상에서 제외
            if (passedSlots.Count > 0)
            {
                // Debug.Log($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)} 적용 제외 슬롯: {string.Join(", ", passedSlots.Select(slot => slot.Slot))}");
                targetSlots = targetSlots.Except(passedSlots).ToList();
            }

            // Debug.Log($"{abilityLog} 적용 시작");

            if (targetSlots.Count > 0)
                yield return StartCoroutine(AbilityAction(aiData, abilityData, selfSlot, targetSlots));
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public IEnumerator AbilityAction(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            int value = abilityData.value;
            var changes = new List<(bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)>();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                    changes.Add((false, targetSlot.Slot, 0, targetSlot.CardType, 0, 0, 0));
            }

            int beforeValue = 0;
            int afterValue = 0;
            int changeValue = 0;

            switch (abilityData.abilityType)
            {
                case AbilityType.Damage:
                    yield return StartCoroutine(AttackAbility(aiData.type, aiData.attackType, abilityData.abilityType, selfSlot, targetSlots, aiData.atk_Cnt, abilityData.value));
                    break;

                case AbilityType.Heal:
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                            continue;

                        beforeValue = targetSlot.Card.hp;
                        afterValue = targetSlot.Card.hp + abilityData.value;
                        changeValue = abilityData.value;

                        targetSlot.AddCardHp(value);

                        // Debug.Log($"{Utils.StatChangesText("hp", changes)} - <color=#f4872e>{AbilityType.Heal} 어빌리티</color> 적용");
                    }
                    break;

                case AbilityType.AtkUp:
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                            continue;

                        beforeValue = targetSlot.Card.atk;
                        afterValue = targetSlot.Card.atk + abilityData.value;
                        changeValue = abilityData.value;

                        targetSlot.AddCardAtk(value);

                        // Debug.Log($"{Utils.StatChangesText("atk", changes)} - <color=#f4872e>{AbilityType.AtkUp} 어빌리티</color> 적용");
                    }
                    break;

                case AbilityType.DefUp:
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                            continue;

                        beforeValue = targetSlot.Card.def;
                        afterValue = targetSlot.Card.def + abilityData.value;
                        changeValue = abilityData.value;

                        targetSlot.AddCardDef(value);

                        // Debug.Log($"{Utils.StatChangesText("def", changes)} - <color=#f4872e>{AbilityType.DefUp} 어빌리티</color> 적용");
                    }
                    break;

                case AbilityType.BrokenDef:
                    foreach (BoardSlot targetSlot in targetSlots)
                    {
                        if (targetSlot.Card == null)
                            continue;

                        beforeValue = targetSlot.Card.def;
                        afterValue = targetSlot.Card.def - value;
                        changeValue = -value;

                        targetSlot.AddCardDef(-value);
                        // Debug.Log($"{Utils.StatChangesText("def", changes)} - <color=#f4872e>{AbilityType.BrokenDef} 어빌리티</color> 적용");
                    }
                    break;

                case AbilityType.ChargeDamage:
                    // 차징 애니메이션
                    selfSlot.AniAttack();
                    break;

                case AbilityType.AddMana:
                    int beforeMana = Master.Instance.Mana;

                    // 마나 추가 획득
                    BoardSystem.Instance.AddMana(Master.Instance, value);
                    Debug.Log($"{Utils.BoardSlotLog(selfSlot)} <color=#257dca>마나 {value} 추가 획득</color>({beforeMana} => {Master.Instance.Mana})");
                    break;

                case AbilityType.AddGoldPer:
                    {
                        float gainGold = aiData.value * abilityData.ratio;
                        int gold = aiData.value + (int)gainGold;
                        int beforeGold = Master.Instance.Gold;

                        BoardSystem.Instance.AddGold(Master.Instance, gold);

                        // 골드 획득량 표시
                        selfSlot.SetFloatingGold(beforeGold, Master.Instance.Gold);
                    }
                    break;

                default:
                    Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)} 에 대한 동작이 없음");
                    break;
            }

            if (beforeValue != 0 || afterValue != 0 || changeValue != 0)
                changes.Add((true, selfSlot.Slot, selfSlot.Card.Id, selfSlot.CardType, beforeValue, afterValue, changeValue));

            // Damage 는 내부 함수에서 출력
            if (changes.Count > 0 && abilityData.abilityType != AbilityType.Damage)
                Debug.Log($"{Utils.BoardSlotLog(selfSlot)} {Utils.AbilityLog(abilityData)}로 {Utils.StatChangesText(abilityData.abilityType, changes)}");
        }

        /// <summary>
        /// 보드 슬롯에 장착된 카드 ability 해제
        /// </summary>
        public IEnumerator AbilityRelease(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            string abilityActionLog = $"{Utils.BoardSlotLog(targetSlot)} {Utils.AbilityLog(ability.abilityType, ability.abilityId)} 효과 지속 시간(<color=yellow>{ability.duration}</color>)";

            Card card = targetSlot.Card;

            if (card == null)
            {
                Debug.LogWarning($"{abilityActionLog} - 카드 없음. 어빌리티 삭제");

                abilities.Remove(ability);
                yield break;
            }

            var changes = new List<(bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)>();

            int beforeValue = 0;
            int afterValue = 0;
            int changeValue = 0;

            // 버프, 디버프는 해제, 차징 공격은 타겟 슬롯에 데미지 적용
            switch (ability.abilityType)
            {
                case AbilityType.AtkUp:
                    {
                        beforeValue = card.atk;

                        targetSlot.AddCardAtk(-ability.abilityValue);

                        afterValue = card.atk;
                        changeValue = -ability.abilityValue;
                    }
                    break;

                case AbilityType.DefUp:
                    {
                        beforeValue = card.def;

                        targetSlot.AddCardDef(-ability.abilityValue);

                        afterValue = card.def;
                        changeValue = -ability.abilityValue;
                    }
                    break;

                case AbilityType.BrokenDef:
                    {
                        beforeValue = card.def;

                        targetSlot.AddCardDef(ability.abilityValue);

                        afterValue = card.def;
                        changeValue = ability.abilityValue;
                    }
                    break;

                case AbilityType.AddMana:
                    {
                        int beforeMana = Master.Instance.Mana;

                        BoardSystem.Instance.AddMana(Master.Instance, ability.abilityValue * -1);

                        Debug.Log($"{abilityActionLog} <color=#257dca>마나 {ability.abilityValue} 감소</color>({beforeMana} => {Master.Instance.Mana})");
                    }
                    break;

                case AbilityType.ChargeDamage:
                    {
                        yield return StartCoroutine(AttackAbility(ability.aiType, ability.aiAttackType, ability.abilityType, selfSlot, new List<BoardSlot> { targetSlot }, ability.atkCount, ability.abilityValue));

                        Debug.Log($"{abilityActionLog} {(targetSlot.Card != null ? $"데미지 {ability.abilityValue} 적용" : "해제")} - AiLogic.AbilityAction");
                    }
                    break;

                default:
                    Debug.LogWarning($"{abilityActionLog} - 아직 구현되지 않음.");
                    break;
            }

            changes.Add((true, targetSlot.Slot, card.Id, targetSlot.CardType, beforeValue, afterValue, changeValue));

            if (changes.Count > 0)
                Debug.Log($"{abilityActionLog} 해제. {Utils.StatChangesText(ability.abilityType, changes)}");

            abilities.Remove(ability);
        }

        /// <summary>
        /// 공격 어빌리티
        /// </summary>
        private IEnumerator AttackAbility(AiDataType aiDatyType, AiDataAttackType aiAttackType, AbilityType abilityType, BoardSlot selfSlot, List<BoardSlot> targetSlots, int atkCount, int abilityValue)
        {
            var changes = new List<(bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)>();

            selfSlot.AniAttack();

            // 카드 선택 공격 타입이면 어빌리티 데미지 값으로 설정
            int damage = Constants.SelectAttackTypes.Contains(aiAttackType) ? abilityValue : selfSlot.Card.atk;

            // 원거리 미사일 발사
            if (aiDatyType == AiDataType.Ranged)
                yield return StartCoroutine(BoardLogic.Instance.FireMissile(selfSlot, targetSlots, atkCount, damage));

            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    changes.Add((false, targetSlot.Slot, 0, targetSlot.CardType, 0, 0, 0));
                    continue;
                }

                int beforeValue = targetSlot.Card.hp;

                for (int i = 0; i < atkCount; i++)
                {
                    targetSlot.SetDamage(damage);
                    targetSlot.AniDamaged();

                    yield return new WaitForSeconds(0.5f);
                }

                changes.Add((true, targetSlot.Slot, targetSlot.Card.Id, targetSlot.CardType, beforeValue, targetSlot.Card.hp, damage * atkCount));
            }

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} <color=#f4872e>{abilityType} 어빌리티</color>로 {Utils.StatChangesText(abilityType, changes)}");
        }

        /// <summary>
        /// 어빌리티 생성
        /// </summary>
        public Ability MakeAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, BoardSlot targetSlot, AbilityWhereFrom whereFrom)
        {
            int beforeValue = GetOriginStatValue(abilityData.abilityType, targetSlot);
            // Debug.Log($"{Utils.AbilityLog(abilityData)} 생성. 슬롯 {selfSlot.Slot} => {targetSlot.Slot}, beforeValue: {beforeValue}, value: {abilityData.value}, duration: {abilityData.duration}");

            Ability ability = new()
            {
                whereFrom = whereFrom,
                aiType = aiData.type,
                aiAttackType = aiData.attackType,
                atkCount = aiData.atk_Cnt,
                abilityId = abilityData.abilityId,
                abilityType = abilityData.abilityType,
                abilityWorkType = abilityData.workType,
                abilityValue = abilityData.value,
                duration = abilityData.duration,
                beforeValue = beforeValue,
                selfBoardSlot = selfSlot.Slot,
                selfCardUid = selfSlot.Card?.Uid ?? string.Empty,
                targetBoardSlot = targetSlot.Slot,
                targetCardId = targetSlot.Card?.Id ?? 0,
                targetCardUid = targetSlot.Card?.Uid ?? string.Empty
            };

            return ability;
        }

        /// <summary>
        /// 어빌리티 추가
        /// </summary>
        private void AddAbility(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, BoardSlot targetSlot, AbilityWhereFrom whereFrom)
        {
            Ability ability = MakeAbility(aiData, abilityData, selfSlot, targetSlot, whereFrom);

            abilities.Add(ability);
        }

        /// <summary>
        /// 어빌리티 타입에 따른 원래 stat 얻기
        /// </summary>
        private int GetOriginStatValue(AbilityType abilityType, BoardSlot boardSlot)
        {
            if (boardSlot.Card == null)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(boardSlot)} <color=#f4872e>{abilityType} 어빌리티</color> 적용 슬롯 카드 없음");
                return 0;
            }

            return abilityType switch
            {
                AbilityType.AtkUp => boardSlot.Card.atk,
                AbilityType.DefUp or AbilityType.BrokenDef => boardSlot.Card.def,
                AbilityType.Heal => boardSlot.Card.hp,
                _ => 0,
            };
        }
    }
}