using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class ActionProcessor : MonoBehaviour
    {
        public static ActionProcessor Instance { get; private set; }

        private Player player;
        private MasterCard masterCard;
        private DeckManager deckManager;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void Initialize(Player player, MasterCard masterCard, DeckManager deckManager)
        {
            this.player = player;
            this.masterCard = masterCard;
            this.deckManager = deckManager;
        }

        public bool UseHandCard(HCard hCard, BSlot targetSlot)
        {
            if (CanUseHandCard(hCard.Card.Uid) == false)
                return false;

            if (hCard.Card.CardType == CardType.Creature || hCard.Card.CardType == CardType.Building)
            {
                EquipCardToSlot(hCard, targetSlot);
                return true;
            }

            if (hCard.Card.CardType == CardType.Magic)
            {
                StartCoroutine(UseHandCard(hCard.Card.Uid, targetSlot));
                return true;
            }

            return false;
        }

        public void EquipCardToSlot(HCard hCard, BSlot boardSlot)
        {
            if (boardSlot.SlotCardType != hCard.Card.CardType)
            {
                hCard.GoBackPosition();
                Debug.LogError($"카드 타입이 일치하지 않아 장착 실패. {boardSlot.SlotCardType}에 {hCard.Card.CardType} 장착 시도");
                return;
            }

            // 핸드 카드 => 보드 카드 이동
            deckManager.HandCardToBoard(hCard.Card);

            // 보드 슬롯에 카드 장착
            boardSlot.EquipCard(hCard.Card);

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(player, hCard.Card);

            Debug.Log($"{boardSlot.ToSlotLogInfo()} 에 {hCard.Card.ToCardLogInfo()} 장착");
        }

        public IEnumerator UseHandCard(string cardUid, BSlot targetSlot)
        {
            BaseCard card = deckManager.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 카드({cardUid}) 없음");
                yield break;
            }

            // 카드 비용 소모
            BoardSystem.Instance.CardCost(player, card);

            FlatLogger.LogCard($"{card.ToCardLogInfo()} 사용");

            foreach (int aiGroupId in card.AiGroupIds)
            {
                FlatLogger.LogAiGroup(aiGroupId);

                int aiDataId = AiLogic.Instance.GetCardAiDataId(card, aiGroupId);
                AiData aiData = AiData.GetAiData(aiDataId);

                if (aiData == null)
                {
                    Debug.LogError($"{card.ToCardLogInfo()} 카드 AiData({aiDataId}) 없음");
                    continue;
                }

                FlatLogger.LogAiData(aiData);

                // 타겟 설정 카드 확인
                bool isSelectAttackType = Constants.SelectAttackTypes.Contains(aiData.attackType);

                // 마법 사용 주체는 마스터 슬롯
                BSlot selfSlot = BoardSystem.Instance.GetMasterSlot();

                // 타겟팅이면 nearastSlot 을 대상으로 설정
                List<BSlot> targetSlots = (aiData.target == AiDataTarget.SelectEnemy) ?
                    new List<BSlot> { targetSlot } :
                    TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot, "HandCardUse");

                // Debug.Log($"{card.LogText} 사용. isSelectAttackType: {isSelectAttackType}, aiDataId: {aiData.ai_Id}, aiData.target: {aiData.target}, targetSlot: {targetSlot?.SlotNum ?? -1}, tagetSlots: {string.Join(", ", targetSlots.Select(slot => slot.SlotNum))}");

                // 대상 선택 사용 카드
                if (isSelectAttackType)
                {
                    if (targetSlot == null)
                    {
                        Debug.LogError($"{card.ToCardLogInfo()} 마법 대상이 없어서 카드 사용 실패");
                        continue;
                    }

                    if (targetSlots.Contains(targetSlot) == false)
                    {
                        Debug.LogError($"{card.ToCardLogInfo()} 대상 슬롯이 아닌 슬롯에 카드 사용 실패");
                        continue;
                    }
                }

                List<AbilityData> abilityDatas = AiLogic.Instance.GetAbilityDatas(aiData.ability_Ids);

                foreach (AbilityData abilityData in abilityDatas)
                    yield return StartCoroutine(AbilityLogic.Instance.AbilityProcess(aiData, abilityData, selfSlot, targetSlots, AbilityWhereFrom.HandUse));
            }

            // 마스터 핸드 카드 제거 먼저 하고 어빌리티 발동
            deckManager.RemoveHandCard(cardUid);
        }

        public bool CanUseHandCard(string cardUid)
        {
            BaseCard card = deckManager.FindHandCard(cardUid);

            if (card == null)
            {
                Debug.LogError($"핸드에 카드({cardUid}) 없음");
                return false;
            }

            if (card.InUse == false)
            {
                ToastNotification.Show($"card({card.Id}) is not in use");
                Debug.LogWarning($"사용할 수 없는 카드({card.Id}) InUse: false 설정");
                return false;
            }

            int requiredMana = 0;

            if (card is CreatureCard creatureCard)
                requiredMana = creatureCard.Mana;

            if (card is MagicCard magicCard)
                requiredMana = magicCard.Mana;

            // 필요 마나 확인
            if (masterCard.Mana < requiredMana)
            {
                ToastNotification.Show($"mana({masterCard.Mana}) is not enough");
                Debug.LogWarning($"핸드 카드({card.Id}) 마나 부족으로 사용 불가능({masterCard.Mana} < {requiredMana})");
                return false;
            }

            // 필요 골드 확인
            if (card is BuildingCard buildingCard && player.Gold < buildingCard.Gold)
            {
                ToastNotification.Show($"gold({player.Gold}) is not enough");
                Debug.LogWarning($"핸드 카드({buildingCard.Id}) 골드 부족으로 사용 불가능({player.Gold} < {buildingCard.Gold})");
                return false;
            }

            // Debug.Log($"핸드 카드({card.Id}) 사용 가능");
            return true;
        }

        public IEnumerator RemoveBoardCard(int slotNum)
        {
            BSlot boardSlot = BoardSystem.Instance.GetBoardSlot(slotNum);

            if (boardSlot == null)
            {
                Debug.LogError($"슬롯({slotNum}). 보드 슬롯 없음");
                yield break;
            }

            Debug.Log($"{boardSlot.ToSlotLogInfo()} 카드 제거");

            int monsterCount = BoardSystem.Instance.GetRightBoardSlots().Count(slot => slot.Card != null);

            // 배틀 종료 체크는 BattleLogic에서 처리하도록 이벤트 발생
            if (monsterCount == 0 || boardSlot.SlotNum == 0)
            {
                // 배틀 종료 이벤트 발생 (추후 이벤트 시스템 도입 시)
                yield return StartCoroutine(BattleLogic.Instance.BattleEnd(monsterCount == 0));
            }
        }
    }
}