using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    // 크리쳐 카드
    public class CreatureCard : BaseCard, IDamageable, IDefensible, IAttackable
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Mana { get; set; }

        public CreatureCard(CardData cardData) : base(cardData)
        {
            Hp = cardData.hp;
            MaxHp = cardData.hp;
            Atk = cardData.atk;
            Def = cardData.def;
            Mana = cardData.costMana;
        }

        public CreatureCard(Master master) : base(master.MasterId, CardType.Master, 0, master.CardImage)
        {
            Hp = master.Hp;
            MaxHp = master.MaxHp;
            Def = master.Def;
            Mana = master.Mana;
        }

        public void TakeDamage(int amount)
        {
            Hp -= amount;
        }

        public void RestoreHealth(int amount)
        {
            Hp += amount;
        }

        public void IncreaseDefense(int amount)
        {
            Def += amount;
        }

        public void DecreaseDefense(int amount)
        {
            Def -= amount;
        }

        public void IncreaseAttack(int amount)
        {
            Atk += amount;
        }

        public void DecreaseAttack(int amount)
        {
            Atk -= amount;
        }

        public void SetAttack(int amount)
        {
            Atk = amount;
        }
    }
}