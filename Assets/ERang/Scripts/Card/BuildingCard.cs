using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class BuildingCard : GameCard, IGoldCard
    {
        private int durability;
        private int gold; // 건물 건설 비용

        public int Durability
        {
            get => durability;
            set => durability = Mathf.Max(0, value);
        }

        public int Gold => gold;

        public BuildingCard() : base()
        {
            // 건물 카드는 전투 비활성화
            usesCombat = false;
            usesAbilities = true;
            usesAi = true;
            CardType = CardType.Building;

            durability = 1;
        }

        public BuildingCard(CardData cardData) : base(cardData)
        {
            // 건물 카드는 전투 비활성화
            usesCombat = false;
            usesAbilities = true;
            usesAi = true;

            durability = cardData.hp > 0 ? cardData.hp : 1;
            // 건물 건설 비용
            gold = cardData.costGold > 0 ? cardData.costGold : 0;
        }

        #region IGoldCard 구현

        public void SetGold(int amount)
        {
            gold = amount;
        }

        public void AddGold(int amount)
        {
            gold += amount;
        }

        public void DeductGold(int amount)
        {
            gold = Mathf.Max(0, gold - amount);
        }

        #endregion

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