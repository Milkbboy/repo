using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ERang.Data;
using ERang;
using Newtonsoft.Json;

public class RewardEvent : MonoBehaviour
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
        int masterId = PlayerDataManager.GetValue(PlayerDataKeys.MasterId);
        int levelId = PlayerDataManager.GetValue(PlayerDataKeys.LevelId);

        LevelData levelData = LevelGroupData.GetLevelData(levelId);

        if (levelData == null)
        {
            Debug.LogError($"RewardEvent - RewardCards - LevelData({levelId}) 테이블 데이터 없음");
            return;
        }

        // RewardSetData 에서 rewardCount 만큼 보상 타입을 랜덤으로 가져온다.
        List<(RewardType rewardType, CardGrade cardGrade)> rewardValues = RewardSetData.GetRewardValues(Constants.RewardCount);

        Debug.Log($"masterId: {masterId}, levelId: {levelId}, rewardId: {levelData.rewardId} Reward rewardValues: {string.Join(", ", rewardValues)}");

        RewardData rewardData = RewardData.rewardDataDict[(levelData.rewardId, masterId)];

        if (rewardData == null)
        {
            Debug.LogError($"rewardId({levelData.rewardId}) RewardData 테이블 데이터 없음");
            return;
        }

        // foreach (RewardCardData cardData in rewardData.rewardCardDatas)
        //     Debug.Log($"cardId: {cardData.cardId}, cardNameDesc: {cardData.cardNameDesc}, cardGrade: {cardData.cardGrade}, weightValue: {cardData.weightValue}, resultValue: {cardData.resultValue}");

        // 선택된 보상 카드 데이터. rewardType, RewardData 의 id, 카드 Id or 골드 or 체력
        List<(RewardType rewardType, int id, int value)> selectedRewardCardDatas = new();

        foreach ((RewardType rewardType, CardGrade cardGrade) in rewardValues)
        {
            // Debug.Log($"Processing reward type: {rewardType}, cardGrade: {cardGrade}");

            var availableRewards = GetAvailableRewards(rewardData, rewardType, cardGrade, selectedRewardCardDatas);
            if (availableRewards.Count == 0)
            {
                Debug.LogError($"No available rewards found for type {rewardType}, grade {cardGrade}");
                return;
            }

            var selectedReward = SelectRandomReward(availableRewards, rewardType);
            if (selectedReward != null)
            {
                selectedRewardCardDatas.Add(selectedReward.Value);
            }
            else
            {
                Debug.LogError($"Failed to select reward for type {rewardType}, grade {cardGrade}");
                return;
            }
        }

        // 선택된 보상 개수 확인
        if (selectedRewardCardDatas.Count != rewardValues.Count)
        {
            Debug.LogError($"Expected {rewardValues.Count} rewards but got {selectedRewardCardDatas.Count} rewards");
            return;
        }

        // 선택된 보상 출력
        foreach (var reward in selectedRewardCardDatas)
            Debug.Log($"Final selected reward: type={reward.rewardType}, id={reward.id}, value={reward.value}");

        // 화면 중앙에 카드를 배치
        List<(RewardType rewardType, int id)> rewards = selectedRewardCardDatas.Select(reward => (reward.rewardType, reward.value)).ToList();

        rewardUI.ShowRewardCards(rewards, OnSelectRewardCard);
    }

    private List<RewardCardData> GetAvailableRewards(RewardData rewardData, RewardType rewardType, CardGrade cardGrade,
        List<(RewardType rewardType, int id, int value)> selectedRewards)
    {
        return rewardData.rewardCardDatas.Where(cardData =>
        {
            bool typeMatch = cardData.rewardType == rewardType;
            bool gradeMatch = rewardType == RewardType.Card ? cardData.cardGrade == cardGrade : true;
            bool notSelected = !selectedRewards.Any(selected => selected.id == cardData.id);

            return typeMatch && gradeMatch && notSelected;
        }).ToList();
    }

    private (RewardType rewardType, int id, int value)? SelectRandomReward(List<RewardCardData> availableRewards, RewardType rewardType)
    {
        int totalResultValue = availableRewards.Sum(cardData => cardData.resultValue);
        int randomValue = Random.Range(0, totalResultValue);
        int sumValue = 0;

        foreach (var cardData in availableRewards)
        {
            sumValue += cardData.resultValue;
            if (randomValue < sumValue)
            {
                int rewardValue = GetRewardValue(cardData, rewardType);
                return (rewardType, cardData.id, rewardValue);
            }
        }

        return null;
    }

    private int GetRewardValue(RewardCardData cardData, RewardType rewardType)
    {
        return rewardType switch
        {
            RewardType.Card => cardData.cardId,
            RewardType.Gold => Random.Range(cardData.goldMin, cardData.goldMax),
            RewardType.HP => Random.Range(cardData.hpMin, cardData.hpMax),
            _ => 0
        };
    }

    public void Confirm()
    {
        if (selectedCard == null)
        {
            Debug.LogError("선택된 카드가 없습니다.");
            return;
        }

        if (selectedCard.Card is HpCard)
        {
            OnClickNextScene?.Invoke();

            if (selectedCard.Card is HpCard hpCard)
            {
                Debug.Log($"Selected Hp Card: ${selectedCard.Card.Hp}");
                Player.Instance.RecoverHp(hpCard.Hp);
                PlayerDataManager.SetValue(PlayerDataKeys.MasterHp, Player.Instance.Hp);
            }
            else
            {
                Debug.LogError($"Selected Hp Card is not HpCard class");
            }
        }
        else if (selectedCard.Card is GoldCard)
        {

            OnClickNextScene?.Invoke();

            if (selectedCard.Card is GoldCard goldCard)
            {
                Debug.Log($"Selected Gold Card: ${goldCard.Gold}");
                Player.Instance.AddGold(goldCard.Gold);
                PlayerDataManager.SetValue(PlayerDataKeys.MasterGold, Player.Instance.Gold);
            }
            else
            {
                Debug.LogError($"Selected Gold Card is not GoldCard class");
            }
        }
        else
        {
            Debug.Log($"Selected RewardCardId: {selectedCard.Card.Id}");
            Debug.Log($"before cards {Player.Instance.AllCardCount}. {string.Join(", ", Player.Instance.AllCards.Select(card => card.Id))}");

            Player.Instance.AddCard(selectedCard.Card.Id);

            PlayerDataManager.SetValue(PlayerDataKeys.MasterCards, JsonConvert.SerializeObject(Player.Instance.AllCards.Select(card => card.Id)));

            selectedCard.DiscardAnimation(deckPosition);

            Debug.Log($"after cards {Player.Instance.AllCardCount}. {string.Join(", ", Player.Instance.AllCards.Select(card => card.Id))}");

            OnClickNextScene?.Invoke();
        }
    }

    private void OnSelectRewardCard(RewardCard rewardCard)
    {
        if (selectedCard != rewardCard)
            selectedCard?.SetScaleFixed(false);

        selectedCard = rewardCard;
    }
}
