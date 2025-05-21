using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class BuildingCard : GameCard
    {
        private int durability;
        
        public int Durability
        {
            get => durability;
            set => durability = Mathf.Max(0, value);
        }
        
        public BuildingCard() : base()
        {
            // 건물 카드는 어빌리티와 값 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = true;
            usesAi = true;
            CardType = CardType.Building;
            
            durability = 1;
        }
        
        public BuildingCard(CardData cardData) : base(cardData)
        {
            // 건물 카드는 어빌리티와 값 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = true;
            usesAi = true;
            
            durability = cardData.hp > 0 ? cardData.hp : 1;
        }
        
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            
            // 건물은 직접 데미지를 받으면 내구도도 감소
            DecreaseDurability(1);
        }
        
        public void DecreaseDurability(int amount)
        {
            Durability -= amount;
            
            if (Durability <= 0)
            {
                OnDestroyed();
            }
        }
        
        protected virtual void OnDestroyed()
        {
            // 건물이 파괴될 때 호출
            Debug.Log($"건물 카드 {Id} 파괴됨");
            
            // 여기에 건물 파괴 효과 구현
        }
        
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            
            // 턴 시작 시 건물 효과 적용
        }
        
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            
            // 턴 종료 시 건물 효과 적용
        }
    }
}