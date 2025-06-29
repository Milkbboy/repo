using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    /// <summary>
    /// 등장하는 모든 카드의 데이터 시트
    /// </summary>
    public class CardData
    {
        public int card_id; // 카드의 Id 값
        public string nameDesc; // 카드의 실제 이름
        public string cardNameId; // 카드의 실제 이름 String이 들어간 데이터 Id
        public string cardDescId; // 카드에 대한 설명을 담은 String이 들어간 데이터 Id
        public string cardShortDescId; // 카드 간단 설명
        public CardType cardType; // 해당 카드의 타입을 입력 (타입은 초안에서 총 6개의 타입으로 구분)
        public CardGrade cardGrade; // 해당 카드의 등급을 입력 (등급은 초안에서 총 3개의 등급으로 구분)
        public List<int> aiGroup_ids = new(); // 해당 카드가 가지고 있는 Ai 그룹의 Id 값
        public int creatureAI_id; // Creature의 공격 Ai id (근접, 원거리 등)
        public int costMana; // 소환에 필요한 마나
        public int costGold; // 소환에 필요한 골드
        public int hp; // 체력 값
        public int atk; // 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int def; // 초기 방어력 값
        public string ability_id; // 해당 카드가 가진 어빌리티 값으로 복수 지정이 가능하다.
        public bool inUse; // 해당 카드가 사용 여부
        public bool extinction; // Bool 값으로 True 시 해당 카드는 사용 시 해당 전투에서 카드 덱에서 삭제된다.
        public bool completeExtinction; // Extinction 값이 TRUE일 경우에만 동작한다. 해당 값이 TRUE일 경우 해당 카드는 사용 시 다음 전투에서도 삭제된다. (1회성 카드)
        public int level; // 기본 레벨 값
        public string level_1_img; // 크리쳐의 경우에만 입력. 해당 크리쳐의 1~2레벨 시 이미지 링크
        public string level_3_img; // 크리쳐의 경우에만 입력. 해당 크리쳐의 3~4레벨 시 이미지 링크
        public string level_5_img; // 크리쳐의 경우에만 입력. 해당 크리쳐의 5레벨 시 이미지 링크
        public string handStart_Ability; // 핸드에 해당 카드가 들어 온 순간 발동되는 어빌티리 (Ex. 전쟁의 가호 : 모든 크리쳐 공격력 1턴 동안 1 증가)
        public string handEnd_Ability; // 해당 카드가 핸드에서 무덤으로 들어가는 순간 발동되는 어빌티리 (Ex. 부패 : 핸드에 남은 카드 개수 당 2의 데미지를 마왕이 받는다)
        public int Owner; // 개성 카드에만 입력. 해당 카드를 소유한 마스터의 Id를 입력
        public List<int> abilityIds = new();

        [Header("Display")]
        public Texture2D cardTexture;

        public static List<CardData> card_list = new();
        public static Dictionary<int, CardData> card_dict = new();

        public void Initialize(CardDataEntity cardEntity)
        {
            card_id = cardEntity.Card_Id;
            nameDesc = cardEntity.NameDesc;
            cardNameId = cardEntity.CardNameDesc_Id;
            cardDescId = cardEntity.CardDesc_Id;
            cardShortDescId = cardEntity.CardShortDesc_Id;
            cardGrade = ConvertCardGrade(cardEntity.Grade);
            cardType = ConvertCardType(cardEntity.CardType);
            if (!string.IsNullOrEmpty(cardEntity.AiGroup_ids))
            {
                aiGroup_ids = Utils.ParseIntArray(cardEntity.AiGroup_ids).ToList();
            }
            costMana = cardEntity.CostMana;
            costGold = cardEntity.CostGold;
            hp = cardEntity.Hp;
            atk = cardEntity.Atk;
            def = cardEntity.Def;
            inUse = cardEntity.InUse;
            extinction = cardEntity.Extinction;
            completeExtinction = cardEntity.CompleteExtinction;
            level = cardEntity.Level;
            level_1_img = cardEntity.Level_1_img;
            level_3_img = cardEntity.Level_3_img;
            level_5_img = cardEntity.Level_5_img;
            handStart_Ability = cardEntity.HandStart_Ability;
            handEnd_Ability = cardEntity.HandEnd_Ability;
            Owner = cardEntity.Owner;

            // 이미지 로드 및 cardImange 에 할당
            string texturePath = $"Textures/{level_1_img}";
            cardTexture = Resources.Load<Texture2D>(texturePath);

            if (cardTexture == null)
                Debug.LogError("Card Texture not found: " + texturePath);
        }

        public void Initialize(MonsterCardDataEntity cardEntity)
        {
            card_id = cardEntity.Card_Id;
            nameDesc = cardEntity.NameDesc;
            cardNameId = cardEntity.CardNameDesc_Id;
            cardDescId = cardEntity.CardDesc_Id;
            cardShortDescId = cardEntity.CardShortDesc_Id;
            cardType = ConvertCardType(cardEntity.CardType);
            if (!string.IsNullOrEmpty(cardEntity.AiGroup_ids))
            {
                aiGroup_ids = Utils.ParseIntArray(cardEntity.AiGroup_ids).ToList();
            }
            costMana = cardEntity.CostMana;
            hp = cardEntity.Hp;
            atk = cardEntity.Atk;
            def = cardEntity.Def;
            extinction = cardEntity.Extinction;
            completeExtinction = cardEntity.CompleteExtinction;
            level = cardEntity.Level;
            level_1_img = cardEntity.Level_1_img;
            level_3_img = cardEntity.Level_3_img;
            level_5_img = cardEntity.Level_5_img;
            handStart_Ability = cardEntity.HandStart_Ability;
            handEnd_Ability = cardEntity.HandEnd_Ability;
            Owner = cardEntity.Owner;

            // 이미지 로드 및 cardImange 에 할당
            string texturePath = $"Textures/{level_1_img}";
            cardTexture = Resources.Load<Texture2D>(texturePath);

            if (cardTexture == null)
                Debug.LogError("Card Texture not found: " + texturePath);
        }

        public static void Load(string path = "")
        {
            // 엑셀로 생성된 ScriptableObject 로드
            CardDataTable cardDataTable = Resources.Load<CardDataTable>(path);

            if (cardDataTable == null)
            {
                Debug.LogError("CardDataTable not found");
                return;
            }

            foreach (var cardEntity in cardDataTable.items)
            {
                if (card_dict.ContainsKey(cardEntity.Card_Id))
                    continue;

                CardData cardData = new();

                cardData.Initialize(cardEntity);

                card_list.Add(cardData);
                card_dict.Add(cardData.card_id, cardData);
            }
        }

        public static CardData GetGoldCardData()
        {
            CardData cardData = new();

            cardData.card_id = 0;
            cardData.cardType = CardType.Gold;
            return cardData;
        }

        public static CardData GetHpCardData()
        {
            CardData cardData = new();

            cardData.card_id = 0;
            cardData.cardType = CardType.Hp;
            return cardData;
        }
        public static CardData GetCardData(int card_id)
        {
            if (!card_dict.TryGetValue(card_id, out CardData cardData))
            {
                Debug.LogError($"CardData {card_id} not found");
                return null;
            }
            return cardData;
        }

        /// <summary>
        /// 카드 id, name을 반환
        /// </summary>
        /// <returns></returns>
        public static List<(int, string)> GetCardIdNames()
        {
            List<(int, string)> cardIds = new();

            foreach (var card in card_list)
            {
                cardIds.Add((card.card_id, card.nameDesc));
            }

            return cardIds;
        }

        public Texture2D GetCardTexture()
        {
            return cardTexture;
        }

        public CardType ConvertCardType(string cardType)
        {
            return cardType switch
            {
                "Magic" => CardType.Magic,
                "Individuality" => CardType.Individuality,
                "Creature" => CardType.Creature,
                "Building" => CardType.Building,
                "Charm" => CardType.Charm,
                "Curse" => CardType.Curse,
                "Monster" => CardType.Monster,
                _ => CardType.None,
            };
        }

        private CardGrade ConvertCardGrade(string grade)
        {
            return grade switch
            {
                "Common" => CardGrade.Common,
                "Rare" => CardGrade.Rare,
                "Legendary" => CardGrade.Legendary,
                _ => CardGrade.None,
            };
        }
    }
}