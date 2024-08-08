using System;
using System.Linq;

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

        public static CardType ConvertCardType(string cardType)
        {
            switch (cardType)
            {
                case "Magic":
                    return CardType.Magic;
                case "Individuality":
                    return CardType.Individuality;
                case "Creature":
                    return CardType.Creature;
                case "Building":
                    return CardType.Building;
                case "Charm":
                    return CardType.Charm;
                case "Curse":
                    return CardType.Curse;
                default:
                    return CardType.None;
            }
        }
    }
}