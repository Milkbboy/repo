using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class BoardLogic : MonoBehaviour
    {
        public static BoardLogic Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 카드 atk 수치로 공격
        /// </summary>
        public void AbilityDamage(AiDataType aiType, int atkCount, BoardSlot selfSlot, List<BoardSlot> targetSlots)
        {
            switch (aiType)
            {
                case AiDataType.Melee: StartCoroutine(MeleeAttack(selfSlot, targetSlots, atkCount, selfSlot.Card.atk)); break;
                case AiDataType.Ranged: StartCoroutine(RangedAttack(selfSlot, targetSlots, atkCount, selfSlot.Card.atk)); break;
                default: Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} AiDataType {aiType} 미구현(Card.atk) - BoardLogic.AbilityDamage"); break;
            }
        }

        public void AbilityDamage(AiDataType aiType, int atkCount, BoardSlot selfSlot, List<BoardSlot> targetSlots, int damage)
        {
            switch (aiType)
            {
                case AiDataType.Melee: StartCoroutine(MeleeAttack(selfSlot, targetSlots, atkCount, damage)); break;
                case AiDataType.Ranged: StartCoroutine(RangedAttack(selfSlot, targetSlots, atkCount, damage)); break;
                default: Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} AiDataType {aiType} 미구현(damate) - BoardLogic.AbilityDamage"); break;
            }
        }

        /// <summary>
        /// 체력 효과
        /// </summary>
        public IEnumerator AbilityHp(List<BoardSlot> targetSlots, int value)
        {
            // 카드 회복 애니메이션 로직을 여기에 추가

            var changes = new List<(bool isAffect, int slot, int cardId, int before, int after)>();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    changes.Add((false, targetSlot.Slot, 0, 0, 0));
                    continue;
                }

                changes.Add((true, targetSlot.Slot, targetSlot.Card.Id, targetSlot.Card.hp, targetSlot.Card.hp + value));
                targetSlot.AddCardHp(value);
            }

            Debug.Log($"{Utils.StatChangesText("체력", changes)} - BoardLogic.AbilityHp");

            yield return new WaitForSeconds(.5f);
        }

        /// <summary>
        /// 공격력 효과
        /// </summary>
        public IEnumerator AbilityAtk(List<BoardSlot> targetSlots, int value)
        {
            // 공격력 증가 애니메이션 로직을 여기에 추가

            var changes = new List<(bool isAffect, int slot, int cardId, int before, int after)>();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    changes.Add((false, targetSlot.Slot, 0, 0, 0));
                    continue;
                }

                changes.Add((targetSlot.Card != null, targetSlot.Slot, targetSlot.Card.Id, targetSlot.Card.atk, targetSlot.Card.atk + value));
                targetSlot.AddCardAtk(value);
            }

            Debug.Log($"{Utils.StatChangesText("공격력", changes)} - BoardLogic.AbilityAtk");

            yield return new WaitForSeconds(.5f);
        }

        /// <summary>
        /// 방어력 효과
        /// </summary>
        public IEnumerator AbilityDef(List<BoardSlot> targetSlots, int value)
        {
            // 방어력 증가 애니메이션 로직을 여기에 추가

            var changes = new List<(bool isAffect, int slot, int cardId, int before, int after)>();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                if (targetSlot.Card == null)
                {
                    changes.Add((false, targetSlot.Slot, 0, 0, 0));
                    continue;
                }

                changes.Add((true, targetSlot.Slot, targetSlot.Card.Id, targetSlot.Card.def, targetSlot.Card.def + value));
                targetSlot.AddCardDef(value);
            }

            Debug.Log($"{Utils.StatChangesText("방어력", changes)} - BoardLogic.AbilityDef");

            yield return new WaitForSeconds(.5f);
        }

        /// <summary>
        /// 충전 효과
        /// </summary>
        public IEnumerator AbilityChargeDamage(AiData aiData, BoardSlot selfSlot, List<BoardSlot> targetSlots, int damage)
        {
            // 충전 애니메이션 로직을 여기에 추가
            // 임시로 공격 애니메이션으로 대체
            selfSlot.AniAttack();

            yield return new WaitForSeconds(.5f);

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 충전 완료 - BoardLogic.AbilityChargeDamage");
        }

        /// <summary>
        /// 마나 추기 획득 효과
        /// </summary>
        public IEnumerator AbilityAddMana(BoardSlot selfSlot, int value)
        {
            // 마나 획득 애니메이션 로직을 여기에 추가

            int beforeMana = Master.Instance.Mana;

            // 마나 추가 획득
            BattleLogic.Instance.AddMasterMana(value);

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} <color=#257dca>마나 {value} 추가 획득</color>({beforeMana} => {Master.Instance.Mana}) - BoardLogic.AffectAddMana");

            yield return new WaitForSeconds(.5f);
        }

        public void AbilityAddGoldPer(AiData aiData, AbilityData abilityData, BoardSlot selfSlot)
        {
            // 골드 획득량 증가 애니메이션 로직을 여기에 추가
            // 골드 추가 획득
            float gainGold = aiData.value * abilityData.ratio;
            int gold = aiData.value + (int)gainGold;
            int beforeGold = Master.Instance.Gold;

            BattleLogic.Instance.AddMasterGold(gold);

            // 골드 획득량 표시
            selfSlot.SetFloatingGold(beforeGold, Master.Instance.Gold);
        }

        /// <summary>
        /// 근접 공격 효과
        /// </summary>
        private IEnumerator MeleeAttack(BoardSlot selfSlot, List<BoardSlot> targetSlots, int ackCount, int damage)
        {
            selfSlot.AniAttack();

            // 근접 공격
            foreach (BoardSlot targetSlot in targetSlots)
            {
                for (int i = 0; i < ackCount; i++)
                {
                    targetSlot.SetDamage(damage);
                    targetSlot.AniDamaged();

                    yield return new WaitForSeconds(0.5f);
                }
            }

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 타겟({Utils.BoardSlotNumersText(targetSlots)}) 근접 공격({damage}) {ackCount}회 완료 - BoardLogic.MeleeAttack");
        }

        /// <summary>
        /// 원거리 공격
        /// </summary>
        private IEnumerator RangedAttack(BoardSlot selfSlot, List<BoardSlot> targetSlots, int ackCount, int damage)
        {
            selfSlot.AniAttack();

            yield return StartCoroutine(FireMissile(selfSlot, targetSlots, ackCount, damage));

            Debug.Log($"{Utils.BoardSlotLog(selfSlot)} 타겟({Utils.BoardSlotNumersText(targetSlots)}) 원거리 공격({damage}) {ackCount}회 완료 - BoardLogic.MeleeAttack");
        }

        /// <summary>
        /// 미사일 발사
        /// </summary>
        private IEnumerator FireMissile(BoardSlot selfSlot, List<BoardSlot> targetSlots, int ackCount, int damage)
        {
            Vector3 startPosition = selfSlot.transform.position;
            List<(GameObject, Vector3)> missiles = new List<(GameObject, Vector3)>();

            foreach (BoardSlot targetSlot in targetSlots)
            {
                Vector3 targetPosition = targetSlot.transform.position;

                GameObject missile = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 임시로 구체를 미사일로 사용
                missile.transform.position = startPosition;
                missile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // 미사일 크기 조정

                missiles.Add((missile, targetPosition));
            }

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                foreach (var missile in missiles)
                    missile.Item1.transform.position = CalculateParabolicPath(startPosition, missile.Item2, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < ackCount; i++)
            {
                foreach (BoardSlot targetSlot in targetSlots)
                {
                    targetSlot.AniDamaged();
                    targetSlot.SetDamage(damage);
                }
                yield return new WaitForSeconds(0.5f);
            }

            foreach (var missile in missiles)
                Destroy(missile.Item1);
        }

        private Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float t)
        {
            float height = 5.0f; // 포물선의 높이
            float parabola = 4 * height * t * (1 - t);
            Vector3 midPoint = Vector3.Lerp(start, end, t);
            return new Vector3(midPoint.x, midPoint.y + parabola, midPoint.z);
        }
    }
}