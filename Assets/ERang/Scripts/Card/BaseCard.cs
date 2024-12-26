using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    // 기본 카드
    [Serializable]
    public class BaseCard : ICard
    {
        public string Uid { get; set; }
        public int Id { get; set; }
        public CardType CardType { get; set; }
        public int AiGroupId { get; set; }
        public int AiGroupIndex { get; set; }
        public bool InUse { get; set; }
        public bool IsExtinction { get; set; }
        public Texture2D CardImage { get; set; }
        public List<CardAbility> Abilities { get => abilities; set => abilities = value; }

        public string LogText => Utils.CardLog(this);

        private List<CardAbility> abilities = new();

        public virtual int Hp { get; set; }
        public virtual int Def { get; set; }
        public virtual int Mana { get; set; }
        public virtual int Atk { get; set; }

        public virtual void TakeDamage(int amount) { }
        public virtual void RestoreHealth(int amount) { }
        public virtual void IncreaseDefense(int amount) { }
        public virtual void DecreaseDefense(int amount) { }

        private Dictionary<int, int> abilityOrder = new();

        public BaseCard()
        {
        }

        public BaseCard(CardData cardData)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            AiGroupIndex = 0;
            CardImage = cardData.GetCardTexture();
        }

        public BaseCard(int cardId, CardType cardType, int aiGroupId, Texture2D cardImage)
        {
            Uid = Utils.GenerateShortUniqueID();
            Id = cardId;
            CardType = cardType;
            AiGroupId = aiGroupId;
            AiGroupIndex = 0;
            CardImage = cardImage;
        }

        public void UpdateCardData(CardData cardData)
        {
            Id = cardData.card_id;
            CardType = cardData.cardType;
            InUse = cardData.inUse;
            IsExtinction = cardData.extinction;
            AiGroupId = cardData.aiGroup_id;
            CardImage = cardData.GetCardTexture();
        }

        public void UpdateCardData(int cardId, CardType cardType, bool inUse, int aiGroupId, Texture2D cardImage)
        {
            Id = cardId;
            CardType = cardType;
            InUse = inUse;
            AiGroupId = aiGroupId;
            AiGroupIndex = 0;
            CardImage = cardImage;
        }

        public void DecreaseAbilityDuration()
        {
            Debug.Log("-- duration 감소 전 --");

            var groupedAndSortedAbilities = abilities
                .GroupBy(a => a.abilityId)
                .Select(g => new
                {
                    abilityId = g.Key,
                    Abilities = g.OrderBy(a => a.createdDt).ToList()
                })
                .ToList();

            // 결과 출력 (디버그용)
            foreach (var group in groupedAndSortedAbilities)
            {
                Debug.Log($"AbilityId: {group.abilityId}");

                foreach (var ability in group.Abilities)
                {
                    Debug.Log($"{ability.abilityUid}: StartTurn: {ability.startTurn}, Duration: {ability.duration} createdDt: {ability.createdDt}");
                }
            }

            // 각 어빌리티의 그룹 첫번째 어빌리티의 Duration 을 감소하고 0이 되면 제거
            foreach (var group in groupedAndSortedAbilities)
            {
                var firstAbility = group.Abilities.First();
                firstAbility.duration--;

                // if (firstAbility.duration <= 0)
                // {
                //     abilities.RemoveAll(a => a.abilityUid == firstAbility.abilityUid);

                //     Debug.Log($"{LogText} 해제된 어빌리티 {firstAbility.abilityUid} - DecreaseAbilityDuration");
                // }
            }

            Debug.Log("-- duration 감소 후 --");
            // 결과 출력 (디버그용)
            foreach (CardAbility ability in abilities)
            {
                Debug.Log($"{ability.abilityUid}: StartTurn: {ability.startTurn}, Duration: {ability.duration} createdDt: {ability.createdDt}");
            }
        }

        public void AddAbility(CardAbility ability, int turnCount)
        {
            // 아이디가 중복되는 어빌리티면 카운트 증가
            if (abilityOrder.ContainsKey(ability.abilityId))
            {
                abilityOrder[ability.abilityId]++;
                ability.abilityUid = $"{ability.abilityId}_{turnCount}_{abilityOrder[ability.abilityId]}";
            }
            else
            {
                abilityOrder.Add(ability.abilityId, 1);
                ability.abilityUid = $"{ability.abilityId}_1_1";
            }

            abilities.Add(ability);
        }

        public int GetBuffCount()
        {
            return abilities.Count(ability => ability.aiType == AiDataType.Buff);
        }

        public int GetDeBuffCount()
        {
            return abilities.Count(ability => ability.aiType == AiDataType.Debuff);
        }
    }
}