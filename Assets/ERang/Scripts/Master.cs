using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    public class Master
    {
        public static Master Instance { get; private set; }

        public int MasterId { get { return masterId; } }
        public int Hp { get { return hp; } set { hp = value; } }
        public int MaxHp { get { return maxHp; } set { maxHp = value; } }
        public int Mana { get { return mana; } set { mana = value; } }
        public int MaxMana { get { return maxMana; } set { maxMana = value; } }
        public int RechargeMana { get { return rechargeMana; } set { rechargeMana = value; } }
        public int Atk { get { return atk; } set { atk = value; } }
        public int Def { get { return def; } set { def = value; } }
        public int Gold { get { return gold; } set { gold = value; } }
        public List<int> StartCardIds { get { return startCardIds; } }

        private readonly int masterId;
        private readonly List<int> startCardIds = new();

        private int hp;
        private int maxHp;
        private int mana;
        private int maxMana;
        private int rechargeMana;
        private int atk;
        private int def;
        private int gold;

        public Master(MasterData masterData)
        {
            Instance = this;

            masterId = masterData.master_Id;
            maxHp = hp = masterData.hp;
            mana = 0;
            maxMana = masterData.maxMana;
            rechargeMana = masterData.rechargeMana;
            atk = masterData.atk;
            def = masterData.def;
            gold = 1000; // 임시
            startCardIds = masterData.startCardIds;
        }

        public void chargeMana()
        {
            mana += rechargeMana;

            if (mana > MaxMana)
                mana = MaxMana;
        }

        public void resetMana()
        {
            mana = 0;
        }

        public void IncreaseMana(int value)
        {
            mana += value;
        }

        public void DecreaseMana(int value)
        {
            mana -= value;

            if (mana < 0)
                mana = 0;
        }

        public void AddGold(int gold)
        {
            this.gold += gold;
        }
    }
}