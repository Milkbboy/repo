using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class CreatureCard : GameCard
    {
        public CreatureCard() : base()
        {
            // 모든 기능 활성화
            usesCombat = true;
            usesAbilities = true;
            usesAi = true;
            CardType = CardType.Creature;
        }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            // 모든 기능 활성화
            usesCombat = true;
            usesAbilities = true;
            usesAi = true;
        }

        public override void OnPlay()
        {
            // 크리쳐 카드가 필드에 배치될 때 실행되는 코드
            Debug.Log($"크리쳐 카드 {Id} 배치됨");
        }

        public override void OnTurnStart()
        {
            // 턴 시작 시 실행
            base.OnTurnStart();

            // 크리쳐 카드의 턴 시작 시 추가 로직 구현
        }

        public override void OnTurnEnd()
        {
            // 턴 종료 시 실행
            base.OnTurnEnd();

            // 크리쳐 카드의 턴 종료 시 추가 로직 구현
        }
    }
}