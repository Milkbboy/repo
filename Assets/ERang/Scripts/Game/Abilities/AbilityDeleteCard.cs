using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ERang
{
    public class AbilityDeleteCard : BaseAbility
    {
        public override AbilityType AbilityType => AbilityType.DeleteCard;

        public override IEnumerator ApplySingle(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, true));
        }

        public override IEnumerator Release(CardAbility cardAbility, BoardSlot selfSlot, BoardSlot targetSlot)
        {
            yield return StartCoroutine(Apply(cardAbility, targetSlot, false));
        }

        /// <summary>
        /// value2 의 cardId 에 해당하는 카드를 준비덱, 핸드덱, 그레이브덱에서 value 만큼 삭제
        /// - 카드를 삭제할때 마다 카드를 삭제할 덱은 랜덤으로 결정
        /// </summary>
        /// <param name="cardAbility"></param>
        /// <param name="targetSlot"></param>
        /// <param name="isDefUp"></param>
        /// <returns></returns>
        private IEnumerator Apply(CardAbility cardAbility, BoardSlot targetSlot, bool isDefUp)
        {
            if (!ValidateTargetSlot(targetSlot, "DeleteCard"))
                yield break;

            BaseCard targetCard = targetSlot.Card;
            int beforeDef = targetCard.Def;
            int deleteCount = cardAbility.abilityValue;
            int deleteCardId = cardAbility.abilityValue2;

            if (deleteCount <= 0)
            {
                LogAbility($"삭제할 카드 개수 오류: {deleteCount}", LogType.Error);
                yield break;
            }

            if (deleteCardId <= 0)
            {
                LogAbility($"삭제 대상 카드 id 오류: {deleteCardId}", LogType.Error);
            }

            DeckKind[] deckKinds = (DeckKind[])System.Enum.GetValues(typeof(DeckKind));
            DeckData deckData = DeckManager.Instance.Data;

            for (int i = 0; i < deleteCount; ++i)
            {
                // 1단계: deleteCardId 가 있는 덱들과 해당 카드들 찾기
                var availableDecks = new List<(DeckKind deckKind, BaseCard card)>();

                // 각 덱에서 카드 ID 를 가진 카드 찾기
                var deckCard = deckData.DeckCards.FirstOrDefault(card => card.Id == deleteCardId);
                if (deckCard != null) availableDecks.Add((DeckKind.Deck, deckCard));

                var handCard = deckData.HandCards.FirstOrDefault(card => card.Id == deleteCardId);
                if (handCard != null) availableDecks.Add((DeckKind.Hand, handCard));

                var graveCard = deckData.GraveCards.FirstOrDefault(card => card.Id == deleteCardId);
                if (graveCard != null) availableDecks.Add((DeckKind.Grave, graveCard));

                // 2단계: 사용 가능한 덱이 없으면 종료
                if (availableDecks.Count == 0)
                {
                    LogAbility($"삭제할 카드 ID {deleteCardId} 가 어떤 덱에도 없습니다.", LogType.Warning);
                    break;
                }

                // 3단계: 사용 가능한 덱들 중에서 랜덤 선택
                int randomIndex = Random.Range(0, availableDecks.Count);
                var selected = availableDecks[randomIndex];

                // 4단계: 선택된 덱에서 카드 삭제
                switch (selected.deckKind)
                {
                    case DeckKind.Deck:
                        deckData.RemoveFromDeck(selected.card);
                        break;

                    case DeckKind.Hand:
                        deckData.RemoveFromHand(selected.card);
                        // 핸드 카드는 UI 업데이트를 위해 HandDeck.RemoveHandCard 사용
                        HandDeck.Instance.RemoveHandCard(selected.card.Uid);
                        break;

                    case DeckKind.Grave:
                        deckData.RemoveFromGrave(selected.card);
                        break;
                }

                LogAbility($"{selected.deckKind}에서 카드 ID {deleteCardId}({selected.card.Uid}) 삭제", LogType.Log);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}