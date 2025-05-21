using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class MagicCard : GameCard, ITargetingCard
    {
        public List<CardType> ValidTargets { get; protected set; }
        public bool RequiresTarget { get; protected set; }
        public bool CanTargetSelf { get; protected set; }
        public bool CanTargetEmpty { get; protected set; }
        
        public MagicCard() : base()
        {
            // 마법 카드는 전투 기능과 어빌리티 기능만 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = false;
            usesAi = true;
            CardType = CardType.Magic;
            
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