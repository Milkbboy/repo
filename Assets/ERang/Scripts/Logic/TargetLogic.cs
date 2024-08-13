using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class TargetLogic : MonoBehaviour
    {
        public static TargetLogic Instance { get; private set; }

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
        /// 공격자 카드와 대상 카드를 비교하여 대상 카드 반환
        /// </summary>
        /// <param name="attackerCard"></param>
        /// <param name="targetCards"></param>
        public List<Card> CalulateTarget(Card attackerCard, List<Card> targetCards)
        {
            // 공격자 카드 기준으로 대상 카드 찾기
            AiData aiData = attackerCard.GetCardAiData();
            // aiData.atk_Ranges;
            // aiData.atk_Cnt;

            List<Card> targets = GetTargets(aiData, targetCards);

            Debug.Log($"TargetLogic.CalulateTarget() - attacker {attackerCard.id} target {targets.Count} ability_Ids {aiData.ability_Ids.Count}");

            if (targets == null)
            {
                Debug.LogError($"TargetLogic.CalulateTarget() - attacker {attackerCard.id} targets is null");
                return null;
            }

            for (int i = 0; i < aiData.ability_Ids.Count; i++)
            {
                AbilityData ability = AbilityData.GetAbilityData(aiData.ability_Ids[i]);

                // AbilityType > Damage 타입에는 데미지 값을 넣지 않을게.카드 데이터에 있는게 맞아 2024-08-12
                switch (ability.abilityType)
                {
                    case AbilityType.Damage:
                        foreach (Card target in targets)
                        {
                            for (int j = 0; j < aiData.atk_Cnt; ++j)
                            {
                                target.hp -= attackerCard.atk;
                            }

                            Debug.Log($"TargetLogic.CalulateTarget() - attacker {attackerCard.id} damage {attackerCard.atk} to target {target.id} hp {target.hp}");
                        }

                        break;
                }
            }

            return targets;
        }

        /// <summary>
        /// // AiData 에 설정된 타겟 얻기
        /// </summary>
        /// <param name="aiData"></param>
        List<Card> GetTargets(AiData aiData, List<Card> targetCards)
        {
            switch (aiData.type)
            {
                case AiDataType.Melee:
                    // 근거리 행동으로 Melee로 설정된 경우 행동 시 가장 가까운 적이 배치된 필드로 이동한다.
                    return MeleeAttack(aiData, targetCards);
                case AiDataType.Ranged:
                    // 원거리 행동으로 Ranged로 설정된 경우 제자리에서 행동한다.
                    break;
                case AiDataType.Explosion:
                    // 폭발 공격으로 Explosion로 설정된 경우 제자리에서 행동한다.
                    break;
                case AiDataType.Buff:
                    // 이로운 효과를 주는 버프 행동으로 제자리에서 행동한다. (Ranged와 동일하나 데이터 가독성을 위해 분리)
                    break;
                case AiDataType.DeBuff:
                    // 해로운 효과를 주는 디버프 행동으로 제자리에서 행동한다. (Ranged와 동일하나 데이터 가독성을 위해 분리)
                    break;
            }

            return null;
        }

        /// <summary>
        /// AiData 의 AtkType 은 Automatic 고정
        /// - 지금은 나중엔 마법 카드로 크리쳐가 공격하는 경우도 있을 수 있음 2024-08-12
        /// </summary>
        /// <param name="aiData"></param>
        List<Card> MeleeAttack(AiData aiData, List<Card> targetCards)
        {
            switch (aiData.target)
            {
                case AiDataTarget.NearEnemy:
                    // 가장 가까운 적을 대상으로 설정한다.
                    return new List<Card> { targetCards.FirstOrDefault() };
                case AiDataTarget.AllEnemy:
                    // 적 보스를 포함한 모든 적을 대상으로 한다.
                    return targetCards;
            }

            return null;
        }
    }
}