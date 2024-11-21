using System.Collections.Generic;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class Enemy
    {
        public static Enemy Instance { get; private set; }

        public int enemyId;
        public int hp;
        public int maxHp;
        public int atk;
        public int def;
        public int Hp => hp;
        public int MaxHp => maxHp;
        public int Atk => atk;
        public int Def => def;

        // 카드 슬롯, 카드
        public List<(int, HCard)> monsterCards = new();

        public Enemy(int enemyId)
        {
            Instance = this;

            MasterData enemyData = MasterData.GetMasterData(enemyId);

            this.enemyId = enemyData.master_Id;
            maxHp = hp = enemyData.hp;
            atk = enemyData.atk;
            def = enemyData.def;

            for (int i = 0; i < enemyData.startCardIds.Count; ++i)
            {
                int cardId = enemyData.startCardIds[i];

                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"MonsterCardData 테이블에 카드({cardId}) 없음");
                    continue;
                }

                // HCard card = new(cardData);
                // monsterCards.Add((i, card));
            }
        }

        public Enemy(List<int> cardIds)
        {
            for (int i = 0; i < cardIds.Count; ++i)
            {
                int cardId = cardIds[i];

                if (cardId == 0)
                {
                    monsterCards.Add((i, null));
                    continue;
                }

                CardData cardData = CardData.GetCardData(cardId);

                if (cardData == null)
                {
                    Debug.LogError($"MonsterCardData 테이블에 카드({cardId}) 없음");
                    continue;
                }

                // Card card = new(cardData);
                // monsterCards.Add((i, card));
            }
        }

        // public HCard GetMonsterCard(string cardUid)
        // {
        //     var cardTuple = monsterCards.Find(card => card.Item2 != null && card.Item2.Uid == cardUid);

        //     return cardTuple.Item2;
        // }

        // public void RemoveMonsterCard(string cardUid)
        // {
        //     var cardTuple = monsterCards.Find(card => card.Item2 != null && card.Item2.Uid == cardUid);

        //     if (cardTuple.Item2 != null)
        //     {
        //         monsterCards.Remove(cardTuple);
        //     }
        // }
    }
}