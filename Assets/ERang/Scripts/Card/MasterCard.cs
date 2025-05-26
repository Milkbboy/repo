using UnityEngine;
using ERang.Data;
using System.Collections.Generic;

namespace ERang
{
    public class MasterCard : GameCard, IGoldCard
    {
        private MasterData masterData;
        private int gold;
        private int satiety;
        private int maxSatiety;
        private int creatureSlots;
        private List<int> cardIds = new();
        private MasterType masterType;
        private int manaPerTurn;

        public MasterData MasterData => masterData;
        public MasterType MasterType => masterType;
        public int Satiety { get => satiety; private set => satiety = value; }
        public int MaxSatiety => maxSatiety;
        public int CreatureSlotCount => creatureSlots;
        public List<int> CardIds { get => cardIds; private set => cardIds = value; }
        public int ManaPerTurn { get => manaPerTurn; private set => manaPerTurn = value; }
        public int Gold => gold;

        public MasterCard() : base()
        {
            // 마스터 카드는 모든 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesAi = true;
            CardType = CardType.Master;
        }

        public MasterCard(CardData cardData, MasterData masterData = null) : base(cardData)
        {
            // 마스터 카드는 모든 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesAi = true;

            if (masterData != null)
            {
                this.masterData = masterData;
                InitializeFromMasterData();
            }
        }

        public MasterCard(MasterData masterData) : base()
        {
            // 마스터 카드는 모든 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesAi = true;

            Id = masterData.master_Id;
            Name = masterData.nameDesc;
            CardType = CardType.Master;
            CardImage = masterData.masterTexture;

            this.masterData = masterData;

            InitializeFromMasterData();
        }

        #region IGoldCard 구현

        public void SetGold(int amount)
        {
            gold = amount;
        }

        public void AddGold(int amount)
        {
            int beforeGold = gold;
            gold += amount;
            GameLogger.Log(LogCategory.DEBUG, $"<color=#257dca>Add Gold({amount}): {beforeGold} -> {gold}</color>");
        }

        public void DeductGold(int amount)
        {
            int beforeGold = gold;
            gold = Mathf.Max(0, gold - amount);
            GameLogger.Log(LogCategory.DEBUG, $"<color=#257dca>Deduct Gold({amount}): {beforeGold} -> {gold}</color>");
        }

        #endregion

        public void SetCardIds(List<int> cardIds)
        {
            this.cardIds = cardIds;
        }

        public void SetSatiety(int satiety)
        {
            this.satiety = satiety;
        }

        protected virtual void InitializeFromMasterData()
        {
            // 마스터 데이터 기반으로 추가 초기화
            if (masterData != null)
            {
                masterType = masterData.masterType;

                // 마스터의 최대 체력/마나 설정
                State.SetMaxHp(masterData.maxHp);
                State.SetMaxMana(masterData.maxMana);
                // State 에서 hp 를 설정할때 maxHp 를 참고하기 때문에 maxHp 먼저 설정
                SetHp(masterData.maxHp);
                SetMana(masterData.startMana);

                // 추가 속성 초기화
                gold = 1000; // 임시
                creatureSlots = masterData.creatureSlots;
                satiety = masterData.satietyGauge;
                maxSatiety = masterData.maxSatietyGauge;
                cardIds = masterData.startCardIds;
            }
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();

            // 턴 시작 시 마나 회복
            int manaGain = masterData != null ? masterData.manaPerTurn : 1;
            State.SetMana(manaGain);

            GameLogger.Log(LogCategory.DEBUG, $"마스터 {Name}({Id}) 턴 시작, 마나 {manaGain} 회복");
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
        }

        public bool CanPlayCard(GameCard card)
        {
            // 카드를 플레이할 수 있는지 마나 체크
            int currentMana = State.Mana;
            return currentMana >= card.State.Mana;
        }

        public void ConsumeCardMana(GameCard card)
        {
            // 카드 플레이 시 마나 소모
            State.SetMana(State.Mana - card.State.Mana);
        }

        public void IncreaseSatiety(int amount)
        {
            int beforeSatiety = satiety;
            satiety += amount;

            if (satiety > maxSatiety)
                satiety = maxSatiety;

            GameLogger.Log(LogCategory.DEBUG, $"<color=#257dca>만복감 증가({amount}): {beforeSatiety} -> {satiety}</color>");
        }

        public void DecreaseSatiety(int amount)
        {
            int beforeSatiety = satiety;
            satiety -= amount;

            if (satiety < 0)
                satiety = 0;

            GameLogger.Log(LogCategory.DEBUG, $"<color=#257dca>Decrease Satiety({amount}): {beforeSatiety} -> {satiety}</color>");
        }

        public void RecoverHp(int amount)
        {
            State.SetHp(Mathf.Clamp(State.Hp + amount, 0, State.MaxHp));
        }
    }
}