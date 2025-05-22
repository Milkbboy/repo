using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class GoldCard : GameCard
    {
        public GoldCard() : base()
        {
            // 골드 카드는 값 관리 기능만 사용
            usesCombat = false;
            usesAbilities = false;
            usesValues = true;
            usesAi = false;
            CardType = CardType.Gold;
            Gold = 0;
        }

        public GoldCard(CardData cardData, int gold) : base(cardData)
        {
            // 골드 카드는 값 관리 기능만 사용
            usesCombat = false;
            usesAbilities = false;
            usesValues = true;
            usesAi = false;

            Gold = gold;
        }

        public GoldCard(int gold) : base()
        {
            // 골드 카드는 값 관리 기능만 사용
            usesCombat = false;
            usesAbilities = false;
            usesValues = true;
            usesAi = false;
            CardType = CardType.Gold;

            Gold = gold;
        }

        public void SetGold(int amount)
        {
            Gold = amount;
        }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public override void OnPlay()
        {
            // 골드 카드 사용 시 실행되는 코드
            Debug.Log($"골드 카드 {Id} 사용됨, 골드량: {Gold}");
        }
    }
}