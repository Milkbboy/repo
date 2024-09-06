using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ERang
{
    public static class Utils
    {
        public static int[] ParseIntArray(string intArray)
        {
            return intArray.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
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
            return $"{boardSlot.Slot}번 슬롯 {GetCardType(boardSlot.CardType)} 카드({boardSlot?.Card?.id ?? 0})";
        }

        public static string CardLog(Card card)
        {
            return $"{GetCardType(card.type)} 카드({card.id})";
        }

        public static string BoardSlotNumersText(List<BoardSlot> boardSlots)
        {
            return $"<color=#ea4123>{string.Join(", ", boardSlots.Select(boardSlot => boardSlot.Slot).ToList())}</color>";
        }

        public static string NumbersText(List<int> numbers)
        {
            return $"<color=#ea4123>{string.Join(", ", numbers)}</color>";
        }

        public static string StatChangesText(string statText, List<(bool isAffect, int slot, int cardId, int before, int after)> changes)
        {
            return $"{string.Join(", ", changes.Select(change => $"{change.slot}번 슬롯 {statText} {(change.isAffect ? $"<color=#00ff00>{change.before} => {change.after}</color>" : "")} 효과 {(change.isAffect ? "적용" : "미적용")}"))}";
        }

        private static string GetCardType(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Master: return "마스터";
                case CardType.Magic: return "마법";
                case CardType.Individuality: return "전용 마법";
                case CardType.Creature: return "크리쳐";
                case CardType.Building: return "건물";
                case CardType.Charm: return "축복";
                case CardType.Curse: return "저주";
                case CardType.Monster: return "몬스터";
                case CardType.EnemyMaster: return "적 마스터";
                default: return "없음";
            }
        }
    }
}