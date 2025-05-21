using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class MasterCard : GameCard
    {
        private MasterData masterData;
        
        public MasterData MasterData => masterData;
        
        public MasterCard() : base()
        {
            // 마스터 카드는 모든 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = true;
            usesAi = true;
            CardType = CardType.Master;
        }
        
        public MasterCard(CardData cardData, MasterData masterData = null) : base(cardData)
        {
            // 마스터 카드는 모든 기능 사용
            usesCombat = true;
            usesAbilities = true;
            usesValues = true;
            usesAi = true;
            
            if (masterData != null)
            {
                this.masterData = masterData;
                InitializeFromMasterData();
            }
        }
        
        protected virtual void InitializeFromMasterData()
        {
            // 마스터 데이터 기반으로 추가 초기화
            if (masterData != null)
            {
                // 마스터의 최대 체력/마나 설정
                SetValue(ValueType.MaxHp, masterData.maxHp);
                SetValue(ValueType.Hp, masterData.maxHp);
                SetValue(ValueType.MaxMana, masterData.maxMana);
                SetValue(ValueType.Mana, masterData.startMana);
                
                // 다른 마스터 고유 속성 설정
            }
        }
        
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            
            // 턴 시작 시 마나 회복
            int manaGain = masterData != null ? masterData.manaPerTurn : 1;
            ModifyValue(ValueType.Mana, manaGain);
            
            Debug.Log($"마스터 {Id} 턴 시작, 마나 {manaGain} 회복");
        }
        
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            
            // 턴 종료 시 추가 로직
        }
        
        public bool CanPlayCard(GameCard card)
        {
            // 카드를 플레이할 수 있는지 마나 체크
            int currentMana = GetValue(ValueType.Mana);
            
            if (card is IValueCard valueCard)
            {
                int cardCost = valueCard.GetValue(ValueType.Mana);
                return currentMana >= cardCost;
            }
            else if (card.State != null)
            {
                return currentMana >= card.State.Mana;
            }
            
            return true;
        }
        
        public void ConsumeCardMana(GameCard card)
        {
            // 카드 플레이 시 마나 소모
            if (card is IValueCard valueCard)
            {
                int cardCost = valueCard.GetValue(ValueType.Mana);
                ModifyValue(ValueType.Mana, -cardCost);
            }
            else if (card.State != null)
            {
                ModifyValue(ValueType.Mana, -card.State.Mana);
            }
        }
    }
}