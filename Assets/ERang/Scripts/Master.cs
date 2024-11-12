using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Master
    {
        public static Master Instance { get; private set; }

        public int MasterId => masterId;
        public MasterType MasterType => masterType;
        public int Hp { get => hp; set => hp = value; }
        public int MaxHp { get => maxHp; set => maxHp = value; }
        public int Mana => mana;
        public int MaxMana { get => maxMana; set => maxMana = value; }
        public int RechargeMana { get => rechargeMana; set => rechargeMana = value; }
        public int Atk { get => atk; set => atk = value; }
        public int Def { get => def; set => def = value; }
        public int Gold { get => gold; set => gold = value; }
        public int CreatureSlotCount => creatureSlots;
        public int Satiety { get => satiety; set => satiety = value; }
        public int MaxSatiety => maxSatiety;
        public List<int> StartCardIds => startCardIds;
        public Texture2D CardImage => masterTexture;

        private readonly int masterId;
        private readonly MasterType masterType;
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
        private int satiety;
        private int maxSatiety;
        private Texture2D masterTexture;

        public Master(MasterData masterData)
        {
            Instance = this;

            masterId = masterData.master_Id;
            masterType = masterData.masterType;
            maxHp = hp = masterData.hp;
            mana = masterData.startMana;
            maxMana = masterData.maxMana;
            rechargeMana = masterData.rechargeMana;
            atk = masterData.atk;
            def = masterData.def;
            gold = 1000; // 임시
            creatureSlots = masterData.creatureSlots;
            satiety = masterData.satietyGauge;
            maxSatiety = masterData.maxSatietyGauge;
            startCardIds = masterData.startCardIds;
            masterTexture = masterData.masterTexture;
        }

        public void AddGold(int gold)
        {
            int beforeGold = this.gold;

            this.gold += gold;

            Debug.Log($"<color=#257dca>Add Gold({gold}): {beforeGold} -> {this.gold}</color>");
        }

        public void IncreaseSatiety(int satiety)
        {
            int beforeSatiety = this.satiety;

            this.satiety += satiety;

            if (this.satiety > maxSatiety)
                this.satiety = maxSatiety;

            Debug.Log($"<color=#257dca>만복감 증가({satiety}): {beforeSatiety} -> {this.satiety}</color>");
        }

        public void DecreaseSatiety(int satiety)
        {
            int beforeSatiety = this.satiety;

            this.satiety -= satiety;

            if (this.satiety < 0)
                this.satiety = 0;

            Debug.Log($"<color=#257dca>Decrease Satiety({satiety}): {beforeSatiety} -> {this.satiety}</color>");
        }
    }
}