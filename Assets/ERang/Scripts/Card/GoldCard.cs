using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class GoldCard : GameCard, IGoldCard
    {
        private int gold;
        public int Gold => gold;

        public GoldCard() : base()
        {
            // 골드 카드는 전투/어빌리티/AI 기능 비활성화
            usesCombat = false;
            usesAbilities = false;
            usesAi = false;

            CardType = CardType.Gold;
            gold = 0;
        }

        public GoldCard(CardData cardData, int gold) : base(cardData)
        {
            // 골드 카드는 전투/어빌리티/AI 기능 비활성화
            usesCombat = false;
            usesAbilities = false;
            usesAi = false;

            this.gold = gold;
        }

        public GoldCard(int gold) : base()
        {
            // 골드 카드는 전투/어빌리티/AI 기능 비활성화
            usesCombat = false;
            usesAbilities = false;
            usesAi = false;
            CardType = CardType.Gold;

            this.gold = gold;
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

        public override void OnPlay()
        {
            // 골드 카드 사용 시 실행되는 코드
            Debug.Log($"골드 카드 {Id} 사용됨, 골드량: {Gold}");
        }
    }
}