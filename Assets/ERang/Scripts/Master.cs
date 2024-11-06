using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Master
    {
        public static Master Instance { get; private set; }

        public int MasterId => masterId;
        public int Hp { get => hp; set => hp = value; }
        public int MaxHp { get => maxHp; set => maxHp = value; }
        public int Mana => mana;
        public int MaxMana { get => maxMana; set => maxMana = value; }
        public int RechargeMana { get => rechargeMana; set => rechargeMana = value; }
        public int Atk { get => atk; set => atk = value; }
        public int Def { get => def; set => def = value; }
        public int Gold { get => gold; set => gold = value; }
        public int CreatureSlotCount => creatureSlots;
        public List<int> StartCardIds => startCardIds;
        public Texture2D CardImage => masterTexture;

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
        private int creatureSlots;
        private Texture2D masterTexture;

        public Master(MasterData masterData)
        {
            Instance = this;

            masterId = masterData.master_Id;
            maxHp = hp = masterData.hp;
            mana = masterData.startMana;
            maxMana = masterData.maxMana;
            rechargeMana = masterData.rechargeMana;
            atk = masterData.atk;
            def = masterData.def;
            gold = 1000; // 임시
            creatureSlots = masterData.creatureSlots;
            startCardIds = masterData.startCardIds;
            masterTexture = masterData.masterTexture;
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