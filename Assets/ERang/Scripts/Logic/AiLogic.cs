using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class AiLogic : MonoBehaviour
    {
        public static AiLogic Instance { get; private set; }

        private const int BOARD_CENTER_OFFSET = 3;
        private static readonly System.Random random = new System.Random();

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HandOnAbilityAction(Card card)
        {
        }

        /// <summary>
        /// AiData 에 설정된 어빌리티 적용
        /// </summary>
        public void AiDataAction(AiData aiData, BoardSlot selfSlot)
        {
            Card selfCard = selfSlot.Card;

            if (selfCard == null)
            {
                Debug.LogError($"{Utils.BoardSlotLog(selfSlot)} 장착된 카드 없음 - AiLogic.AiDataAction");
                return;
            }

            // 상대방 슬롯 리스트
            List<BoardSlot> opponentSlots = Board.Instance.GetOpponentSlots(selfSlot);

            // Debug.Log($"{Utils.BoardSlotLog(selfSlot)} AiData({aiData.ai_Id})에 설정된 타겟({aiData.target}) 얻기 시작 - AiLogic.AiDataAction");

            // AiData 에 설정된 타겟 얻기
            List<BoardSlot> aiTargetSlots = TargetLogic.Instance.GetAiTargetSlots(aiData, selfSlot);

            if (aiTargetSlots.Count == 0)
            {
                Debug.LogWarning($"{Utils.BoardSlotLog(selfSlot)} 설정 타겟({aiData.target}) 없음 - AiLogic.AiDataAction");
                return;
            }

            // Debug.Log($"{Utils.BoardSlotLog(selfSlot)} AiData 에 설정된 어빌리티({string.Join(", ", aiData.ability_Ids)}) 타겟({aiData.target}) Slots: <color=yellow>{string.Join(", ", aiTargetSlots.Select(slot => slot.Slot))}</color>번에 적용 - AiLogic.AiDataAction");

            // 어빌리티 적용
            AbilityLogic.Instance.SetBoardSlotAbility(aiData, selfSlot, aiTargetSlots);
        }

        /// <summary>
        /// HandOn 어빌리티를 가진 카드 얻기
        /// </summary>
        public List<(Card card, AiData aiData, List<AbilityData> abilities)> GetHandOnCards(List<Card> handCards)
        {
            List<(Card card, AiData aiData, List<AbilityData> abilities)> handOnCards = new List<(Card, AiData, List<AbilityData>)>();

            foreach (Card handCard in handCards)
            {
                int aiDataId = handCard.GetCardAiDataId();

                AiData handCardAiData = AiData.GetAiData(aiDataId);

                if (handCardAiData == null)
                {
                    Debug.LogWarning($"{Utils.CardLog(handCard)} AiData({aiDataId}) 데이터 없음 - AiLogic.GetHandOnCards");
                    continue;
                }

                List<AbilityData> abilities = new List<AbilityData>();

                foreach (int abilityId in handCardAiData.ability_Ids)
                {
                    AbilityData ability = AbilityData.GetAbilityData(abilityId);

                    if (ability == null)
                    {
                        Debug.LogWarning($"{Utils.CardLog(handCard)} <color=red>어빌리티 데이터 없음</color> - AiLogic.GetHandOnCards");
                        continue;
                    }

                    if (ability.type == AbilityWorkType.OnHand)
                        abilities.Add(ability);
                }

                if (abilities.Count == 0)
                    continue;

                handOnCards.Add((handCard, handCardAiData, abilities));
            }

            Debug.Log($"HandOn 어빌리티를 가진 카드 {handOnCards.Count}장({string.Join(", ", handOnCards.Select(handOnCard => handOnCard.card.id))}) 확인 - AiLogic.GetHandOnCards");

            return handOnCards;
        }
    }
}