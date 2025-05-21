using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 리팩토링된 카드 시스템 테스트 클래스
    /// </summary>
    public class CardSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        public CardData testCardData;
        public MasterData testMasterData;
        
        void Start()
        {
            RunTests();
        }
        
        void RunTests()
        {
            Debug.Log("=== ERang 카드 시스템 리팩토링 테스트 시작 ===");
            
            TestCardCreation();
            TestValueSystem();
            TestAbilitySystem();
            TestCardInteractions();
            
            Debug.Log("=== 테스트 완료 ===");
        }
        
        /// <summary>
        /// 카드 생성 테스트
        /// </summary>
        void TestCardCreation()
        {
            Debug.Log("--- 카드 생성 테스트 ---");
            
            // 크리쳐 카드 생성
            var creatureCard = CardFactory.CreateEmptyCard(CardType.Creature);
            Debug.Log($"크리쳐 카드 생성: {creatureCard.CardType}, UID: {creatureCard.Uid}");
            
            // 마스터 카드 생성
            var masterCard = CardFactory.CreateEmptyCard(CardType.Master);
            Debug.Log($"마스터 카드 생성: {masterCard.CardType}, UID: {masterCard.Uid}");
            
            // 마법 카드 생성
            var magicCard = CardFactory.CreateEmptyCard(CardType.Magic);
            Debug.Log($"마법 카드 생성: {magicCard.CardType}, UID: {magicCard.Uid}");
            
            // 골드 카드 생성
            var goldCard = CardFactory.CreateGoldCard(100);
            Debug.Log($"골드 카드 생성: 골드량 {goldCard.Gold}");
        }
        
        /// <summary>
        /// 값 시스템 테스트
        /// </summary>
        void TestValueSystem()
        {
            Debug.Log("--- 값 시스템 테스트 ---");
            
            var card = CardFactory.CreateEmptyCard(CardType.Creature);
            
            // 값 설정
            card.SetValue(ValueType.Hp, 50);
            card.SetValue(ValueType.Attack, 10);
            card.SetValue(ValueType.Defense, 5);
            
            Debug.Log($"초기 값 - HP: {card.GetValue(ValueType.Hp)}, ATK: {card.GetValue(ValueType.Attack)}, DEF: {card.GetValue(ValueType.Defense)}");
            
            // 값 수정
            card.ModifyValue(ValueType.Hp, -20);
            card.ModifyValue(ValueType.Attack, 5);
            
            Debug.Log($"수정 후 - HP: {card.GetValue(ValueType.Hp)}, ATK: {card.GetValue(ValueType.Attack)}, DEF: {card.GetValue(ValueType.Defense)}");
        }
        
        /// <summary>
        /// 어빌리티 시스템 테스트
        /// </summary>
        void TestAbilitySystem()
        {
            Debug.Log("--- 어빌리티 시스템 테스트 ---");
            
            var card = CardFactory.CreateEmptyCard(CardType.Creature);
            
            // 어빌리티 생성 (임시로 null 체크하여 진행)
            if (card.AbilitySystem != null)
            {
                Debug.Log($"어빌리티 시스템 활성화됨. 현재 어빌리티 수: {card.AbilitySystem.CardAbilities.Count}");
                
                // 버프/디버프 카운트 테스트
                int buffCount = card.GetBuffCount();
                int debuffCount = card.GetDeBuffCount();
                
                Debug.Log($"버프 수: {buffCount}, 디버프 수: {debuffCount}");
            }
            else
            {
                Debug.Log("어빌리티 시스템이 비활성화된 카드입니다.");
            }
        }
        
        /// <summary>
        /// 카드 상호작용 테스트
        /// </summary>
        void TestCardInteractions()
        {
            Debug.Log("--- 카드 상호작용 테스트 ---");
            
            var attackerCard = CardFactory.CreateEmptyCard(CardType.Creature);
            var defenderCard = CardFactory.CreateEmptyCard(CardType.Creature);
            
            // 초기 값 설정
            attackerCard.SetValue(ValueType.Attack, 15);
            defenderCard.SetValue(ValueType.Hp, 30);
            defenderCard.SetValue(ValueType.Defense, 5);
            
            Debug.Log($"공격 전 - 공격자 ATK: {attackerCard.GetValue(ValueType.Attack)}");
            Debug.Log($"공격 전 - 방어자 HP: {defenderCard.GetValue(ValueType.Hp)}, DEF: {defenderCard.GetValue(ValueType.Defense)}");
            
            // 공격 시뮬레이션
            int damage = attackerCard.GetValue(ValueType.Attack);
            defenderCard.TakeDamage(damage);
            
            Debug.Log($"공격 후 - 방어자 HP: {defenderCard.GetValue(ValueType.Hp)}, DEF: {defenderCard.GetValue(ValueType.Defense)}");
            
            // 힐링 테스트
            defenderCard.RestoreHealth(10);
            Debug.Log($"힐링 후 - 방어자 HP: {defenderCard.GetValue(ValueType.Hp)}");
        }
        
        /// <summary>
        /// 기능 비활성화 테스트
        /// </summary>
        void TestFeatureToggling()
        {
            Debug.Log("--- 기능 비활성화 테스트 ---");
            
            var goldCard = CardFactory.CreateGoldCard(50);
            
            // 골드 카드는 전투/어빌리티 기능이 비활성화되어야 함
            goldCard.TakeDamage(100); // 데미지를 받아도 아무 일이 일어나지 않아야 함
            Debug.Log($"골드 카드 데미지 후 HP: {goldCard.GetValue(ValueType.Hp)} (변화 없어야 함)");
            Debug.Log($"골드 카드 골드량: {goldCard.Gold}");
        }
    }
}