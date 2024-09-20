using System;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;

namespace ERang
{
    public static class Utils
    {
        public static string RedText(object text)
        {
            return $"<color=#ea4123>{text}</color>";
        }

        public static int[] ParseIntArray(string intArray)
        {
            return intArray.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => int.Parse(s.Trim()))
                          .ToArray();
        }

        public static string GenerateShortUniqueID(int length = 8)
        {
            Guid guid = Guid.NewGuid();
            string base64 = Convert.ToBase64String(guid.ToByteArray());
            // Base64 인코딩 결과에서 불필요한 문자를 제거하고 원하는 길이로 자름
            string shortID = base64.Replace("=", "").Replace("+", "").Replace("/", "").Substring(0, length);
            return shortID;
        }

        public static string BoardSlotLog(BoardSlot boardSlot)
        {
            if (boardSlot == null)
                return "보드 슬롯 없음";

            return BoardSlotLog(boardSlot.Slot, boardSlot.Card.Type, boardSlot.Card?.Id ?? 0);
        }

        public static string BoardSlotLog(int slot, CardType cardType, int cardId)
        {
            return $"{slot}번 슬롯 {(cardType == CardType.None ? "" : GetCardType(cardType))} 카드({(cardId != 0 ? cardId : "없음")})";
        }

        public static string CardLog(Card card)
        {
            return $"{GetCardType(card.Type)} 카드({card.Id})";
        }

        public static string AbilityLog(Ability ability)
        {
            return AbilityLog(ability.abilityType, ability.abilityId);
        }

        public static string AbilityLog(AbilityData ability)
        {
            return AbilityLog(ability.abilityType, ability.abilityId);
        }

        public static string AbilityLog(AbilityType abilityType, int abilityId)
        {
            return $"<color=#f4872e>{abilityType} 어빌리티({abilityId})</color>";
        }

        public static string BoardSlotNumersText(List<BoardSlot> boardSlots)
        {
            return $"<color=#ea4123>{string.Join(", ", boardSlots.Select(boardSlot => boardSlot.Slot).ToList())}</color>";
        }

        public static string NumbersText(List<int> numbers)
        {
            return $"<color=#ea4123>{string.Join(", ", numbers)}</color>";
        }

        public static string StatChangesText(AbilityType abilityType, List<(bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)> changes)
        {
            string statText = abilityType switch
            {
                AbilityType.Damage => "hp",
                AbilityType.Heal => "hp",
                AbilityType.ChargeDamage => "hp",
                AbilityType.AtkUp => "atk",
                AbilityType.DefUp => "def",
                AbilityType.BrokenDef => "def",
                _ => "스탯",
            };

            return $"{string.Join(", ", changes.Select(change => $"{change.slot}번 슬롯 {GetCardType(change.cardType)} 카드 {statText} {(change.isAffect ? $"<color=#00ff00>{change.before} => {change.after}</color>" : "")} 효과 {(change.isAffect ? "적용" : "미적용")}. 변화량: {change.changeValue}"))}";
        }

        private static string GetCardType(CardType cardType)
        {
            return cardType switch
            {
                CardType.Master => "마스터",
                CardType.Magic => "마법",
                CardType.Individuality => "전용 마법",
                CardType.Creature => "크리쳐",
                CardType.Building => "건물",
                CardType.Charm => "축복",
                CardType.Curse => "저주",
                CardType.Monster => "몬스터",
                CardType.EnemyMaster => "적 마스터",
                _ => "없음",
            };
        }
    }
}