using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Master
    {
        public static Master Instance { get; private set; }

        public int MasterId => masterId;
        public int Hp { get { return hp; } set { hp = value; } }
        public int MaxHp { get { return maxHp; } set { maxHp = value; } }
        public int Mana => mana;
        public int MaxMana { get { return maxMana; } set { maxMana = value; } }
        public int RechargeMana { get { return rechargeMana; } set { rechargeMana = value; } }
        public int Atk { get { return atk; } set { atk = value; } }
        public int Def { get { return def; } set { def = value; } }
        public int Gold { get { return gold; } set { gold = value; } }
        public List<int> StartCardIds => startCardIds;

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

        public void ChargeMana()
        {
            int beforeMana = mana;

            mana += rechargeMana;

            if (mana > MaxMana)
                mana = MaxMana;

            Debug.Log($"<color=#257dca>Charge Mana({rechargeMana}): {beforeMana} -> {mana}</color>");
        }

        public void ResetMana()
        {
            int beforeMana = mana;

            mana = 0;

            Debug.Log($"<color=#257dca>Reset Mana: {beforeMana} -> {mana}</color>");
        }

        public void AddMana(int value)
        {
            int beforeMana = mana;

            mana += value;

            if (mana < 0)
                mana = 0;

            if (mana > MaxMana)
                mana = MaxMana;

            // Debug.Log($"<color=#257dca>Add Mana({value}): {beforeMana} -> {mana}</color>");
        }

        public void AddGold(int gold)
        {
            int beforeGold = this.gold;

            this.gold += gold;

            Debug.Log($"<color=#257dca>Add Gold({gold}): {beforeGold} -> {this.gold}</color>");
        }
    }
}