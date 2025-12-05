using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using ERang;

namespace ERang.Tests
{
    public class CardStatTests
    {
        private CardAbilitySystem abilitySystem;
        private CardStat cardStat;

        [SetUp]
        public void Setup()
        {
            abilitySystem = new CardAbilitySystem();
            // HP: 100, Def: 10, Mana: 50, Atk: 20
            cardStat = new CardStat(100, 10, 50, 20, abilitySystem, maxHp: 100, maxMana: 50);
        }

        [Test]
        public void Test_BaseStat_Calculation_NoAbilities()
        {
            Assert.AreEqual(100, cardStat.Hp);
            Assert.AreEqual(10, cardStat.Def);
            Assert.AreEqual(50, cardStat.Mana);
            Assert.AreEqual(20, cardStat.Atk);
        }

        [Test]
        public void Test_Stat_Limits()
        {
            cardStat.RestoreHealth(50);
            Assert.AreEqual(100, cardStat.Hp);

            cardStat.IncreaseMana(100);
            Assert.AreEqual(50, cardStat.Mana);
        }
        
        [Test]
        public void Test_ArmorBreak_Priority()
        {
            // 1. 초기 방어력 10 확인
            Assert.AreEqual(10, cardStat.Def);

            // 2. DefUp 어빌리티 (+5) 추가
            var defUp = new CardAbility 
            { 
                abilityId = 1, 
                abilityType = AbilityType.DefUp, 
                abilityValue = 5,
                duration = 2
            };
            abilitySystem.AddCardAbility(defUp, 1, AbilityWhereFrom.None);

            // 예상 방어력: 10 + 5 = 15
            Assert.AreEqual(15, cardStat.Def);

            // 3. ArmorBreak 어빌리티 추가
            var armorBreak = new CardAbility 
            { 
                abilityId = 2, 
                abilityType = AbilityType.ArmorBreak, 
                duration = 1
            };
            abilitySystem.AddCardAbility(armorBreak, 1, AbilityWhereFrom.None);

            // 4. 최종 방어력이 0인지 확인 (ArmorBreak 우선순위)
            Assert.AreEqual(0, cardStat.Def);
        }

        [Test]
        public void Test_Buff_Stacking_SameAbilityId()
        {
            Assert.AreEqual(20, cardStat.Atk);

            // 1. AtkUp (ID: 10, Value: 5) 추가
            var atkUp1 = new CardAbility 
            { 
                abilityId = 10, 
                abilityType = AbilityType.AtkUp, 
                abilityValue = 5,
                duration = 2
            };
            abilitySystem.AddCardAbility(atkUp1, 1, AbilityWhereFrom.None); 
            
            // Atk: 20 + 5 = 25
            Assert.AreEqual(25, cardStat.Atk);

            // 2. AtkUp (ID: 10, Value: 3) 추가 (같은 ID)
            // 구현상 같은 ID는 리스트에 추가되지 않고 내부 Queue(abilityItems)에 쌓임 -> CardAbilitySystem.AddCardAbility 로직에 따름
            // CardAbilitySystem.AddCardAbility를 보면 found가 있으면 Insert/Add 안하고 AddAbilityItem만 호출함
            
            var atkUp2 = new CardAbility 
            { 
                abilityId = 10, 
                abilityType = AbilityType.AtkUp, 
                abilityValue = 3,
                duration = 2
            };
            abilitySystem.AddCardAbility(atkUp2, 2, AbilityWhereFrom.None);

            // CardStat 로직: latestAbility = group.OrderByDescending(createdTurn).First()
            // Turn 2에 생성된 Value 3짜리가 최신이므로 이것만 적용되어야 함
            // Atk: 20 + 3 = 23 (25가 아님)
            Assert.AreEqual(23, cardStat.Atk);
        }

        [Test]
        public void Test_Buff_Stacking_DifferentAbilityId()
        {
            Assert.AreEqual(20, cardStat.Atk);

            // 1. AtkUp (ID: 10, Value: 5)
            var atkUp1 = new CardAbility 
            { 
                abilityId = 10, 
                abilityType = AbilityType.AtkUp, 
                abilityValue = 5,
                duration = 2
            };
            abilitySystem.AddCardAbility(atkUp1, 1, AbilityWhereFrom.None);

            // 2. AtkUp (ID: 11, Value: 3) (다른 ID)
            var atkUp2 = new CardAbility 
            { 
                abilityId = 11, 
                abilityType = AbilityType.AtkUp, 
                abilityValue = 3,
                duration = 2
            };
            abilitySystem.AddCardAbility(atkUp2, 1, AbilityWhereFrom.None);

            // 서로 다른 ID는 합산 되어야 함
            // Atk: 20 + 5 + 3 = 28
            Assert.AreEqual(28, cardStat.Atk);
        }
    }
}
