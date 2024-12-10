using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;
using ERang;

public class Reward : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform deckPosition;

    public UnityAction OnClickNextScene;

    private RewardCard selectedCard;
    private List<RewardCard> rewardCards = new();

    void Start()
    {
        RewardCards();
    }

    public void RewardCards()
    {
        int masterId = PlayerPrefsUtility.GetInt("MasterId", 0);
        int levelId = PlayerPrefsUtility.GetInt("LevelId", 0);

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

        List<RewardCardData> selectedCards = new();
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
                    selectedCards.Add(selectedCardData);
                    selectedCardIds.Add(selectedCardData.cardId);
                }
            }
        }

        // foreach(RewardCard card in rewardData.rewardCards)
        //     Debug.Log($"cardId: {card.cardId}, cardNameDesc: {card.cardNameDesc}, cardGrade: {card.cardGrade}, weightValue: {card.weightValue}, resultValue: {card.resultValue}");

        // foreach (RewardCard card in selectedCards)
        //     Debug.Log($"selected cardId: {card.cardId}, cardNameDesc: {card.cardNameDesc}, cardGrade: {card.cardGrade}, weightValue: {card.weightValue}, resultValue: {card.resultValue}");

        // 화면 중앙에 카드를 배치
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter.z = 0; // Z축 위치를 0으로 설정하여 2D 평면에 배치

        float cardSpacing = 2f;
        float startX = worldCenter.x - (selectedCards.Count - 1) * cardSpacing / 2;

        for (int i = 0; i < selectedCardIds.Count; ++i)
        {
            CardData cardData = CardData.GetCardData(selectedCardIds[i]);

            if (cardData == null)
            {
                Debug.LogError($"cardId({selectedCardIds[i]}) CardData {Utils.RedText("테이블 데이터 없음")}");
                continue;
            }

            BaseCard card = Utils.MakeCard(cardData);

            Vector3 cardPosition = new(startX + i * cardSpacing, worldCenter.y, worldCenter.z);

            GameObject cardObject = Instantiate(cardPrefab, cardPosition, transform.rotation, transform);
            cardObject.name = $"RewardCard_{card.Id}";

            RewardCard rewardCard = cardObject.GetComponent<RewardCard>();
            rewardCard.SetCard(card);
            rewardCard.OnClick += OnRewardCardClicked;

            rewardCards.Add(rewardCard);
        }
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

    private void OnRewardCardClicked(RewardCard rewardCard)
    {
        if (selectedCard != rewardCard)
            selectedCard?.SetScaleFixed(false);

        selectedCard = rewardCard;
    }
}
