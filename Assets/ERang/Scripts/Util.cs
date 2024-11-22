using System;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;

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
        private static Random random = new();

        public static BaseCard MakeCard(CardData cardData)
        {
            BaseCard card = cardData.cardType switch
            {
                CardType.Creature => new CreatureCard(cardData),
                CardType.Monster => new CreatureCard(cardData),
                CardType.Building => new BuildingCard(cardData),
                CardType.Charm => new MagicCard(cardData),
                CardType.Curse => new MagicCard(cardData),
                CardType.Magic => new MagicCard(cardData),
                _ => new BaseCard(cardData),
            };

            return card;
        }

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

        public static string BoardSlotLog(BSlot boardSlot)
        {
            if (boardSlot == null)
                return "보드 슬롯 없음";

            return BoardSlotLog(boardSlot.SlotNum, boardSlot.Card?.CardType ?? CardType.None, boardSlot.Card?.Id ?? 0);
        }

        public static string BoardSlotLog(int slot, CardType cardType, int cardId)
        {
            return $"<color={(slot > 5 ? Colors.Red : Colors.Green)}>{slot}</color>번 슬롯 {(cardType == CardType.None ? "" : GetCardType(cardType))} 카드({(cardId != 0 ? cardId : "없음")})";
        }

        public static string CardLog(HCard card)
        {
            return $"{GetCardType(card.Card.CardType)} 카드({card.Card.Id})";
        }

        public static string CardLog(BaseCard card)
        {
            return $"{GetCardType(card.CardType)} 카드({card.Id})";
        }

        public static string AbilityLog(AbilityData ability)
        {
            return AbilityLog(ability.abilityType, ability.abilityId);
        }

        public static string AbilityLog(AbilityType abilityType, int abilityId)
        {
            return $"<color=#f4872e>{abilityType} 어빌리티({abilityId})</color>";
        }

        public static string BoardSlotNumersText(List<BSlot> boardSlots)
        {
            return $"<color=#ea4123>{string.Join(", ", boardSlots.Select(boardSlot => boardSlot.SlotNum).ToList())}</color>";
        }

        public static string NumbersText(List<int> numbers)
        {
            return $"<color=#ea4123>{string.Join(", ", numbers)}</color>";
        }

        public static string TargetText(AiDataTarget target)
        {
            return $"<color=#ed6ddc>{GetTarget(target)}</color>";
        }

        public static string RedText(object text)
        {
            return $"<color={Colors.Red}>{text}</color>";
        }

        public static string FloorText(int floor)
        {
            return $"<color={Colors.Green}>{floor}</color>층";
        }

        public static string MapLocationText(int locationId)
        {
            int floor = locationId / 100;
            int floorIndex = locationId % 100;

            return $"<color={Colors.Green}>{floor}</color>층 <color={Colors.Yellow}>{floorIndex}</color> 번째";
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

            if (changes.Count == 0)
                return $"changes.Count == 0 으로 변화 없음";

            return $"{string.Join(", ", changes.Select(change => $"{change.slot}번 슬롯 {GetCardType(change.cardType)} 카드 {statText} {(change.isAffect ? $"<color=#00ff00>{change.before} => {change.after}</color>" : "")} 효과 {(change.isAffect ? "적용" : "미적용")}. 변화량: {change.changeValue}"))}";
        }

        public static string LevelDataText(LevelData levelData)
        {
            List<(int, string, int)> cardDataList = new();

            for (int i = 0; i < levelData.cardIds.Count(); ++i)
            {
                int pos = i + 1;
                int cardId = levelData.cardIds[i];

                if (cardId == 0)
                {
                    cardDataList.Add((pos, "빈자리", 0));
                    continue;
                }

                CardData CardData = CardData.GetCardData(cardId);

                if (CardData == null)
                    cardDataList.Add((pos, $"카드 데이터 없음: {cardId}", cardId));
                else
                    cardDataList.Add((pos, CardData.nameDesc, CardData.card_id));
            }

            return $"Level ID: {levelData.levelId}. 등장 카드들 {string.Join(", ", cardDataList.Select(x => $"{x.Item1}: {x.Item2}({x.Item3})").ToList())}";
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

        private static string GetTarget(AiDataTarget target)
        {
            return target switch
            {
                AiDataTarget.NearEnemy => "가장 가까운 적",
                AiDataTarget.Enemy => "적",
                AiDataTarget.RandomEnemy => "임의의 적",
                AiDataTarget.RandomEnemyCreature => "임의의 적 크리쳐",
                AiDataTarget.AllEnemy => "모든 적",
                AiDataTarget.AllEnemyCreature => "모든 적 크리쳐",
                AiDataTarget.Friendly => "아군",
                AiDataTarget.AllFriendly => "모든 아군",
                AiDataTarget.AllFriendlyCreature => "모든 아군 크리쳐",
                AiDataTarget.Self => "자신",
                // AiDataTarget.Enemy1 => "적 1열",
                // AiDataTarget.Enemy2 => "적 2열",
                // AiDataTarget.Enemy3 => "적 3열",
                // AiDataTarget.Enemy4 => "적 마스터",
                // AiDataTarget.FriendlyCreature => "모든 아군 크리쳐",
                // AiDataTarget.EnemyCreature => "모든 적 크리쳐",
                // AiDataTarget.Card => "카드",
                _ => "없음",
            };
        }
    }
}