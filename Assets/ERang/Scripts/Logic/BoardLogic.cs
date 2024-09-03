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
        /// 대상에게 value 만큼의 데미지를 준다.
        /// </summary>
        /// <param name="aiType"></param>
        /// <param name="selfSlot"></param>
        /// <param name="targetSlot"></param>
        /// <param name="ackCount"></param>
        /// <param name="damage"></param>
        public void AbilityDamage(AiDataType aiType, BoardSlot selfSlot, BoardSlot targetSlot, int ackCount, int value)
        {
            switch (aiType)
            {
                case AiDataType.Melee: StartCoroutine(MeleeAttack(selfSlot, targetSlot, ackCount, value)); break;
            }
        }

        /// <summary>
        /// 대상의 체력을 value 만큼 회복한다.
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="aiType"></param>
        /// <param name="abilityData"></param>
        /// <param name="originValue"></param>
        public void AbilityHeal(BoardSlot targetSlot, AiDataType aiType, AbilityData abilityData, int originValue)
        {
            int healValue = abilityData.value;
            StartCoroutine(AffectHp(targetSlot, healValue));

            // 카드 지속 어빌리티에 내용 추가
            targetSlot.Card.AddAbilityDuration(aiType, abilityData.abilityType, abilityData.abilityData_Id, originValue, healValue, abilityData.duration, targetSlot.Card.uid, targetSlot.Slot);
        }

        /// <summary>
        /// 대상의 공격력을 value 만큰 duration 턴 동안 상승시킨다.
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="aiType"></param>
        /// <param name="abilityData"></param>
        /// <param name="originValue"></param>
        public void AbilityAttakUp(BoardSlot targetSlot, AiDataType aiType, AbilityData abilityData, int originValue)
        {
            int atkUpValue = abilityData.value;

            StartCoroutine(AffectAtk(targetSlot, atkUpValue));
            targetSlot.Card.AddAbilityDuration(aiType, abilityData.abilityType, abilityData.abilityData_Id, originValue, abilityData.value, abilityData.duration, targetSlot.Card.uid, targetSlot.Slot);
        }

        /// <summary>
        /// 대상의 방어력을 value 만큰 duration 턴 동안 상승시킨다.
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="aiType"></param>
        /// <param name="abilityData"></param>
        /// <param name="originValue"></param>
        public void AbilityDefUp(BoardSlot targetSlot, AiDataType aiType, AbilityData abilityData, int originValue)
        {
            int defUpValue = abilityData.value;

            StartCoroutine(AffectDef(targetSlot, defUpValue));
            targetSlot.Card.AddAbilityDuration(aiType, abilityData.abilityType, abilityData.abilityData_Id, originValue, abilityData.value, abilityData.duration, targetSlot.Card.uid, targetSlot.Slot);
        }

        /// <summary>
        /// 대상의 방어력을 value 만큼 duration 턴 동안 감소시킨다.
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="aiType"></param>
        /// <param name="abilityData"></param>
        /// <param name="originValue"></param>
        public void AbilityBrokenDef(BoardSlot targetSlot, AiDataType aiType, AbilityData abilityData, int originValue)
        {
            int brokenDefValue = -abilityData.value;

            targetSlot.SetCardDef(brokenDefValue);
            targetSlot.Card.AddAbilityDuration(aiType, abilityData.abilityType, abilityData.abilityData_Id, originValue, brokenDefValue, abilityData.duration, targetSlot.Card.uid, targetSlot.Slot);
        }

        /// <summary>
        /// duration 만큼 공격을 충전 후, 대상에게 value 만큼의 데미지를 준다.
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="aiType"></param>
        /// <param name="abilityData"></param>
        /// <param name="originValue"></param>
        public void AbilityChargeDamage(BoardSlot targetSlot, AiDataType aiType, AbilityData abilityData, int originValue)
        {
            int chargeDamageValue = abilityData.value;

            targetSlot.SetDamage(chargeDamageValue);
            targetSlot.Card.AddAbilityDuration(aiType, abilityData.abilityType, abilityData.abilityData_Id, originValue, chargeDamageValue, abilityData.duration, targetSlot.Card.uid, targetSlot.Slot);
        }

        /// <summary>
        /// 근접 공격 효과
        /// </summary>
        /// <param name="selfSlot"></param>
        /// <param name="targetSlot"></param>
        /// <param name="ackCount"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        private IEnumerator MeleeAttack(BoardSlot selfSlot, BoardSlot targetSlot, int ackCount, int damage)
        {
            // 원래 위치 저장
            Vector3 originalPosition = selfSlot.transform.position;

            // 대상 카드로 이동
            yield return StartCoroutine(MoveCard(selfSlot, targetSlot.transform.position));

            // 근접 공격
            for (int i = 0; i < ackCount; i++)
            {
                targetSlot.SetDamage(damage);
                yield return new WaitForSeconds(0.5f);
            }

            // 원래 자리로 이동
            yield return StartCoroutine(MoveCard(selfSlot, originalPosition));
        }

        /// <summary>
        /// 보드 슬롯 이동
        /// </summary>
        /// <param name="fromSlot"></param>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        private IEnumerator MoveCard(BoardSlot fromSlot, Vector3 toPosition)
        {
            // 카드 이동 애니메이션 로직을 여기에 추가
            // 예를 들어, 카드가 특정 위치로 이동하는 애니메이션
            Vector3 startPosition = fromSlot.transform.position;
            float duration = 1.0f; // 애니메이션 지속 시간

            float elapsed = 0f;
            while (elapsed < duration)
            {
                fromSlot.transform.position = Vector3.Lerp(startPosition, toPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            fromSlot.transform.position = toPosition;
        }

        /// <summary>
        /// 체력 효과
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerator AffectHp(BoardSlot targetSlot, int value)
        {
            // 카드 회복 애니메이션 로직을 여기에 추가

            targetSlot.AddCardHp(value);

            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// 공격력 효과
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerator AffectAtk(BoardSlot targetSlot, int value)
        {
            // 공격력 증가 애니메이션 로직을 여기에 추가

            targetSlot.SetCardAtk(value);

            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// 방어력 효과
        /// </summary>
        /// <param name="targetSlot"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerator AffectDef(BoardSlot targetSlot, int value)
        {
            // 방어력 증가 애니메이션 로직을 여기에 추가

            targetSlot.SetCardDef(value);

            yield return new WaitForSeconds(1f);
        }
    }
}