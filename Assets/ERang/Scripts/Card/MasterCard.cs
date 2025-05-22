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

        public MasterCard(Master master) : base()
        {
            Id = master.MasterId;
            CardType = CardType.Master;
            CardImage = master.CardImage;

            State.SetHp(master.Hp);
            State.SetMana(master.Mana);
            State.SetMaxHp(master.MaxHp);
            State.SetMaxMana(master.MaxMana);
        }

        protected virtual void InitializeFromMasterData()
        {
            // 마스터 데이터 기반으로 추가 초기화
            if (masterData != null)
            {
                // 마스터의 최대 체력/마나 설정
                SetHp(masterData.maxHp);
                SetMana(masterData.startMana);
                State.SetMaxHp(masterData.maxHp);
                State.SetMaxMana(masterData.maxMana);
            }
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();

            // 턴 시작 시 마나 회복
            int manaGain = masterData != null ? masterData.manaPerTurn : 1;
            State.SetMana(manaGain);

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
            int currentMana = State.Mana;

            return currentMana >= card.State.Mana;
        }

        public void ConsumeCardMana(GameCard card)
        {
            // 카드 플레이 시 마나 소모
            State.SetMana(State.Mana - card.State.Mana);
        }
    }
}