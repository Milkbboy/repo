using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class MagicCard : GameCard, ITargetingCard
    {
        #region MagicCard 특유 속성 (게임 로직)
        /// <summary>
        /// 핸드에 있을 때 지속 효과가 있는지 (게임 규칙)
        /// </summary>
        public bool IsHandOnCard { get; private set; }

        /// <summary>
        /// AI가 타겟으로 삼을 슬롯 번호들 (AI 로직)
        /// </summary>
        public List<int> TargetSlotNumbers { get; private set; }

        /// <summary>
        /// 플레이어가 타겟을 직접 선택하는 공격 타입인지 (게임 규칙)
        /// </summary>
        public bool IsSelectAttackType { get; private set; }
        #endregion

        #region ITargetingCard 구현 (UI/상호작용)
        /// <summary>
        /// UI에서 타겟 가능한 카드 타입들
        /// </summary>
        public List<CardType> ValidTargets { get; protected set; }

        /// <summary>
        /// UI에서 타겟 선택이 필수인지
        /// </summary>
        public bool RequiresTarget { get; protected set; }

        /// <summary>
        /// 자기 자신을 타겟할 수 있는지
        /// </summary>
        public bool CanTargetSelf { get; protected set; }

        /// <summary>
        /// 빈 슬롯을 타겟할 수 있는지
        /// </summary>
        public bool CanTargetEmpty { get; protected set; }
        #endregion

        public MagicCard() : base()
        {
            // 마법 카드는 전투 기능과 어빌리티 기능만 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = true;
            usesAi = true;
            CardType = CardType.Magic;

            TargetSlotNumbers = new List<int>();
            InitializeTargeting();
        }

        public MagicCard(CardData cardData) : base(cardData)
        {
            // 마법 카드는 전투 기능과 어빌리티 기능만 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = false;
            usesAi = true;

            InitializeTargeting();
        }

        #region 게임 로직 메서드들
        public void SetHandOnCard(bool isHandOnCard)
        {
            IsHandOnCard = isHandOnCard;
        }

        public void SetTargetSlotNumbers(List<int> targetSlotNumbers)
        {
            TargetSlotNumbers = targetSlotNumbers;
        }

        public void SetSelectAttackType(bool isSelectAttackType)
        {
            IsSelectAttackType = isSelectAttackType;
            // 게임 로직이 UI 설정에 영향을 줄 수 있음
            RequiresTarget = isSelectAttackType;
        }
        #endregion

        #region ITargetingCard 구현
        protected virtual void InitializeTargeting()
        {
            // 기본 타겟팅 규칙 설정
            ValidTargets = new List<CardType> {
                CardType.Creature,
                CardType.Master,
                CardType.Monster,
                CardType.EnemyMaster
            };
            RequiresTarget = true;
            CanTargetSelf = false;
            CanTargetEmpty = false;
        }

        public bool IsValidTarget(ICard target)
        {
            if (target == null)
            {
                return CanTargetEmpty;
            }

            // 타겟팅 규칙에 따라 유효성 검사
            return ValidTargets.Contains(target.CardType);
        }

        public void OnTargetSelected(ICard target)
        {
            // 타겟 선택 시 효과 적용
            Debug.Log($"마법 카드 {Id} 타겟 선택: {target.Id}");

            // 타겟에 어빌리티 효과 적용
            // (실제 구현은 카드의 특성에 따라 다름)
        }
        #endregion
        public override void OnPlay()
        {
            // 마법 카드 사용 시 실행되는 코드
            Debug.Log($"마법 카드 {Id} 사용됨");

            // 타겟팅이 필요 없는 마법인 경우 바로 효과 적용
            if (!RequiresTarget)
            {
                ApplyEffect();
            }
        }

        protected virtual void ApplyEffect()
        {
            // 마법 카드의 효과 적용 (자식 클래스에서 구현)
            Debug.Log($"마법 카드 {Id} 효과 적용");
        }
    }
}