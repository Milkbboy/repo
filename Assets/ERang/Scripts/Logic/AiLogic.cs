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