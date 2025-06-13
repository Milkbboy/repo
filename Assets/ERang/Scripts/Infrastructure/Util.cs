using System;
using System.Linq;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 컬러 상수 클래스
    /// </summary>
    public static class Colors
    {
        public static string Red { get; } = "#dd3333";
        public static string Green { get; } = "#78d641";
        public static string Blue { get; } = "#257dca";
        public static string Yellow { get; } = "#eeee22";
        public static string Orange { get; } = "#e78a27";
        public static string Puple { get; } = "#ed6ddc";
    }

    /// <summary>
    /// 유틸리티 클래스
    /// </summary>
    public static class Utils
    {
        private static System.Random random = new();

        public static void GetMasterText(int masterId, out string cardName, out string cardDesc, out string cardShortDesc)
        {
            MasterData masterData = MasterData.GetMasterData(masterId);

            cardName = TextData.GetKr(masterData.cardNameId);
            cardDesc = TextData.GetKr(masterData.cardDescId);
            cardShortDesc = TextData.GetKr(masterData.cardShortDescId);
        }

        public static void GetCardText(int cardId, out string cardName, out string cardDesc, out string cardShortDesc)
        {
            CardData cardData = CardData.GetCardData(cardId);

            if (cardData == null)
            {
                cardName = "카드 데이터 없음";
                cardDesc = "카드 데이터 없음";
                cardShortDesc = "카드 데이터 없음";
                return;
            }

            cardName = TextData.GetKr(cardData.cardNameId);
            cardDesc = TextData.GetKr(cardData.cardDescId);
            cardShortDesc = TextData.GetKr(cardData.cardShortDescId);
        }

        public static string GetAbilityIconText(int abilityId)
        {
            AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

            if (abilityData == null)
            {
                return "어빌리티 데이터 없음";
            }

            return TextData.GetKr(abilityData.skillIconDesc);
        }

        public static string GetCardDescText(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);

            return cardData == null ? "카드 데이터 없음" : TextData.GetKr(cardData.cardDescId);
        }

        public static string GetCardShortDescText(int cardId)
        {
            CardData cardData = CardData.GetCardData(cardId);

            return cardData == null ? "카드 데이터 없음" : TextData.GetKr(cardData.cardShortDescId);
        }

        /// <summary>
        /// Fisher-Yates shuffle (피셔 예이츠)알고리즘
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
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

        public static string FloorText(int floor)
        {
            return $"<color={Colors.Green}>{floor}</color>층";
        }

        // public static string DataNullText(string tableText, int dataId)
        // {
        //     return $"<color=#78d641>{tableText}</color> 테이블 {dataId} <color=red>데이터 없음</color>";
        // }

        // public static T CheckData<T>(Func<int, T> getDataFunc, string dataType, int dataId) where T : class
        // {
        //     T data = getDataFunc(dataId);

        //     if (data == null)
        //         Debug.LogError(DataNullText(dataType, dataId));

        //     return data;
        // }

        public static string MapLocationText(int locationId)
        {
            int floor = locationId / 100;
            int floorIndex = locationId % 100;

            return $"<color={Colors.Green}>{floor}</color>층 <color={Colors.Yellow}>{floorIndex}</color> 번째";
        }

        public static Texture2D LoadTexture(string texturePath)
        {
            Texture2D texture = Resources.Load<Texture2D>(texturePath);

            if (texture == null)
                Debug.LogError("Texture not found: " + texturePath);

            return texture;
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