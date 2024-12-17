using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;
using ERang;

public class Reward : MonoBehaviour
{
    public Transform deckPosition;

    public UnityAction OnClickNextScene;

    private RewardCard selectedCard;
    private RewardUI rewardUI;

    void Awake()
    {
        rewardUI = GetComponent<RewardUI>();
    }

    void Start()
    {
        RewardCards();
    }

    public void RewardCards()
    {
        int masterId = PlayerPrefsUtility.GetInt("MasterId", 1001);
        int levelId = PlayerPrefsUtility.GetInt("LevelId", 100100101);

        PlayerPrefsUtility.SetInt("MasterId", masterId);
        PlayerPrefsUtility.SetInt("LevelId", levelId);

        LevelData levelData = LevelGroupData.GetLevelData(levelId);

        if (levelData == null)
        {
            Debug.LogError($"levelId({levelId}) LevelGroupData {Utils.RedText("테이블 데이터 없음")}");
            return;
        }

        int cardCount = Constants.RewardCardCount;

        List<CardGrade> cardGrades = new();

        for (int i = 0; i < cardCount; ++i)
        {
            CardGrade cardGrade = RewardSetData.PickupCardGrade();
            cardGrades.Add(cardGrade);
        }

        Debug.Log($"masterId: {masterId}, levelId: {levelId}, rewardId: {levelData.rewardId} Reward cardGrades: {string.Join(", ", cardGrades)}");

        RewardData rewardData = RewardData.rewardDataDict[(levelData.rewardId, masterId)];

        if (rewardData == null)
        {
            Debug.LogError($"rewardId({levelData.rewardId}) RewardData {Utils.RedText("테이블 데이터 없음")}");
            return;
        }

        List<int> selectedCardIds = new();

        foreach (CardGrade cardGrade in cardGrades)
        {
            var rewardCardDatas = rewardData.rewardCardDatas.Where(card => card.cardGrade == cardGrade && !selectedCardIds.Contains(card.cardId)).ToList();

            if (rewardCardDatas.Count > 0)
            {
                // 가중치 합산
                int totalResultValue = rewardCardDatas.Sum(card => card.resultValue);

                // 가중치 랜덤 선택
                int randomValue = Random.Range(0, totalResultValue);

                // 랜덤 값에 해당하는 카드 선택
                int sumValue = 0;
                RewardCardData selectedCardData = null;

                foreach (var cardData in rewardCardDatas)
                {
                    sumValue += cardData.resultValue;

                    if (randomValue < sumValue)
                    {
                        selectedCardData = cardData;
                        break;
                    }
                }

                if (selectedCardData != null)
                {
                    selectedCardIds.Add(selectedCardData.cardId);

                    // Debug.Log($"selected cardId: {selectedCardData.cardId}, cardNameDesc: {selectedCardData.cardNameDesc}, cardGrade: {selectedCardData.cardGrade}, weightValue: {selectedCardData.weightValue}, resultValue: {selectedCardData.resultValue}");
                }
            }
        }

        // foreach(RewardCard card in rewardData.rewardCards)
        //     Debug.Log($"cardId: {card.cardId}, cardNameDesc: {card.cardNameDesc}, cardGrade: {card.cardGrade}, weightValue: {card.weightValue}, resultValue: {card.resultValue}");

        // 화면 중앙에 카드를 배치
        rewardUI.ShowRewardCards(selectedCardIds, OnSelectRewardCard);
    }

    public void Confirm()
    {
        if (selectedCard == null)
        {
            Debug.LogError("선택된 카드가 없습니다.");
            return;
        }

        Debug.Log($"Selected RewardCard: {selectedCard.Card.Id}");
        Debug.Log($"before cards {Player.Instance.AllCardCount}. {string.Join(", ", Player.Instance.AllCards.Select(card => card.Id))}");

        Player.Instance.AddCard(selectedCard.Card.Id);

        selectedCard.DiscardAnimation(deckPosition);

        Debug.Log($"after cards {Player.Instance.AllCardCount}. {string.Join(", ", Player.Instance.AllCards.Select(card => card.Id))}");

        OnClickNextScene?.Invoke();
    }

    private void OnSelectRewardCard(RewardCard rewardCard)
    {
        if (selectedCard != rewardCard)
            selectedCard?.SetScaleFixed(false);

        selectedCard = rewardCard;
    }
}
