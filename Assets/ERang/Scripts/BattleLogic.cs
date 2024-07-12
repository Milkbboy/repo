using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        HandDeck handDeck;

        public int turn = 0;

        // Start is called before the first frame update
        void Start()
        {
            handDeck = GameObject.Find("HandDeck").GetComponent<HandDeck>();

            // 처음 시작이면 플레이서 startDeck 설정
            if (turn == 0)
            {
                ChampionData championData = ChampionData.GetChampionData("champion_a");

                if (championData == null)
                {
                    Debug.LogError("ChampionData is null");
                    return;
                }

                championData.startCardUids.ForEach(uid =>
                {
                    CardData cardData = CardData.GetCardData(uid);

                    if (cardData)
                    {
                        handDeck.handCards.Add(cardData);
                    }
                    else
                    {
                        Debug.LogError($"CardData {uid} is null");
                    }
                });
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}